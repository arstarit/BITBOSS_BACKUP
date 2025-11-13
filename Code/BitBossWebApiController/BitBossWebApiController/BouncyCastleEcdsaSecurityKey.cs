using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text.RegularExpressions;

using Org.BouncyCastle.Crypto.Generators;
using System.IO;

namespace BitBossWebApiController
{

    public class CustomSignatureProvider : SignatureProvider
    {

        public CustomSignatureProvider(BouncyCastleEcdsaSecurityKey key, string algorithm
            ) : base(key, algorithm) {
            // Console.WriteLine("here!!!");
        }

        protected override void Dispose(bool disposing) { }

        public IConfiguration Configuration { get; }

        public static byte[] PadBytes(byte[] input, int length) {
            var newArray = new byte[length];

            var startAt = newArray.Length - input.Length;
            Array.Copy(input, 0, newArray, startAt, input.Length);
            return newArray;
        }
        public override byte[] Sign(byte[] input) {
            var ecDsaSigner = new ECDsaSigner();
            BouncyCastleEcdsaSecurityKey key = Key as BouncyCastleEcdsaSecurityKey;
            ecDsaSigner.Init(true, key.KeyParameters);

            byte[] hashedInput;
            using (var hasher = SHA256.Create()) {
                hashedInput = hasher.ComputeHash(input);
            }

            var output = ecDsaSigner.GenerateSignature(hashedInput);

            var r = PadBytes(output[0].ToByteArrayUnsigned(), 32);
            var s = PadBytes(output[1].ToByteArrayUnsigned(), 32);

            // Console.WriteLine($"sig lengths: {r.Length} + {s.Length}");
            var signature = new byte[r.Length + s.Length];
            r.CopyTo(signature, 0);
            s.CopyTo(signature, r.Length);

            return signature;
        }

        public static byte[] FromHexString(string hex)
        {
            var numberChars = hex.Length;
            var hexAsBytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                hexAsBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return hexAsBytes;
        }

        // GetSecurityKey, dado una string 
        public static BouncyCastleEcdsaSecurityKey GetSecurityKey(string keyStr)
        {
            var decompressed = Cryptography.ECDSA.Secp256K1Manager
                .PublicKeyDecompress(CustomSignatureProvider.FromHexString(keyStr));

            byte[] x = decompressed.Skip(1).Take(32).ToArray();
            byte[] y = decompressed.Skip(33).ToArray();

            X9ECParameters secp256k1 = ECNamedCurveTable.GetByName("secp256k1");
            ECDomainParameters domainParams = new ECDomainParameters(
                secp256k1.Curve, secp256k1.G, secp256k1.N, secp256k1.H, secp256k1.GetSeed());
            var point = secp256k1.Curve.CreatePoint(
                new BigInteger(1, x),
                new BigInteger(1, y));

            return new BouncyCastleEcdsaSecurityKey(
                new ECPublicKeyParameters(point, domainParams))
            { };
        }

        public static JwtSecurityToken ParseJWT(string jwt) {
            // Me creo un token con una firma descartable con propósito de parseo usando las librerías third-party, 
            // NO USO esta firma para la validación sino que valido el signature que llega como parámetro en el método Verify
            // I create a token with a disposable signature for parsing purposes using the third-party libraries,
            // I DO NOT USE this signature for validation but I validate the signature that arrives as a parameter in the Verify method
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(jwt);
        }

        public override bool Verify(byte[] input, byte[] signature)
        {
            try {
                // Console.WriteLine("Verify");
                var ecDsaSigner = new ECDsaSigner();

                string headerAndpayload = Encoding.UTF8.GetString(input)
                    +"."+ "--------------------------------------------------------------------------------------";
                var handler = new JwtSecurityTokenHandler();
                var jsonToken =  handler.ReadJwtToken(headerAndpayload);
                // Console.WriteLine($"jsonToken: {jsonToken}");
                var iss = jsonToken.Issuer;

                // Comparo el Issuer con el guardado en las settings, y si coinciden, procedo a verificar el hash
                if (iss == Startup.Configuration["Jwt:CMPubKey"])
                {
                    // La clave teniendo en cuenta el iss
                    // BouncyCastleEcdsaSecurityKey key = GetSecurityKey(iss);
                    BouncyCastleEcdsaSecurityKey key = Key as BouncyCastleEcdsaSecurityKey;
                    ecDsaSigner.Init(false, key.KeyParameters);

                    // el input codificado como SHA256
                    byte[] hashedInput;
                    using (var hasher = SHA256.Create())
                    {
                        hashedInput = hasher.ComputeHash(input);
                    }

                    // 
                    BigInteger r = new BigInteger(1, signature.Take(32).ToArray());
                    BigInteger s = new BigInteger(1, signature.Skip(32).ToArray());

                    var ret = ecDsaSigner.VerifySignature(hashedInput, r, s);
                    // Console.WriteLine($"is valid: {ret}");
                    return ret;
                }
                else
                {
                    Console.WriteLine($"bad iss {iss} v {Startup.Configuration["Jwt:CMPubKey"]}");
                    return false;
                }
            } catch (Exception e) {
                Console.WriteLine("Exception", e);
                Console.WriteLine(e);
                return false;
            }
        }
    }

    public class CustomCryptoProvider : ICryptoProvider
    {
        public bool IsSupportedAlgorithm(string algorithm, params object[] args)
            => algorithm == "ES256K";

        public object Create(string algorithm, params object[] args)
        {
            // Console.WriteLine($"Create: {algorithm}");
            if (algorithm == "ES256K"
                && args[0] is BouncyCastleEcdsaSecurityKey key)
            {
                return new CustomSignatureProvider(key, algorithm);
            }

            Console.WriteLine($"Create: NotSupportedException");
            throw new NotSupportedException();
        }

        public void Release(object cryptoInstance)
        {
            if (cryptoInstance is IDisposable disposableObject)
                disposableObject.Dispose();
        }
    }

    public class BouncyCastleEcdsaSecurityKey : AsymmetricSecurityKey
    {
        public BouncyCastleEcdsaSecurityKey()
        {
            CryptoProviderFactory.CustomCryptoProvider = new CustomCryptoProvider();
        }
        public BouncyCastleEcdsaSecurityKey(AsymmetricKeyParameter keyParameters)
        {
            KeyParameters = keyParameters;
            CryptoProviderFactory.CustomCryptoProvider = new CustomCryptoProvider();
        }

        public AsymmetricKeyParameter KeyParameters { get; }
        public override int KeySize => throw new NotImplementedException();

        [Obsolete("HasPrivateKey method is deprecated, please use PrivateKeyStatus.")]
        public override bool HasPrivateKey => KeyParameters.IsPrivate;

        public override PrivateKeyStatus PrivateKeyStatus
            => KeyParameters.IsPrivate ? PrivateKeyStatus.Exists : PrivateKeyStatus.DoesNotExist;
    }
}
