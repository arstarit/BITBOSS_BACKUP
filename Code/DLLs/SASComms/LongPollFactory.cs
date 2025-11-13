using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SASComms
{
    /// <summary>
    /// Singleton. Un único objeto que se encarga de fabricar long polls
    /// Singleton. An unique object that creates new long polls
    /// </summary>
    public sealed class LongPollFactory
    {

        private LongPollFactory()
        {
        }

        /// <summary>
        /// Dado un array (o lista) de array de bytes, la idea es "aplanar" la lista en un sólo array que concatene cada elemento
        // Given an array or list of byte array, this functions concatenates each byte arrays to create a single array
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private Byte[] join(params Byte[][] arr)
        {
            Byte[] b = new Byte[] { };
            foreach (Byte[] b_ in arr)
            {
                b = b.ToList().Concat(b_.ToList()).ToArray();
            }
            return b;
        }

        /// <summary>
        /// Dado un single byte, retorna un array de un sólo elemento conteniendo ese byte
        /// Given a single byte, returns an array with only one element containing that byte
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private Byte[] B(Byte arr)
        {
            return new Byte[] { arr };
        }

        private static readonly Lazy<LongPollFactory> lazy = new Lazy<LongPollFactory>(() => new LongPollFactory());
        public static LongPollFactory Singleton
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// Función privada que calcula el CRC (en array de bytes) 
        /// Private function that computes the CRC, as byte array
        /// </summary>
        /// <param name="bytes">Un array de bytes o longpoll</param>
        /// <returns></returns>
        private byte[] GetCRCBytes(byte[] bytes)
        {
            CyclicalRedundancyCheck crc = new CyclicalRedundancyCheck();
            return crc.GetCRCBytes(bytes);
        }
        /***********************************************************************************************/
        /***********************************************************************************************/
        /*****************************************HOST**************************************************/
        /***********************************************************************************************/
        /***********************************************************************************************/

        #region "Host"

        /// <summary>
        /// BUILD LP01
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] LockOutPlay(Byte address)
        {
            Byte[] gp = join(B(address),
                             B(0x01));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP02
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] EnablePlay(Byte address)
        {
            Byte[] gp = join(B(address),
                             B(0x02));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP03
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] SoundOff(Byte address)
        {
            Byte[] gp = join(B(address),
                             B(0x03));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP04
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] SoundOn(Byte address)
        {
            Byte[] gp = join(B(address),
                             B(0x04));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP06
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP06(byte address)
        {
            byte[] gp = new byte[] { address, 0x06 };
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP07
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP07(byte address)
        {
            byte[] gp = new byte[] { address, 0x07 };
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP08
        /// </summary>
        /// <param name="address"></param>
        /// <param name="billDenominations"></param>
        /// <param name="billAcceptorFlag"></param>
        /// <returns></returns>
        public Byte[] BuildConfigureBillDenominationsLongPollCommand(byte address,
                                                                     byte[] billDenominations,
                                                                     byte billAcceptorFlag)
        {
            Byte[] body = join(billDenominations,
                               B(billAcceptorFlag));
            Byte[] gp = join(B(address),
                             B(0x08),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP0A
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetEnterMaintenanceMode(Byte address)
        {
            Byte[] gp = { address, 0x0A };
            CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
            Byte[] _crc = crcGen.GetCRCBytes(gp);

            gp = gp.ToList().Concat(_crc.ToList()).ToArray();
            return gp;
        }

        /// <summary>
        /// BUILD LP0B
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetExitMaintenanceMode(Byte address)
        {
            Byte[] gp = { address, 0x0B };
            CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
            Byte[] _crc = crcGen.GetCRCBytes(gp);

            gp = gp.ToList().Concat(_crc.ToList()).ToArray();
            return gp;
        }

        /// <summary>
        /// BUILD LP0E
        /// </summary>
        /// <param name="address"></param>
        /// <param name="enable_disable"></param>
        /// <returns></returns>
        public Byte[] BuildEnableDisableRealTimeEvent(byte address,
                                                      byte enable_disable)
        {
            Byte[] gp = join(B(address),
                             B(0x0E),
                             B(enable_disable));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP1B
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] BuildHandPayInformationCommand(Byte address)
        {

            Byte[] gp = { address, 0x1B };

            return gp;
        }

        /// <summary>
        /// BUILD LP1C
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetSendMeters(Byte address)
        {

            Byte[] gp = { address, 0x1C };

            return gp;
        }

        /// <summary>
        /// BUILD LP1E
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP1E(byte address)
        {
            Byte[] gp = new byte[] { address, 0x1E };
            return gp;
        }

        /// <summary>
        /// BUILD LP1F
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetGamingMachineIDAndInformation(Byte address)
        {

            Byte[] gp = { address, 0x1F };

            return gp;
        }


        /// <summary>
        /// BUILD LP20
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP20(byte address)
        {
            byte[] gp = new byte[] { address, 0x20 };
            return gp;
        }

        /// <summary>
        /// BUILD LP2A
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP2A(byte address)
        {
            byte[] gp = new byte[] { address, 0x2A };
            return gp;
        }


        /// <summary>
        /// BUILD LP2B
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP2B(byte address)
        {
            byte[] gp = new byte[] { address, 0x2B };
            return gp;
        }


        /// <summary>
        /// BUILD LP2D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public Byte[] GetLP2D(Byte address, byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x2D),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP2E
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bufferAmount"></param>
        /// <returns></returns>
        public byte[] GetLP2E(byte address, byte[] bufferAmount)
        {
            byte[] gp = join(B(address),
                             B(0x2E),
                             bufferAmount);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP2F
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="RequestedMeterCode"></param>
        /// <returns></returns>
        public Byte[] GetSendSelectedMeters(Byte address, Byte[] gameNumber, Byte[] RequestedMeterCode)
        {
            int length = 0;
            Byte[] gp1 = { address, 0x2F };
            Byte[] gp3 = { };
            gp3 = gp3.ToList().Concat(gameNumber.ToList())
                             .Concat(RequestedMeterCode.ToList()).ToArray();
            length = gp3.Length;
            Byte[] gp2 = { BitConverter.GetBytes(length)[0] };
            Byte[] gp = { };
            gp = gp.ToList().Concat(gp1.ToList())
                            .Concat(gp2.ToList())
                            .Concat(gp3.ToList()).ToArray();

            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();
            return gp;

        }

        /// <summary>
        /// BUILD LP48
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] GetSendLastBillAcceptedInformation(Byte address)
        {
            byte[] gp = new byte[] { address, 0x48 };
            return gp;
        }

        /// <summary>
        /// BUILD LP21
        /// </summary>
        /// <param name="address"></param>
        /// <param name="seedValue"></param>
        /// <returns></returns>
        public byte[] BuildROMSignatureVerification(byte address, byte[] seedValue)
        {
            Byte[] header = new byte[] { address, 0x21 };
            Byte[] gp = join(header,
                             seedValue);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);

            return gp;
        }

        /// <summary>
        /// BUILD LP4C
        /// </summary>
        /// <param name="address"></param>
        /// <param name="machineID"></param>
        /// <param name="sequenceNumber"></param>
        /// <returns></returns>
        public Byte[] BuildSetSecureEnhancedValidationID(Byte address,
                                                         Byte[] machineID,
                                                         Byte[] sequenceNumber)
        {
            Byte[] gp = join(machineID,
                             sequenceNumber);
            gp = join(B(address),
                      B(0x4C),
                      gp);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP4D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="functionCode"></param>
        /// <returns></returns>
        public Byte[] GetSendEnhancedValidationInformation(Byte address, Byte functionCode)
        {
            Byte[] gp = { address, 0x4D, functionCode };
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }

        /// <summary>
        /// BUILD LP50
        /// </summary>
        /// <param name="address"></param>
        /// <param name="validationType"></param>
        /// <returns></returns>
        public byte[] BuildSendValidationMetersComand(byte address,
                                              byte validationType)
        {
            Byte[] gp = join(B(address),
                             B(0x50),
                             B(validationType));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP51
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetLP51(Byte address)
        {
            Byte[] gp = { address, 0x51 };
            return gp;
        }

        /// <summary>
        /// BUILD LP53
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] GetLP53(byte address,
                              byte[] gameNumber)
        {
            Byte[] header = new byte[] { address, 0x53 };
            Byte[] gp = join(header,
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);

            return gp;

        }

        /// <summary>
        /// BUILD LP54
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetSASVersionAndMachineSerialNumber(Byte address)
        {
            Byte[] gp = { address, 0x54 };
            return gp;
        }

        /// <summary>
        /// BUILD LP55
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP55(byte address)
        {
            byte[] gp = new byte[] { address, 0x55 };
            return gp;
        }

        /// <summary>
        /// BUILD LP56
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP56(byte address)
        {
            byte[] gp = new byte[] { address, 0x56 };
            return gp;
        }

        /// <summary>
        /// BUILD LP57
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetPendingCashoutInformation(Byte address)
        {
            Byte[] gp = { address, 0x57 };
            return gp;
        }

        /// <summary>
        /// BUILD LP58
        /// </summary>
        /// <param name="address"></param>
        /// <param name="validationSystemID"></param>
        /// <param name="validationNumber"></param>
        /// <returns></returns>
        public Byte[] GetReceiveValidationNumber(Byte address, byte validationSystemID, byte[] validationNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x58),
                             B(validationSystemID),
                             validationNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP6F
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="RequestedMeterCode"></param>
        /// <returns></returns>
        public Byte[] GetSendExtendMeters(Byte address, Byte[] gameNumber, Byte[] RequestedMeterCode)
        {
            int length = 0;
            Byte[] gp1 = { address, 0x6F };
            Byte[] gp3 = { };
            gp3 = gp3.ToList().Concat(gameNumber.ToList())
                             .Concat(RequestedMeterCode.ToList()).ToArray();
            length = gp3.Length;
            Byte[] gp2 = { BitConverter.GetBytes(length)[0] };
            Byte[] gp = { };
            gp = gp.ToList().Concat(gp1.ToList())
                            .Concat(gp2.ToList())
                            .Concat(gp3.ToList()).ToArray();

            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();
            return gp;

        }

        /// <summary>
        /// BUILD LP70
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] BuildSendTicketValidationDataCommand(Byte address)
        {
            Byte[] gp = { address, 0x70 };
            return gp;
        }


        /// <summary>
        /// BUILD LP71
        /// </summary>
        /// <param name="address"></param>
        /// <param name="transferCode"></param>
        /// <param name="transferAmount"></param>
        /// <param name="parsingCode"></param>
        /// <param name="validationData"></param>
        /// <param name="restrictedExpiration"></param>
        /// <param name="poolId"></param>
        /// <returns></returns>
        public Byte[] BuildRedeemTicketCommand(Byte address,
                                               Byte transferCode,
                                               uint transferAmount,
                                               Byte parsingCode,
                                               Byte[] validationData,
                                               DateTime restrictedExpiration,
                                               Byte[] poolId)
        {
            Byte[] transferAmount_ = intToBCD5_v2(transferAmount, 5);
            Byte[] day = intToBCD5_v2((uint)restrictedExpiration.Day, 1);
            Byte[] month = intToBCD5_v2((uint)restrictedExpiration.Month, 1);
            Byte[] year = intToBCD5_v2((uint)restrictedExpiration.Year, 2);
            Byte[] restrictedExpiration_ = join(day, month, year);

            Byte[] gp = join(B(transferCode),
                             transferAmount_,
                             B(parsingCode),
                             validationData,
                             restrictedExpiration_,
                             poolId);
            gp = join(B(address),
                      B(0x71),
                      B((byte)gp.Length),
                      gp);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP71 (Only Transfer Code)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="transferCode"></param>
        /// <returns></returns>
        public Byte[] BuildRedeemTicketCommand(Byte address,
                                               Byte transferCode)
        {


            Byte[] gp = join(B(address),
                             B(0x71),
                             B(0x01),
                             B(transferCode));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP71 (with Amount, parsing Code and validationData)
        /// </summary>
        /// <param name="address"></param>
        /// <param name="transferCode"></param>
        /// <param name="transferAmount"></param>
        /// <param name="parsingCode"></param>
        /// <param name="validationData"></param>
        /// <returns></returns>
        public Byte[] BuildRedeemTicketCommand(Byte address,
                                               Byte transferCode,
                                               uint transferAmount,
                                               Byte parsingCode,
                                               Byte[] validationData)
        {
            Byte[] transferAmount_ = intToBCD5_v2(transferAmount, 5);

            Byte[] gp = join(B(transferCode),
                             transferAmount_,
                             B(parsingCode),
                             validationData);
            gp = join(B(address),
                      B(0x71),
                      B((byte)gp.Length),
                      gp);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        // 
        /// <summary>
        /// BUILD LP72
        /// </summary>
        /// <param name="address"></param>
        /// <param name="transaction_index"></param>
        /// <returns></returns>
        public Byte[] GetAFTInt(Byte address, Byte transaction_index)
        {
            Byte[] gp = { address, 0x72, 0x02, 0xFF, transaction_index };
            CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
            Byte[] _crc = crcGen.GetCRCBytes(gp);

            gp = gp.ToList().Concat(_crc.ToList()).ToArray();
            return gp;
        }

        /// <summary>
        /// BUILD LP72. ¡¡¡¡¡¡USED BY SASCONSOLE!!!!!!
        /// </summary>
        /// <param name="address"></param>
        /// <param name="assetNumber"></param>
        /// <param name="transferCode"></param>
        /// <param name="transferType"></param>
        /// <param name="cashableAmount"></param>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public Byte[] GetAFTTransferFunds(Byte address, int assetNumber, byte[] poolID, byte transferCode, byte transferType, long cashableAmount, Byte[] transactionID)
        {
            //int length = 1;
            byte[] _assetNumber = BitConverter.GetBytes(assetNumber);
            //byte[] _pos_id = BitConverter.GetBytes(pos_id);
            byte[] _cashableAmount = intToBCD5((uint)cashableAmount);
            // Expiration: 10 after now
            DateTime Expiration = DateTime.Now.AddDays(10);
            
            Byte[] header = { address, 0x72 }; // Address + Command


            byte[] body = join(B(transferCode), // Transfer Code
                                B(0x00), // Transaction Index
                                B(transferType), // Transfer Type
                                new byte[] {_cashableAmount[4], _cashableAmount[3], _cashableAmount[2], _cashableAmount[1], _cashableAmount[0] }, // Cashable Amount
                                new byte[] {0x00, 0x00, 0x00, 0x00, 0x00}, // Restricted amount
                                new byte[] {0x00, 0x00, 0x00, 0x00, 0x00}, // Non restricted amount
                                B(0x00), // Transfer Flags
                                new byte[] {_assetNumber[0], _assetNumber[1], _assetNumber[2], _assetNumber[3]}, // Asset Number
                                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20 }, // Registration Key
                                new byte[] { (byte)transactionID.Length }, // Transaction ID Length
                                transactionID, // Transaction ID
                                intToBCD5_v2((uint)Expiration.Month, 1),  intToBCD5_v2((uint)Expiration.Day, 1), intToBCD5_v2((uint)Expiration.Year, 2), // Expiration
                                poolID, // PoolID
                                B(0x00)); // Receipt Data
            
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);

            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
           
        }

        /// <summary>
        /// BUILD LP72
        /// </summary>
        /// <param name="address"></param>
        /// <param name="assetNumber"></param>
        /// <param name="transferCode"></param>
        /// <param name="transferType"></param>
        /// <param name="cashableAmount"></param>
        /// <param name="restrictedAmount"></param>
        /// <param name="nonrestrictedAmount"></param>
        /// <param name="transactionID"></param>
        /// <returns></returns>
        public Byte[] GetAFTTransferFunds(Byte address, int assetNumber, byte[] poolID, byte transferCode, byte transferType, long cashableAmount, long restrictedAmount, long nonrestrictedAmount, Byte[] transactionID, byte[] registrationKey, byte[] Expiration)
        {

            //int length = 1;
            byte[] _assetNumber = BitConverter.GetBytes(assetNumber);
            //byte[] _pos_id = BitConverter.GetBytes(pos_id);
            byte[] _cashableAmount = intToBCD5((uint)cashableAmount);
            byte[] _restrictedAmount = intToBCD5((uint)restrictedAmount);
            byte[] _nonrestrictedAmount = intToBCD5((uint)nonrestrictedAmount);

            Byte[] header = { address, 0x72 }; // Address + Command

            byte[] body = join(B(transferCode), // Transfer Code
                                B(0x00), // Transaction Index
                                B(transferType), // Transfer Type
                                new byte[] {_cashableAmount[4], _cashableAmount[3], _cashableAmount[2], _cashableAmount[1], _cashableAmount[0] }, // Cashable Amount
                                new byte[] {_restrictedAmount[4], _restrictedAmount[3], _restrictedAmount[2], _restrictedAmount[1], _restrictedAmount[0] }, // Restricted amount
                                new byte[] {_nonrestrictedAmount[4], _nonrestrictedAmount[3], _nonrestrictedAmount[2], _nonrestrictedAmount[1], _nonrestrictedAmount[0] }, // Non restricted amount
                                B(0x00), // Transfer Flags
                                new byte[] {_assetNumber[0], _assetNumber[1], _assetNumber[2], _assetNumber[3]}, // Asset Number
                                registrationKey, // Registration Key
                                new byte[] { (byte)transactionID.Length }, // Transaction ID Length
                                transactionID, // Transaction ID
                                Expiration, // Expiration
                                poolID, // PoolID
                                B(0x00)); // Receipt Data
            
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);

            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }

        /// <summary>
        /// BUILD LP73
        /// </summary>
        /// <param name="address"></param>
        /// <param name="registration_code"></param>
        /// <returns></returns>
        public Byte[] GetAFTRegistration(Byte address, Byte registration_code)
        {
                int length = 1;
                Byte[] lp = { address, 0x73, 0x01, registration_code};

                CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
                Byte[] _crc = crcGen.GetCRCBytes(lp);

                lp = lp.ToList().Concat(_crc.ToList()).ToArray();

                return lp;
        }


        /// <summary>
        /// BUILD LP73
        /// </summary>
        /// <param name="address"></param>
        /// <param name="registration_code"></param>
        /// <param name="asset_number"></param>
        /// <param name="registration_key"></param>
        /// <param name="pos_id"></param>
        /// <returns></returns>
        public Byte[] GetAFTRegistration(Byte address, Byte registration_code, int asset_number, Byte[] registration_key, int pos_id)
        {
            if (registration_code == 0xFF)
            {
                int lengthFF = 1;
                Byte[] lpFF = { address, 0x73, BitConverter.GetBytes(lengthFF)[0], registration_code};
                CyclicalRedundancyCheck crcGenFF = new CyclicalRedundancyCheck();
                Byte[] _crcFF = crcGenFF.GetCRCBytes(lpFF);

                lpFF = lpFF.ToList().Concat(_crcFF.ToList()).ToArray();

                return lpFF;
            }
            else
            {
                int length = 1;
                byte[] _asset_number = BitConverter.GetBytes(asset_number);
                byte[] _pos_id = BitConverter.GetBytes(pos_id);

                length = 1 + 4 + registration_key.Length + 4;

                Byte[] lp = { address, 0x73, BitConverter.GetBytes(length)[0], registration_code,
                    _asset_number[0], _asset_number[1], _asset_number[2], _asset_number[3]};

                lp = lp.ToList().Concat(registration_key.ToList()).ToArray();

                lp = lp.ToList().Concat(_pos_id.ToList()).ToArray();

                CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
                Byte[] _crc = crcGen.GetCRCBytes(lp);

                lp = lp.ToList().Concat(_crc.ToList()).ToArray();

                return lp;
            }
        }



        /// <summary>
        /// BUILD LP74
        /// </summary>
        /// <param name="address"></param>
        /// <param name="lockCode"></param>
        /// <param name="transferCondition"></param>
        /// <param name="lockTimeout1"></param>
        /// <param name="lockTimeout2"></param>
        /// <returns></returns>
        public Byte[] GetLockLP74(Byte address, Byte lockCode = 0x00, Byte transferCondition = 0x00, Byte lockTimeout1 = 0x00, Byte lockTimeout2 = 0x00)
        {

            Byte[] lp = { address, 0x74, lockCode, transferCondition, lockTimeout1, lockTimeout2 };
            CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
            Byte[] crc = crcGen.GetCRCBytes(lp);

            if (crc.Length == 2)
            {
                Byte[] lp74 = { address, 0x74, lockCode, transferCondition, lockTimeout1, lockTimeout2, crc[0], crc[1] };
                return lp74;
            }
            else
            {
                return lp;
            }
        }

        /// <summary>
        /// BUILD LP7B
        /// </summary>
        /// <param name="address"></param>
        /// <param name="control_mask1"></param>
        /// <param name="control_mask2"></param>
        /// <param name="status_bit_control_states1"></param>
        /// <param name="status_bit_control_states2"></param>
        /// <param name="cashable_ticket_and_receipt_expiration1"></param>
        /// <param name="cashable_ticket_and_receipt_expiration2"></param>
        /// <param name="restricted_ticket_default_expiration1"></param>
        /// <param name="restricted_ticket_default_expiration2"></param>
        /// <returns></returns>
        public Byte[] ExtendedValidationStatus(Byte address,
                                               Byte control_mask1, Byte control_mask2,
                                               Byte status_bit_control_states1, Byte status_bit_control_states2,
                                               Byte cashable_ticket_and_receipt_expiration1, Byte cashable_ticket_and_receipt_expiration2,
                                               Byte restricted_ticket_default_expiration1, Byte restricted_ticket_default_expiration2)
        {

            Byte[] gp = { address, 0x7B, 0x08, control_mask1, control_mask2, status_bit_control_states1, status_bit_control_states2, cashable_ticket_and_receipt_expiration1, cashable_ticket_and_receipt_expiration2, restricted_ticket_default_expiration1, restricted_ticket_default_expiration2 };
            CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
            Byte[] _crc = crcGen.GetCRCBytes(gp);

            gp = gp.ToList().Concat(_crc.ToList()).ToArray();
            return gp;
        }


        /// <summary>
        /// BUILD LP7C
        /// </summary>
        /// <param name="address"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Byte[] BuildSetExtendedTicketDataCommand(Byte address,
                                                        List<Tuple<byte, string>> elements)
        {
            Encoding encoding = Encoding.Default;
            Byte[] gp = { address, 0x7C };
            byte[] elements_serialized = { };
            foreach (Tuple<byte, string> e in elements)
            {
                byte[] data_bytes = encoding.GetBytes(e.Item2);
                elements_serialized = join(elements_serialized,
                                           B(e.Item1),
                                           B((byte)data_bytes.Length),
                                           data_bytes);
            }
            gp = join(gp,
                      B((byte)elements_serialized.Length),
                      elements_serialized);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP7D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="hostID"></param>
        /// <param name="expiration"></param>
        /// <param name="location"></param>
        /// <param name="address1"></param>
        /// <param name="address2"></param>
        /// <returns></returns>
        public byte[] BuildSetTicketData(byte address,
                                         byte[] hostID,
                                         byte expiration,
                                         byte[] location,
                                         byte[] address1,
                                         byte[] address2)
        {
            Byte[] gp = { address, 0x7D };
            byte[] body = join(hostID,
                               B(expiration),
                               B((byte)location.Length),
                               location,
                               B((byte)address1.Length),
                               address1,
                               B((byte)address2.Length),
                               address2);
            gp = join(gp,
                      B((byte)body.Length),
                      body);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP7D
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildSetTicketData(byte address)
        {
            Byte[] gp = { address, 0x7D };
            byte[] body = new byte[] { };
            gp = join(gp,
                      B((byte)body.Length),
                      body);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }




        /// <summary>
        /// BUILD LP7E
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP7E(byte address)
        {
            byte[] gp = new byte[] { address, 0x7E };
            return gp;
        }


        /// <summary>
        ///  BUILD LP7F
        /// </summary>
        /// <param name="address"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Byte[] BuildReceiveDateAndTimeCommand(byte address,
                                                     byte[] date,
                                                     byte[] time)
        {
            Byte[] gp = join(B(address),
                             B(0x7F),
                             date,
                             time);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP80
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="level"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public byte[] BuildLP80(byte address,
                                byte group,
                                byte level,
                                byte[] amount)
        {
            byte[] header = new byte[] { address, 0x80 };
            byte[] gp = join(header,
                             B(group),
                             B(level),
                             amount);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        ///  BUILD LP83
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP83(byte address,
                                byte[] gameNumber)
        {
            byte[] header = new byte[] { address, 0x83 };
            byte[] gp = join(header,
                             gameNumber);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD LP84
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP84(byte address)
        {
            byte[] gp = new byte[] { address, 0x84 };
            return gp;
        }


        /// <summary>
        /// BUILD LP85
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP85(byte address)
        {
            byte[] gp = new byte[] { address, 0x85 };
            return gp;
        }


        /// <summary>
        /// BUILD LP86
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="AmountsAndLevels"></param>
        /// <returns></returns>
        public byte[] BuildLP86(byte address,
                                byte group,
                                List<Tuple<byte, byte[]>> AmountsAndLevels)
        {
            byte[] header = new byte[] { address, 0x86 };
            byte[] gp = B(group);
            foreach (Tuple<byte, byte[]> t in AmountsAndLevels)
            {
                gp = join(gp,
                          B(t.Item1),
                          t.Item2);
            }
            gp = join(header,
                      B((byte)gp.Length),
                      gp);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP87
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLP87(byte address)
        {
            byte[] gp = new byte[] { address, 0x87 };
            return gp;
        }


        /// <summary>
        /// BUILD LP8A
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bonusAmount"></param>
        /// <param name="taxStatus"></param>
        /// <returns></returns>
        public byte[] BuildLP8A(byte address,
                                byte[] bonusAmount,
                                byte taxStatus)
        {
            byte[] gp = new byte[] {address,
                                    0x8A};
            gp = join(gp,
                      bonusAmount,
                      B(taxStatus));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP8C
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="time"></param>
        /// <param name="credits"></param>
        /// <param name="pulses"></param>
        /// <returns></returns>
        public byte[] BuildLP8C(byte address,
                                byte[] gameNumber,
                                byte[] time,
                                byte[] credits,
                                byte pulses)
        {
            byte[] header = new byte[] { address, 0x8C };
            byte[] gp = join(header,
                             gameNumber,
                             time,
                             credits,
                             B(pulses));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP94
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] BuildResetHandpayCommand(byte address)
        {
            Byte[] gp = join(B(address),
                             B(0x94));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        ///  BUILD LP95
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP95(byte address,
                                byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x95),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP96
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP96(byte address,
                                byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x96),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }



        /// <summary>
        /// BUILD LP97
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP97(byte address,
                                byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x97),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LP98
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP98(byte address,
                                byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x98),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        ///  BUILD LP99
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP99(byte address,
                                byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x99),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        ///  BUILD LP9A
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLP9A(byte address,
                                byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0x9A),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }



        /// <summary>
        /// BUILD LPA0
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public Byte[] BuildSendEnabledFeaturesCommand(byte address, byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0xA0),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LPA4
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLPA4(byte address, byte[] gameNumber)
        {
            Byte[] body = join(gameNumber);
            Byte[] gp = join(B(address),
                             B(0xA4),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LPA8
        /// </summary>
        /// <param name="address"></param>
        /// <param name="resetMethod"></param>
        /// <returns></returns>
        public byte[] BuildLPA8(byte address, byte resetMethod)
        {
            Byte[] body = join(B(resetMethod));
            Byte[] gp = join(B(address),
                             B(0xA8),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LPB1
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLPB1(byte address)
        {
            byte[] gp = new byte[] { address, 0xB1 };
            return gp;
        }


        /// <summary>
        ///  BUILD LPB2
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLPB2(byte address)
        {
            byte[] gp = new byte[] { address, 0xB2 };
            return gp;
        }


        /// <summary>
        ///  BUILD LPB3
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] BuildLPB3(byte address)
        {
            byte[] gp = new byte[] { address, 0xB3 };
            return gp;
        }


        /// <summary>
        ///  BUILD LPB4
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public byte[] BuildLPB4(byte address, byte[] gameNumber)
        {
           Byte[] gp = join(B(address),
                             B(0xB4),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD LPB5
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <returns></returns>
        public Byte[] BuildLPB5(Byte address, byte[] gameNumber)
        {
            Byte[] gp = join(B(address),
                             B(0xB5),
                             gameNumber);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }



        /// <summary>
        /// Get Sync (0x80)
        /// </summary>
        /// <returns></returns>
        public Byte[] GetSync()
        {

            Byte[] sync = { 0x80 };

            return sync;
        }

        /// <summary>
        /// Get General Poll (0x80 ORed the address) 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Byte[] GetGeneralPoll(byte address)
        {

            Byte[] gp = { (byte)(0x80 | address) };

            return gp;
        }


        #endregion


        /***********************************************************************************************/
        /***********************************************************************************************/
        /*****************************************CLIENT**************************************************/
        /***********************************************************************************************/
        /***********************************************************************************************/
        #region "Client"


        /// <summary>
        /// BUILD RESPONSE FOR LP0F
        /// </summary>
        /// <param name="address"></param>
        /// <param name="total_cancelled_credits"></param>
        /// <param name="total_coin_in"></param>
        /// <param name="total_coin_out"></param>
        /// <param name="total_drop"></param>
        /// <param name="total_jackpot"></param>
        /// <param name="games_played"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP0F(byte address,
                                         byte[] total_cancelled_credits,
                                         byte[] total_coin_in,
                                         byte[] total_coin_out,
                                         byte[] total_drop,
                                         byte[] total_jackpot,
                                         byte[] games_played)
        {
            byte[] header = new byte[] { address, 0x0F };
            byte[] gp = join(header,
                             total_cancelled_credits,
                             total_coin_in,
                             total_coin_out,
                             total_drop,
                             total_jackpot,
                             games_played);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }



        /// <summary>
        /// BUILD RESPONSE FOR LP18
        /// </summary>
        /// <param name="address"></param>
        /// <param name="games_played_since_last_power_up"></param>
        /// <param name="games_played_since_last_slot_door_closure"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP18(byte address,
                                         byte[] games_played_since_last_power_up,
                                         byte[] games_played_since_last_slot_door_closure)
        {
            byte[] header = new byte[] { address, 0x18 };
            byte[] gp = join(header,
                             games_played_since_last_power_up,
                             games_played_since_last_slot_door_closure);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP1B
        /// </summary>
        /// <param name="address"></param>
        /// <param name="ProgressiveGroup"></param>
        /// <param name="Level"></param>
        /// <param name="Amount"></param>
        /// <param name="PartialPay"></param>
        /// <param name="resetID"></param>
        /// <returns></returns>
        public byte[] GetResponse1B(byte address,
                                     byte ProgressiveGroup,
                                     byte Level,
                                     byte[] Amount,
                                     byte[] PartialPay,
                                     byte resetID)
        {
            Byte[] body = join(B(ProgressiveGroup),
                               B(Level),
                               Amount,
                               PartialPay,
                               B(resetID));
            Byte[] gp = join(B(address),
                             B(0x1B),
                             body,
                             new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;

        }


        /// <summary>
        /// BUILD RESPONSE FOR LP1C
        /// </summary>
        /// <param name="address"></param>
        /// <param name="total_coin_in"></param>
        /// <param name="total_coin_out"></param>
        /// <param name="total_drop"></param>
        /// <param name="total_jackpot"></param>
        /// <param name="games_played"></param>
        /// <param name="games_won"></param>
        /// <param name="slot_door_opened"></param>
        /// <param name="power_reset"></param>
        /// <returns></returns>
        public Byte[] GetResponseMeters(Byte address,
                                     byte[] total_coin_in,
                                     byte[] total_coin_out,
                                     byte[] total_drop,
                                     byte[] total_jackpot,
                                     byte[] games_played,
                                     byte[] games_won,
                                     byte[] slot_door_opened,
                                     byte[] power_reset)
        {

            Byte[] gp = { address, 0x1C };
            gp = gp.ToList().Concat(total_coin_in.ToList())
                            .Concat(total_coin_out.ToList())
                            .Concat(total_drop.ToList())
                            .Concat(total_jackpot.ToList())
                            .Concat(games_played.ToList())
                            .Concat(games_won.ToList())
                            .Concat(slot_door_opened.ToList())
                            .Concat(power_reset.ToList()).ToArray();
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP1D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="CumulativePromoCreditsToGM"></param>
        /// <param name="CumulativeNonCashableCreditsToGM"></param>
        /// <param name="CumulativeCreditsToHost"></param>
        /// <param name="CumulativeCashableCreditsToGM"></param>
        /// <returns></returns>
        public Byte[] GetResponse1D(byte address,
                                    byte[] CumulativePromoCreditsToGM,
                                    byte[] CumulativeNonCashableCreditsToGM,
                                    byte[] CumulativeCreditsToHost,
                                    byte[] CumulativeCashableCreditsToGM)
        {

            Byte[] gp = { address, 0x1D };
            gp = gp.ToList().Concat(CumulativePromoCreditsToGM.ToList())
                            .Concat(CumulativeNonCashableCreditsToGM.ToList())
                            .Concat(CumulativeCreditsToHost.ToList())
                            .Concat(CumulativeCashableCreditsToGM.ToList()).ToArray();
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }



        /// <summary>
        /// BUILD RESPONSE FOR LP1E
        /// </summary>
        /// <param name="address"></param>
        /// <param name="Bills1Accepted"></param>
        /// <param name="Bills5Accepted"></param>
        /// <param name="Bills10Accepted"></param>
        /// <param name="Bills20Accepted"></param>
        /// <param name="Bills50Accepted"></param>
        /// <param name="Bills100Accepted"></param>
        /// <returns></returns>
        public byte[] GetResponseForMultipleMetersLP1E(byte address,
                                                       byte[] Bills1Accepted,
                                                       byte[] Bills5Accepted,
                                                       byte[] Bills10Accepted,
                                                       byte[] Bills20Accepted,
                                                       byte[] Bills50Accepted,
                                                       byte[] Bills100Accepted)
        {
            byte[] header = new byte[] { address, 0x1E };
            byte[] body = join(Bills1Accepted,
                               Bills5Accepted,
                               Bills10Accepted,
                               Bills20Accepted,
                               Bills50Accepted,
                               Bills100Accepted);
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP1F
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameId"></param>
        /// <param name="additionalId"></param>
        /// <param name="denomination"></param>
        /// <param name="maxBet"></param>
        /// <param name="progressiveGroup"></param>
        /// <param name="gameOptions"></param>
        /// <param name="paytableId"></param>
        /// <param name="BasePercentage"></param>
        /// <returns></returns>
        public Byte[] GetResponseSendGamingMachineIDandInformationLongPoll(byte address,
                                                                           byte[] gameId,
                                                                           byte[] additionalId,
                                                                           byte denomination,
                                                                           byte maxBet,
                                                                           byte progressiveGroup,
                                                                           byte[] gameOptions,
                                                                           byte[] paytableId,
                                                                           byte[] BasePercentage)

        {

            byte[] header = new byte[] { address, 0x1F };
            byte[] body = join(gameId,
                               additionalId,
                               B(denomination),
                               B(maxBet),
                               B(progressiveGroup),
                               gameOptions,
                               paytableId,
                               BasePercentage);
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP19
        /// </summary>
        /// <param name="address"></param>
        /// <param name="total_coin_in"></param>
        /// <param name="total_coin_out"></param>
        /// <param name="total_drop"></param>
        /// <param name="total_jackpot"></param>
        /// <param name="games_played"></param>
        /// <returns></returns>
        public Byte[] GetResponseForLP19(Byte address,
                                         byte[] total_coin_in,
                                         byte[] total_coin_out,
                                         byte[] total_drop,
                                         byte[] total_jackpot,
                                         byte[] games_played)
        {
            Byte[] header = { address, 0x19 };
            byte[] gp = join(header,
                             total_coin_in,
                             total_coin_out,
                             total_drop,
                             total_jackpot,
                             games_played);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }



        /// <summary>
        /// BUILD RESPONSE FOR LP20
        /// </summary>
        /// <param name="address"></param>
        /// <param name="dollar_value"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP20(byte address,
                                         byte[] dollar_value)
        {
            Byte[] header = { address, 0x20 };
            byte[] gp = join(header,
                             dollar_value);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP27
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP27(byte address,
                                         byte[] value)
        {
            Byte[] header = { address, 0x27 };
            byte[] gp = join(header,
                             value);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP28
        /// </summary>
        /// <param name="address"></param>
        /// <param name="raw_logs"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP28(byte address,
                                         byte[] raw_logs)
        {
            Byte[] header = { address, 0x28 };
            byte[] gp = join(header,
                             raw_logs);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP2D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Byte[] GetResponseSendHandPaidCancelledCredits(byte address,
                                                             byte[] gameNumber,
                                                             byte[] value)
        {
            byte[] header = new byte[] { address, 0x2D };
            byte[] body = join(gameNumber,
                               value);
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP2F
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="metersResult"></param>
        /// <returns></returns>
        public byte[] GetResponse2F(byte address,
                                     byte[] gameNumber,
                                     byte[] metersResult)
        {
            Byte[] body = join(gameNumber,
                               metersResult);
            Byte[] gp = join(B(address),
                             B(0x2F),
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;

        }

        /// <summary>
        /// BUILD RESPONSE FOR LP48
        /// </summary>
        /// <param name="address"></param>
        /// <param name="countryCode"></param>
        /// <param name="denominationCode"></param>
        /// <param name="billMeter"></param>
        /// <returns></returns>
        public byte[] GetResponse48(byte address,
                                    byte countryCode,
                                    byte denominationCode,
                                    byte[] billMeter)
        {
            Byte[] body = join(B(countryCode),
                               B(denominationCode),
                               billMeter);
            Byte[] gp = join(B(address),
                             B(0x48),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;

        }

        /// <summary>
        /// BUILD RESPONSE FOR LP4D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="validationType"></param>
        /// <param name="indexNumber"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <param name="validationNumber"></param>
        /// <param name="amount"></param>
        /// <param name="ticketNumber"></param>
        /// <param name="validationSystemId"></param>
        /// <param name="expiration"></param>
        /// <param name="poolId"></param>
        /// <returns></returns>
        public Byte[] GetResponseEnhancedValidationInformation(byte address,
                                                               byte validationType,
                                                               byte indexNumber,
                                                               byte[] date,
                                                               byte[] time,
                                                               byte[] validationNumber,
                                                               uint amount,
                                                               byte[] ticketNumber,
                                                               byte validationSystemId,
                                                               byte[] expiration,
                                                               byte[] poolId)
        {
            Byte[] body = join(B(validationType),
                               B(indexNumber),
                               date,
                               time,
                               validationNumber,
                               intToBCD5_v2(amount),
                               ticketNumber,
                               B(validationSystemId),
                               expiration,
                               poolId);
            Byte[] gp = join(B(address),
                             B(0x4D),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP50
        /// </summary>
        /// <param name="address"></param>
        /// <param name="validationType"></param>
        /// <param name="totalValidations"></param>
        /// <param name="cumulativeAmount"></param>
        /// <returns></returns>
        public byte[] GetResponse50(byte address,
                                     byte validationType,
                                     int totalValidations,
                                     int cumulativeAmount)
        {
            byte[] totalValidations_byte = intToBCD5_v2((uint)totalValidations, 4);
            byte[] cumulativeAmount_byte = intToBCD5_v2((uint)cumulativeAmount);
            Byte[] body = join(B(validationType),
                               totalValidations_byte,
                               cumulativeAmount_byte);
            Byte[] gp = join(B(address),
                             B(0x50),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;

        }


        /// <summary>
        ///  BUILD RESPONSE FOR LP51
        /// </summary>
        /// <param name="address"></param>
        /// <param name="numberOfGames"></param>
        /// <returns></returns>
        public byte[] GetResponse51(byte address,
                                    uint numberOfGames)
        {
            byte[] gp = join(B(address),
                             B(0x51),
                             intToBCD5_v2(numberOfGames, 2));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        ///  BUILD RESPONSE FOR LP53
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="gameId"></param>
        /// <param name="additionalId"></param>
        /// <param name="denomination"></param>
        /// <param name="maxBet"></param>
        /// <param name="progressiveGroup"></param>
        /// <param name="gameOptions"></param>
        /// <param name="paytableId"></param>
        /// <param name="BasePercentage"></param>
        /// <returns></returns>
        public Byte[] GetResponseSendGameNConfiguration(byte address,
                                                        byte[] gameNumber,
                                                        byte[] gameId,
                                                        byte[] additionalId,
                                                        byte denomination,
                                                        byte maxBet,
                                                        byte progressiveGroup,
                                                        byte[] gameOptions,
                                                        byte[] paytableId,
                                                        byte[] BasePercentage)

        {

            byte[] header = new byte[] { address, 0x53 };
            byte[] body = join(gameNumber,
                               gameId,
                               additionalId,
                               B(denomination),
                               B(maxBet),
                               B(progressiveGroup),
                               gameOptions,
                               paytableId,
                               BasePercentage);
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP54
        /// </summary>
        /// <param name="address"></param>
        /// <param name="SASVersion"></param>
        /// <param name="GamingMachineSerialNumber"></param>
        /// <returns></returns>
        public byte[] GetResponseSendSASVersionIDAndGamingMachineSerialNumber(byte address,
                                                                              byte[] SASVersion,
                                                                              byte[] GamingMachineSerialNumber)
        {
            byte[] header = new byte[] { address, 0x54 };
            byte[] body = join(SASVersion,
                               GamingMachineSerialNumber);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP55
        /// </summary>
        /// <param name="address"></param>
        /// <param name="selected_game_number"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP55(byte address,
                                         byte[] selected_game_number)
        {
            byte[] header = new byte[] { address, 0x55 };
            byte[] gp = join(header,
                             selected_game_number);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP56
        /// </summary>
        /// <param name="address"></param>
        /// <param name="Enabled_Games"></param>
        /// <returns></returns>
        public byte[] GetResponseForLP56(byte address,
                                         List<byte[]> Enabled_Games)
        {
            byte[] header = new byte[] { address, 0x56 };
            byte games_count = (byte)Enabled_Games.Count();
            byte[] games = new byte[] { };
            foreach (byte[] g in Enabled_Games)
            {
                games = join(games,
                             g);
            }
            byte[] gp = join(B(games_count),
                             games);
            gp = join(header,
                      B((byte)gp.Length),
                      gp);

            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP57
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cashoutType"></param>
        /// <param name="cashoutAmount"></param>
        /// <returns></returns>
        public Byte[] GetResponsePendingCashoutInformation(byte address,
                                                        byte cashoutType,
                                                        byte[] cashoutAmount)
        {
            Byte[] gp = { address, 0x57, cashoutType };
            gp = gp.ToList().Concat(cashoutAmount.ToList()).ToArray();
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP58
        /// </summary>
        /// <param name="address"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Byte[] GetResponseReceiveValidationNumber(byte address,
                                                        byte status)
        {
            Byte[] gp = { address, 0x58, status };
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        ///  BUILD RESPONSE FOR LP6F
        /// </summary>
        /// <param name="command"></param>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="meters"></param>
        /// <returns></returns>
        public Byte[] GetResponseExtendedMeters(byte command,
                                                byte address,
                                                byte[] gameNumber,
                                                byte[] meters)
        {
            Byte[] gp = { address, command, (byte)(gameNumber.Length + meters.Length) };
            gp = gp.ToList().Concat(gameNumber.ToList())
                            .Concat(meters.ToList()).ToArray();
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP70
        /// </summary>
        /// <param name="address"></param>
        /// <param name="ticketStatus"></param>
        /// <param name="ticketAmount"></param>
        /// <param name="parsingCode"></param>
        /// <param name="validationData"></param>
        /// <returns></returns>
        public Byte[] GetResponseTicketValidationData(Byte address,
                                                      byte ticketStatus,
                                                      int ticketAmount,
                                                      byte parsingCode,
                                                      byte[] validationData)
        {

            Byte[] header = { address, 0x70 };
            Byte[] amount = intToBCD5((uint)ticketAmount);
            amount = amount.Reverse().ToArray();
            Byte[] body = join(B(ticketStatus),
                               amount,
                               B(parsingCode),
                               validationData);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP71
        /// </summary>
        /// <param name="address"></param>
        /// <param name="status"></param>
        /// <param name="transferAmount"></param>
        /// <param name="code"></param>
        /// <param name="validationData"></param>
        /// <returns></returns>
        public Byte[] GetResponseRedeemTicketGM(byte address,
                                                byte status,
                                                byte[] transferAmount,
                                                byte[] code,
                                                byte[] validationData)
        {
            byte[] header = new byte[] { address, 0x71 };
            byte[] body = join(B(status),
                               transferAmount,
                               code,
                               validationData);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP72
        /// </summary>
        /// <returns></returns>
        public Byte[] GetResponseLP72(byte address,
                                      byte transactionbufferposition,
                                      byte transferStatus,
                                      byte receiptStatus,
                                      byte transferType,
                                      byte[] cashableAmount,
                                      byte[] restrictedAmount,
                                      byte[] nonRestrictedAmount,
                                      byte transferFlags,
                                      byte[] assetNumber,
                                      byte[] transactionID,
                                      DateTime? TransactionDateTime,
                                      byte[] Expiration,
                                      byte[] poolId,
                                      byte[] cumulativeCashableAmount,
                                      byte[] cumulativeRestrictedAmount,
                                      byte[] cumulativeNonRestrictedAmount)
        {
            if (transferStatus == 0xC0)
            {
                byte[] header = new byte[] { address, 0x72 };
                byte[] body = join(B(transactionbufferposition),
                                   B(transferStatus));
                byte[] gp = join(header,
                B((byte)body.Length),
                body);
                byte[] crc_sign = GetCRCBytes(gp);
                gp = join(gp,
                        crc_sign);
                return gp;
            }
            if (transferStatus == 0xFF)
            {
                byte[] header = new byte[] { address, 0x72 };
                byte[] body = join(B(transactionbufferposition),
                                   B(transferStatus),
                                   B(receiptStatus));
                byte[] gp = join(header,
                B((byte)body.Length),
                body);
                byte[] crc_sign = GetCRCBytes(gp);
                gp = join(gp,
                        crc_sign);
                return gp;
            }
            else
            {
                byte[] transactionDate = TransactionDateTime == null ? new byte[] {0x00, 0x00, 0x00, 0x00} : join(intToBCD5_v2((uint)TransactionDateTime.Value.Month, 1),  intToBCD5_v2((uint)TransactionDateTime.Value.Day, 1), intToBCD5_v2((uint)TransactionDateTime.Value.Year, 2));
                byte[] transactionTime = TransactionDateTime == null ? new byte[] {0x00, 0x00, 0x00} : join(intToBCD5_v2((uint)TransactionDateTime.Value.Hour, 1),  intToBCD5_v2((uint)TransactionDateTime.Value.Minute, 1), intToBCD5_v2((uint)TransactionDateTime.Value.Second, 1));
                byte[] header = new byte[] { address, 0x72 };
                byte[] body = join(B(transactionbufferposition),
                                B(transferStatus),
                                B(receiptStatus),
                                B(transferType),
                                cashableAmount,
                                restrictedAmount,
                                nonRestrictedAmount,
                                B(transferFlags),
                                assetNumber,
                                B((byte)transactionID.Length),
                                transactionID,
                                transactionDate,
                                transactionTime,
                                Expiration,
                                poolId,
                                B((byte)cumulativeCashableAmount.Length),
                                cumulativeCashableAmount,
                                B((byte)cumulativeRestrictedAmount.Length),
                                cumulativeRestrictedAmount,
                                B((byte)cumulativeNonRestrictedAmount.Length),
                                cumulativeNonRestrictedAmount);
                byte[] gp = join(header,
                                B((byte)body.Length),
                                body);
                byte[] crc_sign = GetCRCBytes(gp);
                gp = join(gp,
                        crc_sign);
                return gp;
            }
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP74
        /// </summary>
        public byte[] GetResponseLP74(byte address,
                                        byte[] assetNumber,
                                        byte gameLockStatus,
                                        byte availableTrasfers,
                                        byte hostCashoutStatus,
                                        byte aftStatus,
                                        byte maxBuferIndex,
                                        byte[] currentCashableAmount,
                                        byte[] currentRestrictedAmount,
                                        byte[] currentNonRestrictedAmount,
                                        byte[] gamingMachineTransferLimit,
                                        DateTime restrictedExpiration,
                                        byte[] restrictedPoolID)
        {
            
            byte[] header = new byte[] { address, 0x74 };
            byte[] body = join(assetNumber,
                               B(gameLockStatus),
                               B(availableTrasfers),
                               B(hostCashoutStatus),
                               B(aftStatus),
                               B(maxBuferIndex),
                               currentCashableAmount,
                               currentRestrictedAmount,
                               currentNonRestrictedAmount,
                               gamingMachineTransferLimit,
                               intToBCD5_v2((uint)restrictedExpiration.Month, 1),  intToBCD5_v2((uint)restrictedExpiration.Day, 1), intToBCD5_v2((uint)restrictedExpiration.Year, 2),
                               restrictedPoolID);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP7B
        /// </summary>
        /// <param name="address"></param>
        /// <param name="assetNumber"></param>
        /// <param name="statusBits"></param>
        /// <param name="cashableTicketAndReceiptExpiration"></param>
        /// <param name="restrictedTicketDefaultExpiration"></param>
        /// <returns></returns>
        public Byte[] GetResponseExtendedValidationStatusGamingMachine(byte address,
                                                                       byte[] assetNumber,
                                                                       byte[] statusBits,
                                                                       byte[] cashableTicketAndReceiptExpiration,
                                                                       byte[] restrictedTicketDefaultExpiration)
        {
            byte[] header = new byte[] { address, 0x7B };
            byte[] body = join(assetNumber,
                               statusBits,
                               cashableTicketAndReceiptExpiration,
                               restrictedTicketDefaultExpiration);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP7C
        /// </summary>
        /// <param name="address"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public Byte[] GetResponseExtendedTicketData(byte address,
                                                    byte flag)
        {
            Byte[] gp = { address, 0x7C, flag };
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP7D
        /// </summary>
        /// <param name="address"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public byte[] GetResponseTicketData(byte address,
                                            byte flag)
        {
            byte[] gp = { address, 0x7D, flag };
            byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP7E
        /// </summary>
        /// <param name="address"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public byte[] GetResponseForDateTimeLP7E(byte address,
                                                 int month,
                                                 int year,
                                                 int day,
                                                 int hour,
                                                 int minute,
                                                 int second)
        {
            byte[] header = new byte[] { address, 0x7E };
            byte[] date = join(intToBCD5_v2((uint)month, 1),
                               intToBCD5_v2((uint)day, 1),
                               intToBCD5_v2((uint)year, 2));
            byte[] time = join(intToBCD5_v2((uint)hour, 1),
                               intToBCD5_v2((uint)minute, 1),
                               intToBCD5_v2((uint)second, 1));
            byte[] gp = join(header,
                             date,
                             time);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP83
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="CumulativeProgressiveWins"></param>
        /// <returns></returns>
        public byte[] GetResponseLP83(byte address,
                                      byte[] gameNumber,
                                      byte[] CumulativeProgressiveWins)
        {
            byte[] header = new byte[] { address, 0x83 };
            byte[] gp = join(header,
                             gameNumber,
                             CumulativeProgressiveWins);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        ///  BUILD RESPONSE FOR LP84 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="level"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public byte[] GetResponseLP84(byte address,
                                      byte group,
                                      byte level,
                                      int amount)
        {
            byte[] header = new byte[] { address, 0x84 };
            byte[] gp = join(header,
                             B(group),
                             B(level),
                             intToBCD5_v2((uint)amount));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP85
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="level"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public byte[] GetResponseLP85(byte address,
                                      byte group,
                                      byte level,
                                      int amount)
        {
            byte[] header = new byte[] { address, 0x85 };
            byte[] gp = join(header,
                             B(group),
                             B(level),
                             intToBCD5_v2((uint)amount));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP87
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="AmountsAndLevels"></param>
        /// <returns></returns>
        public byte[] GetResponseLP87(byte address,
                                      byte group,
                                      List<Tuple<byte, int>> AmountsAndLevels)
        {
            byte[] header = new byte[] { address, 0x87 };
            byte[] amounts_and_levels = new byte[] { };
            foreach (Tuple<byte, int> t in AmountsAndLevels)
            {
                amounts_and_levels = join(amounts_and_levels,
                                          B(t.Item1),
                                          intToBCD5_v2((uint)t.Item2));
            }
            byte[] body = join(B(group),
                               B((byte)AmountsAndLevels.Count()),
                               amounts_and_levels);
            byte[] gp = join(header,
                             B((byte)body.Count()),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LP94
        /// </summary>
        /// <param name="address"></param>
        /// <param name="resetCode"></param>
        /// <returns></returns>
        public byte[] GetResponseResetHandpayGaming(byte address,
                                                    byte resetCode)
        {
            Byte[] gp = join(B(address),
                             B(0x94),
                             B(resetCode));
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        ///  BUILD LP9A
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="deductible"></param>
        /// <param name="nonDeductible"></param>
        /// <param name="wagetMatch"></param>
        /// <returns></returns>
        public byte[] GetResponseLP9A(byte address,
                                      byte[] gameNumber,
                                      byte[] deductible,
                                      byte[] nonDeductible,
                                      byte[] wagetMatch)
        {
            Byte[] gp = join(B(address),
                             B(0x9A),
                             gameNumber,
                             deductible,
                             nonDeductible,
                             wagetMatch);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPA0
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="feat1"></param>
        /// <param name="feat2"></param>
        /// <param name="feat3"></param>
        /// <returns></returns>
        public Byte[] GetResponseSendEnabledFeatures(byte address,
                                                     byte[] gameNumber,
                                                     byte feat1,
                                                     byte feat2,
                                                     byte feat3)
        {

            byte[] header = new byte[] { address, 0xA0 };
            byte[] body = join(gameNumber,
                               B(feat1),
                               B(feat2),
                               B(feat3),
                               new byte[] { 0x0, 0x0, 0x0 });
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPA0
        /// </summary>
        /// <param name="address"></param>
        /// <param name="feat1"></param>
        /// <param name="feat2"></param>
        /// <param name="feat3"></param>
        /// <returns></returns>
        public Byte[] GetResponseA0(Byte address,
                                    byte[] feat1,
                                    byte[] feat2,
                                    byte[] feat3)
        {

            Byte[] gp = { address, 0xA0 };
            gp = gp.ToList().Concat(feat1.ToList())
                            .Concat(feat2.ToList())
                            .Concat(feat3.ToList()).ToArray();
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPA4
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="cashoutLimit"></param>
        /// <returns></returns>
        public byte[] GetResponseSendCashoutLimitGamingMachine(byte address,
                                                               byte[] gameNumber,
                                                               byte[] cashoutLimit)
        {
            byte[] header = new byte[] { address, 0xA4 };
            byte[] body = join(gameNumber,
                               cashoutLimit);
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPA8
        /// </summary>
        /// <param name="address"></param>
        /// <param name="ACKCode"></param>
        /// <returns></returns>
        public byte[] GetResponseEnableJackpotHandpayResetMethod(byte address,
                                                                 byte ACKCode)
        {
            byte[] header = new byte[] { address, 0xA8 };
            byte[] body = join(B(ACKCode));
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }



        /// <summary>
        /// BUILD RESPONSE FOR LPB1
        /// </summary>
        /// <param name="address"></param>
        /// <param name="CurrentPlayerDenomination"></param>
        /// <returns></returns>
        public byte[] GetResponseForLPB1(byte address,
                                         byte CurrentPlayerDenomination)
        {
            byte[] header = new byte[] { address, 0xB1 };
            byte[] gp = join(header,
                             B(CurrentPlayerDenomination));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        ///  BUILD RESPONSE FOR LPB2
        /// </summary>
        /// <param name="address"></param>
        /// <param name="NumberOfDenominations"></param>
        /// <param name="PlayerDenominations"></param>
        /// <returns></returns>
        public byte[] GetResponseForLPB2(byte address,
                                         byte NumberOfDenominations,
                                         byte[] PlayerDenominations)
        {
            Byte[] header = { address, 0xB2 };
            byte[] body = join(B(NumberOfDenominations),
                               PlayerDenominations);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPB3
        /// </summary>
        /// <param name="address"></param>
        /// <param name="TokenDenomination"></param>
        /// <returns></returns>
        public byte[] GetResponseForLPB3(byte address,
                                         byte TokenDenomination)
        {
            Byte[] header = { address, 0xB3 };
            byte[] gp = join(header,
                             B(TokenDenomination));
            byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LPB4
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="wagerCategory"></param>
        /// <param name="paybackPercentage"></param>
        /// <param name="coinInMeterValue"></param>
        /// <returns></returns>
        public byte[] GetResponseForLPB4(byte address,
                                         byte[] gameNumber,
                                         byte[] wagerCategory,
                                         byte[] paybackPercentage,
                                         byte[] coinInMeterValue)
        {
            Byte[] header = { address, 0xB4 };
            byte[] body = join(gameNumber, 
                               wagerCategory,
                               paybackPercentage,
                               B((byte)coinInMeterValue.Length),
                               coinInMeterValue);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = gp.ToList().Concat(crc_sign.ToList()).ToArray();

            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPB5
        /// </summary>
        /// <param name="address"></param>
        /// <param name="gameNumber"></param>
        /// <param name="maxBet"></param>
        /// <param name="progressiveGroup"></param>
        /// <param name="progressiveLevels"></param>
        /// <param name="gameNameLength"></param>
        /// <param name="gameName"></param>
        /// <param name="paytableLength"></param>
        /// <param name="paytableName"></param>
        /// <param name="wagerCategories"></param>
        /// <returns></returns>
        public Byte[] GetResponseExtendedGameInformation(byte address,
                                                          byte[] gameNumber,
                                                          byte[] maxBet,
                                                          byte progressiveGroup,
                                                          byte[] progressiveLevels,
                                                          byte gameNameLength,
                                                          string gameName,
                                                          byte paytableLength,
                                                          string paytableName,
                                                          byte[] wagerCategories)
        {
            Encoding encoding = Encoding.Default;
            byte[] header = new byte[] { address, 0xB5 };
            byte[] body = join(gameNumber,
                               maxBet,
                               B(progressiveGroup),
                               progressiveLevels,
                               B(gameNameLength),
                               encoding.GetBytes(gameName),
                               B(paytableLength),
                               encoding.GetBytes(paytableName),
                               wagerCategories);
            byte[] gp = join(header,
                             B((byte)body.Length),
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPFF
        /// </summary>
        /// <param name="address"></param>
        /// <param name="exception"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Byte[] GetResponseException(byte address,
                                           byte exception,
                                           byte[] data)
        {
            byte[] header = new byte[] { address, 0xFF };
            byte[] body = join(B(exception),
                               data);
            byte[] gp = join(header,
                             body);
            byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        /// <summary>
        /// BUILD RESPONSE FOR LPFF
        /// </summary>
        /// <param name="address"></param>
        /// <param name="exceptionCode"></param>
        /// <param name="data"></param>
        /// <param name="real_time"></param>
        /// <returns></returns>
        public byte[] GetResponseException(byte address,
                                           byte exceptionCode,
                                           byte[] data,
                                           bool real_time)
        {
            if (real_time)
            {
                Byte[] body = join(B(exceptionCode),
                                   data);
                Byte[] gp = join(B(address),
                                 B(0xFF),
                                 body);
                byte[] crc_sign = GetCRCBytes(gp);
                gp = join(gp,
                          crc_sign);
                return gp;
            }
            else
            {
                return new byte[] { exceptionCode };
            }
        }



        /// <summary>
        /// BUILD RESPONSE FOR REG
        /// </summary>
        /// <param name="address"></param>
        /// <param name="command"></param>
        /// <param name="length"></param>
        /// <param name="reg_status"></param>
        /// <param name="asset_number"></param>
        /// <param name="registration_key"></param>
        /// <param name="pos_id"></param>
        /// <returns></returns>
        public Byte[] GetResponseRegistration(byte address,
                                              byte command,
                                              byte reg_status,
                                              byte[] asset_number,
                                              byte[] registration_key,
                                              byte[] pos_id)
        {

            Byte[] lp = new byte[] { address, command};

            byte[] body = new byte[] {reg_status};
            body = body.ToList().Concat(asset_number.ToList()).ToArray();

            body = body.ToList().Concat(registration_key.ToList()).ToArray();

            body = body.ToList().Concat(pos_id.ToList()).ToArray();

            lp = lp.ToList().Concat((new byte[] { (byte)body.Length}).ToList()).Concat(body.ToList()).ToArray();

            CyclicalRedundancyCheck crcGen = new CyclicalRedundancyCheck();
            Byte[] _crc = crcGen.GetCRCBytes(lp);

            lp = lp.ToList().Concat(_crc.ToList()).ToArray();

            return lp;
        }



        /// <summary>
        /// BUILD RESPONSE FOR GetResponseHostSingleMeterAccounting
        /// </summary>
        /// <param name="address"></param>
        /// <param name="single_meter_accounting_long_poll"></param>
        /// <param name="meter"></param>
        /// <returns></returns>
        public byte[] GetResponseHostSingleMeterAccounting(byte address,
                                                           byte single_meter_accounting_long_poll,
                                                           byte[] meter)
        {
            Byte[] gp = join(B(address),
                             B(single_meter_accounting_long_poll),
                             meter);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }

        /// <summary>
        /// BUILD RESPONSE FOR LP21 ROM Signature Verification
        /// </summary>
        /// <param name="address"></param>
        /// <param name="romSignature"></param>
        /// <returns></returns>
        public byte[] GetResponseForROMSignature(byte address,
                                                byte[] romSignature)
        {
            byte[] header = new byte[] { address, 0x21 };
            Byte[] gp = join(header, romSignature);
            Byte[] crc_sign = GetCRCBytes(gp);
            gp = join(gp,
                      crc_sign);
            return gp;
        }


        #endregion




        /// <summary>
        /// Int to BCD5:Toma un entero y devuelve el equivalente en lectura y bytes
        /// Int to BCD5: With an integer as argument, returns the value as byte array in BCD format
        /// </summary>
        /// <param name="numericvalue"></param>
        /// <param name="bytesize"></param>
        /// <returns></returns>
        public byte[] intToBCD5(long numericvalue, int bytesize = 5)
        {
            byte[] bcd = new byte[bytesize];
            for (int byteNo = 0; byteNo < bytesize; ++byteNo)
                bcd[byteNo] = 0;
            for (int digit = 0; digit < bytesize * 2; ++digit)
            {
                long hexpart = numericvalue % 10;
                bcd[digit / 2] |= (byte)(hexpart << ((digit % 2) * 4));
                numericvalue /= 10;
            }
            return bcd;
        }


        /// <summary>
        /// Int to BCD5:Toma un entero y devuelve el equivalente en lectura y bytes, pero de reversa
        /// Int to BCD5: With an integer as arguments, returns the value as byte array in BCD format but in reverse
        /// </summary>
        /// <param name="numericvalue"></param>
        /// <param name="bytesize"></param>
        /// <returns></returns>
        private byte[] intToBCD5_v2(long numericvalue, int bytesize = 5)
        {
            byte[] bcd = new byte[bytesize];
            for (int byteNo = 0; byteNo < bytesize; ++byteNo)
                bcd[byteNo] = 0;
            for (int digit = 0; digit < bytesize * 2; ++digit)
            {
                long hexpart = numericvalue % 10;
                bcd[digit / 2] |= (byte)(hexpart << ((digit % 2) * 4));
                numericvalue /= 10;
            }

            bcd = bcd.Reverse().ToArray();
            return bcd;
        }




    }
}
