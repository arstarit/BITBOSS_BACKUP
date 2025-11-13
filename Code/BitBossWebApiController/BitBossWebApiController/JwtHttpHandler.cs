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

using Org.BouncyCastle.Crypto.Agreement;
using System.Security.Claims;

namespace BitBossWebApiController {
	public class RollingList<T> : IEnumerable<T> {
		private readonly LinkedList<T> _list = new LinkedList<T>();

		public RollingList(int maximumCount) {
			if (maximumCount <= 0)
				throw new ArgumentException(null, nameof(maximumCount));

			MaximumCount = maximumCount;
		}

		public int MaximumCount { get; }
		public int Count => _list.Count;

		public void Add(T value) {
			lock(_list) {
				if (_list.Count == MaximumCount) {
					_list.RemoveFirst();
				}
				_list.AddLast(value);
			}
		}
		public bool FindAndRemove(T value) {
			lock(_list) {
				return _list.Remove(value);
			}
		}

		public T this[int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException();

				return _list.Skip(index).First();
			}
		}

		public IEnumerator<T> GetEnumerator() {
			return _list.GetEnumerator();
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
			return _list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}
	}

	public class JwtHttpHandler {
		static SecureRandom secureRandom = new SecureRandom();
		static RollingList<int> nonceList = new RollingList<int>(10);
		// static int nonce = 0;
		static string[] postmanPermitted = {
			"/V0/Control/Health",
			"/V0/Control/APICommHealth",
			"/V0/Control/LinksHealth"
		};
		static ECPrivateKeyParameters _privKey = null;
		static BouncyCastleEcdsaSecurityKey _pubKey = null;

		public static void TestList() {
			for (var i = 0; i < 20; i++) {
				var nonce = secureRandom.NextInt();
				Console.WriteLine($"adding: {nonce}");
				nonceList.Add(nonce);
			}
			Console.WriteLine($"list test: {nonceList.Count} \n"
				+ $"{string.Join(", ", nonceList)}");
		}
		public static async Task Check(HttpContext context, Func<Task> next) {
			var path = context.Request.Path.ToString();
			Console.WriteLine(path);
			// Console.WriteLine($"bypassJwt: {Startup.Configuration["Jwt:bypassJwt"]}");
			if (Startup.Configuration["Jwt:bypassJwt"] == "True") {
				Console.WriteLine("NO NONCE CHECK");
				await next(); 
				return;
			}
			bool isPost = context.Request.Method == "POST";
			// Console.WriteLine($"isPost: {isPost}");

			string bodyHash = "";
			var n = 0;
			string payloadHash = "";
			JwtSecurityToken token = null;

			if (path.StartsWith("/V0/Control/Upgrade")) isPost = false;
			Console.WriteLine($"isPost: {isPost}");
			isPost = false;
			if (isPost) {
				var body = await GetBody(context);
				// Console.WriteLine($"body: {body}");
				using (var hasher = SHA256.Create()) {
					bodyHash = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(body)))
						.Split("=")[0];
				}
				// Console.WriteLine($"bodyHash: {bodyHash}");
			}
			try {
				foreach (var h in context.Request.Headers) {
					// Console.WriteLine(h);
					if (h.Key.Equals("Authorization")) {
						token = ParseJWT(h.Value[0].Split(' ')[1]);
					}
				}
				// Console.WriteLine($"Token: {token}");
				if (token.Payload.ContainsKey("hash")) payloadHash = token.Payload["hash"].ToString();
				// Console.WriteLine($"payloadHash: {payloadHash}");
				Int32.TryParse(token.Payload.Nonce, out n);
			} catch (Exception e) {
				Console.WriteLine("Exception");
				Console.WriteLine(e);
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("bad jwt");
				return;
			}
			if (n == 0 && Array.Exists(postmanPermitted, s => s.Equals(path))) {
				await next();
				return;
			}
			if (isPost && bodyHash != payloadHash) {
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("bad hash");
				return;
			}
			if (!path.Equals("/V0/Control/Start")) {
				if (IsNonce(n)) {
					// Console.WriteLine("Nonce pass");
				} else {
					Console.WriteLine($"Nonce fail: {n} not in "
						+ $"{string.Join(", ", nonceList)}");
					// throw new NotSupportedException();
					context.Response.StatusCode = 400;
					await context.Response.WriteAsync("bad nonce");
					return;
				}
			} else {
				Console.WriteLine("Nonce fetch");
			}
			SetNonce(context.Response.Headers);
			// throw new BadHttpRequestException(401);
			// throw new NotSupportedException();
			await next(); 
		}
		public static bool IsNonce(int n) {
			// return (nonce != 0 && n == nonce);
			// Console.WriteLine($"IsNonce: {n} \n"
			// 	+ $"{string.Join(", ", nonceList)}");
			return nonceList.FindAndRemove(n);
		}
		public static void SetNonce(IHeaderDictionary headers) {
			var nonce = secureRandom.NextInt();
			nonceList.Add(nonce);
			// Console.WriteLine($"returning nonce: {nonce}");
			// Console.WriteLine($"nonce list: {nonceList.Count} \n"
			// 	+ $"{string.Join(", ", nonceList)}");

			string ephemKeyPub;
			string iv;
			string encryptedMessage;
			var nonceStr = $"{nonce}";
			var key = Encryption.GetEncyptKeyAgreement(out ephemKeyPub);
			encryptedMessage = Encryption.Encrypt(key, nonceStr, out iv);
			var enonce = ephemKeyPub + " " + iv + " " + encryptedMessage
				+ " " + createSig(nonceStr);
			// headers.Add("x-enonce", $"{enonce}");
			// headers.Add("x-device-id", $"{Program.device_id}");
			headers.Add("Authorization", "Bearer " + CreateJWT(ephemKeyPub + " " + iv + " " + encryptedMessage));
			// headers.Add("jwt", $"{createJWT()}");
		}
		public static byte[] FromHexString(string hex) {
			var numberChars = hex.Length;
			var hexAsBytes = new byte[numberChars / 2];
			for (var i = 0; i < numberChars; i += 2)
				hexAsBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return hexAsBytes;
		}
		private static BouncyCastleEcdsaSecurityKey GetSecurityKey() {
			if (_pubKey != null) return _pubKey;

			_pubKey = CustomSignatureProvider.GetSecurityKey(Startup.Configuration["Jwt:CMPubKey"]);

			return _pubKey;
		}
		private static BouncyCastleEcdsaSecurityKey GetPrivSecurityKey() {
            return new BouncyCastleEcdsaSecurityKey(GetPrivKey()) { };
		}
		private static ECPrivateKeyParameters GetPrivKey() {
			if (_privKey != null) return _privKey;
			X9ECParameters secp256k1 = ECNamedCurveTable.GetByName("secp256k1");
			var parameters = new ECDomainParameters(
					secp256k1.Curve, secp256k1.G, secp256k1.N,
					secp256k1.H, secp256k1.GetSeed());

			var keyStr = Startup.Configuration["Jwt:MyPrivKey"];
			var key = FromHexString(keyStr);
			Console.WriteLine($"key.Length: {key.Length}");
			// ECPrivateKeyParameters privKey = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(key);
			_privKey = new ECPrivateKeyParameters("ECDSA",
				new BigInteger(keyStr, 16), // d
				parameters);
			return _privKey;
		}
		public static string createSig(string input) {
			byte[] hashedInput;
			using (var hasher = SHA256.Create())
			{
				hashedInput = hasher.ComputeHash(Encoding.ASCII.GetBytes(input));
			}
			ECPrivateKeyParameters privKey = GetPrivKey();
			SecureRandom random = new SecureRandom();

			ECDsaSigner ecdsa = new ECDsaSigner();
			// SecureRandom random = new SecureRandom();
			ParametersWithRandom param = new ParametersWithRandom(privKey, random);

			ecdsa.Init(true, param);
			// ECKeyPairGenerator pGen = new ECKeyPairGenerator();
			// ECKeyGenerationParameters genParam = new ECKeyGenerationParameters(parameters, random);
			// pGen.Init(genParam);
			// AsymmetricCipherKeyPair ephemKey = pGen.GenerateKeyPair();
			// var privKey = ephemKey.Private as ECPrivateKeyParameters;
			// var pubKey = (ECPublicKeyParameters)ephemKey.Public;
			// var pubKeyStr = Encryption.ToHex((pubKey).Q.GetEncoded(true));
			// Console.WriteLine($"Private key: {Encryption.ToHex(privKey.D.ToByteArrayUnsigned())}");
			// Console.WriteLine($"pubKeyStr: {pubKeyStr}");
			// pubKey.setPointFormat("COMPRESSED");
			// pubkeystr = Convert.ToBase64String((pubKey).Q.GetEncoded(true));

			var sig = ecdsa.GenerateSignature(hashedInput);
			// Console.WriteLine($"hashedInput: {Encryption.ToHex(hashedInput)}");

			return sig[0].ToString(16) + " " + sig[1].ToString(16);
		}
		public static async Task<string> GetBody(HttpContext context) {
			var request = context.Request;
			// IMPORTANT: Ensure the requestBody can be read multiple times.
			HttpRequestRewindExtensions.EnableBuffering(request);
			string strRequestBody;
			
			// IMPORTANT: Leave the body open so the next middleware can read it.
			using (StreamReader reader = new StreamReader(
				request.Body,
				Encoding.UTF8,
				detectEncodingFromByteOrderMarks: false,
				leaveOpen: true))
			{
				strRequestBody = await reader.ReadToEndAsync();

				// IMPORTANT: Reset the request body stream position so the next middleware can read it
				request.Body.Position = 0;
			}

			return strRequestBody;
		}
		public static JwtSecurityToken ParseJWT(string jwt) {
			var handler = new JwtSecurityTokenHandler();
			SecurityToken validatedToken;
			var principal = handler.ValidateToken(jwt, new TokenValidationParameters() {
					ValidateLifetime = true,
					ValidateAudience = false,
					ValidateIssuer = true,
					ValidIssuer = Startup.Configuration["Jwt:CMPubKey"],
					ValidAudience = null,
					IssuerSigningKey = GetSecurityKey()
				}, out validatedToken);
			// Console.WriteLine($"ParseJWT.principal: {principal}");
			// Console.WriteLine($"ParseJWT.validatedToken: {validatedToken}");
			// Console.WriteLine($"ParseJWT.principal.Identity: {principal.Identity}");
			// Console.WriteLine($"ParseJWT.principal.Identity: {principal.Identity.IsAuthenticated}");
			return handler.ReadJwtToken(jwt);
		}
		public static string CreateJWT(string nonce) {
			var handler = new JwtSecurityTokenHandler();
			List<Claim> claims = new List<Claim>() {
				new Claim("nonce", nonce),
			};
			var jwtSecurityToken = handler.CreateEncodedJwt(
				Startup.Configuration["Jwt:MyPubKey"],
				null, // "Audience",
				new ClaimsIdentity(claims),
				DateTime.Now,
				DateTime.Now.AddHours(1),
				DateTime.Now,
				new SigningCredentials(GetPrivSecurityKey(), "ES256K")
				);
			// Console.WriteLine($"jwtSecurityToken: {jwtSecurityToken}");
			return jwtSecurityToken;
		}
	}
	public class Encryption {
		private static readonly String ALGORITHM = "AES";
		private static readonly String CIPHER = "AES/GCM/NoPadding";
		private readonly byte[] key;


		public Encryption(byte[] key) {
			this.key = key;

			// Console.WriteLine("Encryption!!!");
		}

		public static string ByteArrayToString(byte[] ba) {
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}
		public static string Encrypt(byte[] key, string plainText, out string ivout) {
			byte[] array;

			using (Aes aes = Aes.Create()) {
				aes.Key = key;
				ivout = Convert.ToBase64String(aes.IV);

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream()) {
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write)) {
						using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream)) {
							streamWriter.Write(plainText);
						}
						array = memoryStream.ToArray();
					}
				}
			}
			return Convert.ToBase64String(array);
		}
		public static void encrypt1(string secretMessage, out byte[] encryptedMessage, out byte[] iv) {
			// var keyStr = Startup.Configuration["Jwt:CMPubKey"];
			// var key = Cryptography.ECDSA.Secp256K1Manager
			//     .PublicKeyDecompress(FromHexString(keyStr));
			// BouncyCastleEcdsaSecurityKey key = GetSecurityKey(Startup.Configuration["Jwt:CMPubKey"]);
			var key = JwtHttpHandler.FromHexString("4640d1105298593d1bfc56c9017aa45d3668673ac3242687654532832b78c4b5");
			using (Aes aes = new AesCryptoServiceProvider())
			{
				aes.Key = key;
				iv = aes.IV;

				// aes.BlockSize = 128;  
				// aes.KeySize = 128;  
				// aes.Key = System.Text.Encoding.UTF8.GetBytes(key);  
				// aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);  
				// aes.Padding = PaddingMode.PKCS7;  
				// aes.Mode = CipherMode.CBC;  
				Console.WriteLine($"aes.BlockSize: {aes.BlockSize}");
				Console.WriteLine($"aes.KeySize: {aes.KeySize}");
				Console.WriteLine($"aes.Key: {ByteArrayToString(aes.Key)}");
				Console.WriteLine($"aes.IV: {ByteArrayToString(aes.IV)}");
				Console.WriteLine($"aes.Padding: {aes.Padding}");
				Console.WriteLine($"aes.Mode: {aes.Mode}");
				// Encrypt the message
				using (MemoryStream ciphertext = new MemoryStream())
				using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
				{
					byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
					cs.Write(plaintextMessage, 0, plaintextMessage.Length);
					cs.Close();
					encryptedMessage = ciphertext.ToArray();
				}
			}
		}
		public static string encrypt3(string plainText, out string ivout) {
			// string keystr = "xFs7Tlz5VMUKZUymYZAYjmUwXFFK8Ak0vP3ZZUEjOLQ=";
			string keystr = "xSQluC2O/kQarW7EzhE/8wI7JxCWqu2wO4iBLP37WbE=";
			var key = Convert.FromBase64String(keystr);
			byte[] iv = new byte[16];
			byte[] array;
			Console.WriteLine($"Encoding.UTF8.GetBytes(key): {Encoding.UTF8.GetBytes("test")}");
			Console.WriteLine($"iv: {iv}");

			using (Aes aes = Aes.Create()) {
				aes.Key = key;
				// aes.IV = iv;
				iv = aes.IV;
				Console.WriteLine($"aes.BlockSize: {aes.BlockSize}");
				Console.WriteLine($"aes.KeySize: {aes.KeySize}");
				Console.WriteLine($"aes.Key: {Convert.ToBase64String(aes.Key)}");
				Console.WriteLine($"aes.Key: {ByteArrayToString(aes.Key)}");
				Console.WriteLine($"aes.IV: {ByteArrayToString(aes.IV)}");
				Console.WriteLine($"aes.Padding: {aes.Padding}");
				Console.WriteLine($"aes.Mode: {aes.Mode}");

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
						{
							streamWriter.Write(plainText);
						}

						array = memoryStream.ToArray();
					}
				}
			}

			ivout = Convert.ToBase64String(iv);
			return Convert.ToBase64String(array);
		}
		public static string encrypt4(byte[] key, string plainText, out string ivout) {
			// string keystr = "xFs7Tlz5VMUKZUymYZAYjmUwXFFK8Ak0vP3ZZUEjOLQ=";
			// string keystr = "xSQluC2O/kQarW7EzhE/8wI7JxCWqu2wO4iBLP37WbE=";
			// var key = Convert.FromBase64String(keystr);
			byte[] iv = new byte[16];
			byte[] array;
			Console.WriteLine($"Encoding.UTF8.GetBytes(key): {Encoding.UTF8.GetBytes("test")}");
			Console.WriteLine($"iv: {ByteArrayToString(iv)}");

			using (Aes aes = Aes.Create()) {
				aes.Key = key;
				// aes.IV = iv;
				iv = aes.IV;
				Console.WriteLine($"aes.BlockSize: {aes.BlockSize}");
				Console.WriteLine($"aes.KeySize: {aes.KeySize}");
				Console.WriteLine($"aes.Key: {Convert.ToBase64String(aes.Key)}");
				Console.WriteLine($"aes.Key: {ByteArrayToString(aes.Key)}");
				Console.WriteLine($"aes.IV: {ByteArrayToString(aes.IV)}");
				Console.WriteLine($"aes.Padding: {aes.Padding}");
				Console.WriteLine($"aes.Mode: {aes.Mode}");

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
						{
							streamWriter.Write(plainText);
						}

						array = memoryStream.ToArray();
					}
				}
			}

			ivout = Convert.ToBase64String(iv);
			return Convert.ToBase64String(array);
		}
		public static Byte[] GetEncyptKeyAgreement(out string pubkeystr) {
			var decompressed = Cryptography.ECDSA.Secp256K1Manager
				.PublicKeyDecompress(JwtHttpHandler.FromHexString(
					Startup.Configuration["Jwt:CMPubKey"]));

			X9ECParameters secp256k1 = ECNamedCurveTable.GetByName("secp256k1");
			var parameters = new ECDomainParameters(
					secp256k1.Curve, secp256k1.G, secp256k1.N,
					secp256k1.H, secp256k1.GetSeed());
			var publicKey = new ECPublicKeyParameters(
				secp256k1.Curve.CreatePoint(
					new BigInteger(1, decompressed.Skip(1).Take(32).ToArray()), // x
					new BigInteger(1, decompressed.Skip(33).ToArray()) // y
					),
				parameters
				);

			SecureRandom random = new SecureRandom();
			ECKeyPairGenerator pGen = new ECKeyPairGenerator();
			ECKeyGenerationParameters genParam = new ECKeyGenerationParameters(parameters, random);
			pGen.Init(genParam);

			AsymmetricCipherKeyPair ephemKey = pGen.GenerateKeyPair();

			IBasicAgreement agreement = new ECDHBasicAgreement();
			agreement.Init(ephemKey.Private);

			var pubKey = (ECPublicKeyParameters)ephemKey.Public;
			// pubKey.setPointFormat("COMPRESSED");
			pubkeystr = Convert.ToBase64String((pubKey).Q.GetEncoded(true));
			// Console.WriteLine($"ephemKey: {pubkeystr}");

			var sharedSecret = agreement.CalculateAgreement(publicKey).ToByteArray();
			if (sharedSecret[0] == 0) {
				sharedSecret = sharedSecret.Skip(1).ToArray();
				// Console.WriteLine($"trimming to: {ByteArrayToString(sharedSecret)}");
			}
			// Console.WriteLine($"sharedSecret: {ByteArrayToString(sharedSecret)}");
			// Console.WriteLine($"sharedSecret.Length: {sharedSecret.Length}");
			SHA512 shaM = new SHA512Managed();
			var sha512 = shaM.ComputeHash(sharedSecret).Take(32).ToArray();
			// Console.WriteLine($"sha512: {ByteArrayToString(sha512)}");
			return sha512;

		}
		public static AsymmetricCipherKeyPair GenerateKeyPair() {
			var curve = ECNamedCurveTable.GetByName("secp256k1");
			var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

			var secureRandom = new SecureRandom();
			var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

			var generator = new ECKeyPairGenerator("ECDSA");
			generator.Init(keyParams);
			var keyPair = generator.GenerateKeyPair();

			var privateKey = keyPair.Private as ECPrivateKeyParameters;
			var publicKey = keyPair.Public as ECPublicKeyParameters;

			// Console.WriteLine($"Private key: {ToHex(privateKey.D.ToByteArrayUnsigned())}");
			// Console.WriteLine($"Public key: {ToHex(publicKey.Q.GetEncoded())}");

			return keyPair;
		}
		public static string ToHex(byte[] data) => String.Concat(data.Select(x => x.ToString("x2")));
		public String GenerateIV() {
			byte[] randomInitializationVector = new byte[16];
			SecureRandom secureRandom = SecureRandom.GetInstance("SHA1PRNG");
			secureRandom.NextBytes(randomInitializationVector);
			return Encoding.UTF8.GetString(Base64.Encode(randomInitializationVector));
		}

		private IBufferedCipher CreateCipher(String iv, bool encrypt) {
			KeyParameter keySpec = ParameterUtilities.CreateKeyParameter(ALGORITHM, key);
			ParametersWithIV ivSpec = new ParametersWithIV(keySpec, Base64.Decode(iv));

			IBufferedCipher cipher = CipherUtilities.GetCipher(CIPHER);
			cipher.Init(encrypt, ivSpec);
			return cipher;
		}

	}
}
