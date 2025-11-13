
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
// using BitbossInterface;

namespace SASComms
{

    /// <summary>
    /// The SASClient has conection with SMIB. Contains primitives from a library SerialPortController for use, at UNIX level, the serial port.
    /// It has different events that they are sent to VirtualEGM, i.e by a command receiving from SMIB or by a link down
    /// Contains different functions builders of long poll responses to SMIB
    /// </summary>
    public class SASClient
    {



        #region PRIMITIVAS PARA MANEJAR EL PUERTO SERIAL A TRAVES DE TERMIOS

        [DllImport("SerialPortController", EntryPoint = "OpenPort")]
        public static extern int OpenPort(string portName);

        [DllImport("SerialPortController", EntryPoint = "ClosePort")]
        public static extern void ClosePort(int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "readBytes")]
        public static extern int readBytes(byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "readSpaceBytes2")]
        public static extern int readSpaceBytes2(byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "writeBytes")]
        public static extern void writeBytes(byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "set_wakeup")]
        public static extern int set_wakeup(int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "set_space")]
        public static extern int set_space(int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "flushPort")]
        public static extern void flushPort(int PortFileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "flushOutputPort")]
        public static extern void flushOutputPort(int PortFileDescriptor);

        #endregion


        /*  Calls to Primitives methods   */

        #region Calls to Primitives methods

        /// <summary>
        /// This method closes the serial port
        /// </summary>
        private void closePort()
        {

            ClosePort(SerialPortFileDescriptor);
        }

        /// <summary>
        /// Dado un array (o lista) de array de bytes, la idea es "aplanar" la lista en un sólo array que concatene cada elemento
        /// Given an array or list of byte array, this functions concatenates each byte arrays to create a single array
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
        /// Reads the serial port and returns the byte array read
        /// </summary>
        /// <returns></returns>
        private byte[] readPort()
        {
            // buf, it is the result to return
            byte[] buf = new byte[] { };
            byte[] buffer1 = new byte[128];
            // Read for the first time and save the ret value in buffer1
            int howmuch1 = readBytes(buffer1, buffer1.Length, SerialPortFileDescriptor) + 1;
            bool notempty = howmuch1 > 0;
            // In buffer1, take the non-null value of 'howmuch1' length and discard the rest
            buffer1 = buffer1.ToList().Take(howmuch1).ToArray();
            // if (howmuch1 == 1 && buffer1[0] != 0x80 && buffer1[0] != 0x81) Console.WriteLine("readPort() " + BitConverter.ToString(buffer1));
            // Concatenates the buf to buffer1
            buf = join(buf, buffer1);
            /// While CRC is not true in buf, and buffer1 is still having at least one element
            while (!CheckCRC(buf) && notempty)
            {
                byte[] buffer2 = new byte[128];
                /// Read again the serial port and assign its value in buffer2 
                int howmuch2 = readSpaceBytes2(buffer2, buffer2.Length, SerialPortFileDescriptor) + 1;
                // notempty representa el valor de verdad de si el buffer2 no está vacío
                /// notempty represents the truth value of if buffer2 is not empty
                notempty = howmuch2 > 0;
                // In buffer2, take the non-null value of 'howmuch2' length and discard the rest
                buffer2 = buffer2.ToList().Take(howmuch2).ToArray();
                howmuch1 += howmuch2;
                // Console.WriteLine("2: " + BitConverter.ToString(buffer2));
                // A buf le concateno buffer2, lo que leí
                // Concatenates the buf to buffer2
                buf = join(buf, buffer2);
                // Check again if the loop condition is true, i.e if buf is completed or not
            }
            // Return buf
            // if (buf.Length == 1 && buffer1[0] != 0x80 && buffer1[0] != 0x81 && buffer1[0] != 0x82)
            //     Console.WriteLine("readPort() " + BitConverter.ToString(buffer1));
            return buf;
        }

        #endregion


        #region DECLARACIONES

        private EventArgs ee;



        /// <summary>
        /// Client property. Let client write every log in console if it is in true, or nothing otherwise
        /// </summary>
        bool print = false;
        /// <summary>
        /// Client property. Acts as state when the client is stopped (without receiving any long poll) or running
        /// </summary>
        bool stopped = true;
        /// <summary>
        /// Client property. Acts as state when the client is writing in the serial port
        /// </summary>
        bool writing = false;
        /// <summary>
        /// Client property. Acts as state when the client is disabled
        /// </summary>
        bool disabled = true;

        public bool? communication = null;

        DateTime LastMSGRecvdOrSend;
        int ms = 0;

        /// The client will have the following properties
        static int SerialPortFileDescriptor; // The Serial port file descriptor 
        private static System.Timers.Timer TimerMaxForStartChirp; // A timer that sets the max waiting time for start chirp
        private static System.Timers.Timer TimerMaxForData; // A timer that sets the max waiting time to receive data
        private static System.Timers.Timer TimerChirping; // A timer that sets the chirp interval
        private string smib_communication_enabled_msg = "SMIB COMMUNICATION ENABLED";
        private string smib_communication_link_down = "SMIB COMMUNICATION LINK DOWN";

        private byte? ACKByte; // Ack byte
        private byte? NACKByte; // NACK byte
        private byte? AddressByte; // The address
        private byte? GeneralPollByte; // The general poll byte 
        private bool? RealTimeBool; // A flag that indicates that there is events or exceptions in real time 
        private byte[] last_byteArr_received;  //  the last byte array or long poll received 
        private byte[] last_byteArr_sent; // The last sent byte array or longpoll
        private enum ChirpingStatus
        {
            Chirping,
            PollingNoSync,
            PollingSync,
            Stopped
        }
        private ChirpingStatus chirpStatus = ChirpingStatus.Stopped;

        #endregion



        #region "Events"
        /* We have some events throwed by SASClient to VirtualEGM, like requests or events */
        // Getting adress of Virtual EGM
        public event VirtualEGMGetAddressHandler VirtualEGMGetAddress; public delegate byte VirtualEGMGetAddressHandler(EventArgs e);
        // Getting real_time of Virtual EGM
        public event VirtualEGMGetEnabledRealTimeHandler VirtualEGMGetEnabledRealTime; public delegate bool VirtualEGMGetEnabledRealTimeHandler(EventArgs e);
        // A0 of VirtualEGM
        public event VirtualEGMA0Handler VirtualEGMA0; public delegate void VirtualEGMA0Handler(byte address, byte[] gameNumber, EventArgs e);
        // A8 of VirtualEGM
        public event VirtualEGMA8Handler VirtualEGMA8; public delegate void VirtualEGMA8Handler(byte address, byte resetMethod, EventArgs e);
        // A4 of VirtualEGM
        public event VirtualEGMA4Handler VirtualEGMA4; public delegate void VirtualEGMA4Handler(byte address, byte[] gameNumber, EventArgs e);
        // 1B from VirtualEGM
        public event VirtualEGM1BHandler VirtualEGM1B; public delegate void VirtualEGM1BHandler(byte address, EventArgs e);
        // 1F from VirtualEGM
        public event VirtualEGM1FHandler VirtualEGM1F; public delegate void VirtualEGM1FHandler(byte address, EventArgs e);
        // 1E from VirtualEGM
        public event VirtualEGM1EHandler VirtualEGM1E; public delegate void VirtualEGM1EHandler(byte address, EventArgs e);
        // 01 from VirtualEGM
        public event VirtualEGM01Handler VirtualEGM01; public delegate void VirtualEGM01Handler(byte address, EventArgs e);
        // 02 from VirtualEGM
        public event VirtualEGM02Handler VirtualEGM02; public delegate void VirtualEGM02Handler(byte address, EventArgs e);
        // 03 from VirtualEGM
        public event VirtualEGM03Handler VirtualEGM03; public delegate void VirtualEGM03Handler(byte address, EventArgs e);
        // 04 from VirtualEGM
        public event VirtualEGM04Handler VirtualEGM04; public delegate void VirtualEGM04Handler(byte address, EventArgs e);
        // 06 from VirtualEGM
        public event VirtualEGM06Handler VirtualEGM06; public delegate void VirtualEGM06Handler(byte address, EventArgs e);
        // 07 from VirtualEGM
        public event VirtualEGM07Handler VirtualEGM07; public delegate void VirtualEGM07Handler(byte address, EventArgs e);
        // 18 from VirtualEGM
        public event VirtualEGMLP18Handler VirtualEGMLP18; public delegate void VirtualEGMLP18Handler(byte address, EventArgs e);
        // 19 from VirtualEGM
        public event VirtualEGMLP19Handler VirtualEGMLP19; public delegate void VirtualEGMLP19Handler(byte address, EventArgs e);
        // 20 from VirtualEGM
        public event VirtualEGMLP20Handler VirtualEGMLP20; public delegate void VirtualEGMLP20Handler(byte address, EventArgs e);
        // 21 of VirtualEGM
        public event VirtualEGM21Handler VirtualEGM21; public delegate void VirtualEGM21Handler(byte address, byte[] seedValue, EventArgs e);
        // 27 from VirtualEGM
        public event VirtualEGMLP27Handler VirtualEGMLP27; public delegate void VirtualEGMLP27Handler(byte address, EventArgs e);
        // 28 from VirtualEGM
        public event VirtualEGMLP28Handler VirtualEGMLP28; public delegate void VirtualEGMLP28Handler(byte address, EventArgs e);
        // 48 from VirtualEGM
        public event VirtualEGMLP48Handler VirtualEGMLP48; public delegate void VirtualEGMLP48Handler(byte address, EventArgs e);
        // 2A from VirtualEGM
        public event VirtualEGM2AHandler VirtualEGM2A; public delegate void VirtualEGM2AHandler(byte address, EventArgs e);
        // 2B from VirtualEGM
        public event VirtualEGM2BHandler VirtualEGM2B; public delegate void VirtualEGM2BHandler(byte address, EventArgs e);
        // 74 from VirtualEGM
        public event VirtualEGM74Handler VirtualEGM74; public delegate void VirtualEGM74Handler(byte address, byte lockCode, byte transferCondition, byte[] lockTimeout, EventArgs e);
        // 7E from VirtualEGM
        public event VirtualEGM7EHandler VirtualEGM7E; public delegate void VirtualEGM7EHandler(byte address, EventArgs e);

        // LP Distinct
        public event LPDistinctHandler LPDistinct; public delegate void LPDistinctHandler(EventArgs e);
        // Send Enhanced Validation Information 4D from VirtualEGM
        public event VirtualEGM4DHandler VirtualEGM4D; public delegate void VirtualEGM4DHandler(byte address, byte functionCode, EventArgs e);
        // Send Validation Meters 50 from VirtualEGM
        public event VirtualEGM50Handler VirtualEGM50; public delegate void VirtualEGM50Handler(byte address, byte validationType, EventArgs e);
        // Send Meters from VirtualEGM
        public event VirtualEGMSendMetersHandler VirtualEGMSendMeters; public delegate void VirtualEGMSendMetersHandler(byte address, EventArgs e);
        // Send SAS Version ID And Game Serial Number;
        public event VirtualEGMSendSASVersionIDAndGameSerialNumberHandler VirtualEGMSendSASVersionIDAndGameSerialNumber; public delegate void VirtualEGMSendSASVersionIDAndGameSerialNumberHandler(byte address, EventArgs e);
        // Send Number of Games Implemented from VirtualEGM
        public event VirtualEGMSendNumberOfGamesImplementedHandler VirtualEGMSendNumberOfGamesImplemented; public delegate void VirtualEGMSendNumberOfGamesImplementedHandler(byte address, EventArgs e);
        // Send Selected Meters from VirtualEGM
        public event VirtualEGMSendSelectedMetersHandler VirtualEGMSendSelectedMeters; public delegate void VirtualEGMSendSelectedMetersHandler(byte address, byte[] gameNumber,
                                                                                                                                                byte[] meters,
                                                                                                                                                EventArgs e);
        // Send Wager Category Game Information from Virtual EGM                                                                                                                
        public event VirtualEGMSendWagerCategoryInformationHandler VirtualEGMSendWagerCategoryInformation; public delegate void VirtualEGMSendWagerCategoryInformationHandler(byte address, byte[] gameNumber, byte[] wagerCategory, EventArgs e);
        // Send Extended Game N Information from Virtual EGM
        public event VirtualEGMSendExtendedGameInformationHandler VirtualEGMSendExtendedGameInformation; public delegate void VirtualEGMSendExtendedGameInformationHandler(byte address, byte[] gameNumber, EventArgs e);
        // Reset handpay from VirtualEGM
        public event VirtualEGMResetHandpayHandler VirtualEGMResetHandpay; public delegate void VirtualEGMResetHandpayHandler(byte address, EventArgs e);
        // Send Meters from VirtualEGM
        public event VirtualEGM0EHandler VirtualEGM0E; public delegate void VirtualEGM0EHandler(byte address, byte Enable_Disable, EventArgs e);
        // Send Meters from VirtualEGM
        public event VirtualEGM0FHandler VirtualEGM0F; public delegate void VirtualEGM0FHandler(byte address, EventArgs e);
        // Host Single Meter Accounting from LongPoll
        public event VirtualEGMHostSingleMeterAccountingHandler VirtualEGMHostSingleMeterAccounting; public delegate void VirtualEGMHostSingleMeterAccountingHandler(byte address, byte single_meter_accounting_long_poll, EventArgs e);

        // Send Extended Meters for Game N (gameNumber) from VirtualEGM
        public event VirtualEGMSendExtendedMetersHandler VirtualEGMSendExtendedMeters; public delegate void VirtualEGMSendExtendedMetersHandler(byte address, byte command,
                                                                                                                                                byte[] gameNumber,
                                                                                                                                                byte[] requestedMeterCode,
                                                                                                                                                List<byte[]> additionalMeterCodes, EventArgs e);
        // Receive Date and Time from VirtualEGM
        public event VirtualEGMReceiveDateAndTimeHandler VirtualEGMReceiveDateAndTime; public delegate void VirtualEGMReceiveDateAndTimeHandler(byte address,
                                                                                                                                                byte[] date,
                                                                                                                                                byte[] time, EventArgs e);
        // Response Pending Cashout Information from VirtualEGM
        public event VirtualEGMResponsePendingCashoutInformationHandler VirtualEGMResponsePendingCashoutInformation; public delegate void VirtualEGMResponsePendingCashoutInformationHandler(byte address, EventArgs e);
        // Register Gaming Machine
        public event VirtualEGMRegisterGamingMachine73Handler VirtualEGMRegisterGamingMachine; public delegate void VirtualEGMRegisterGamingMachine73Handler(byte address, byte RegistrationCode, EventArgs e);
        // Send Hand paid cancelled credits from Virtual EGM
        public event VirtualEGMSendHandPaidCancelledCreditsHandler VirtualEGMSendHandPaidCancelledCredits; public delegate void VirtualEGMSendHandPaidCancelledCreditsHandler(byte address, byte[] gameNumber, EventArgs e);
        // Register Gaming Machine
        public event VirtualEGMRegisterGamingMachine73_1Handler VirtualEGMRegisterGamingMachine1; public delegate void VirtualEGMRegisterGamingMachine73_1Handler(byte address, byte RegistrationCode,
                                                                                                                                                        byte[] AssetNumber,
                                                                                                                                                        byte[] RegistrationKey,
                                                                                                                                                        byte[] POSID, EventArgs e);
        // Extended Validation Status
        public event VirtualEGMExtendedValidationStatusHandler VirtualEGMExtendedValidationStatus; public delegate void VirtualEGMExtendedValidationStatusHandler(byte address,
                                                                                                                                                                  byte[] controlMask,
                                                                                                                                                                  byte[] statusBitControlStates,
                                                                                                                                                                  byte[] cashableTicketAndReceiptExpiration,
                                                                                                                                                                  byte[] restrictedTicketDefaultExpiration, EventArgs e);
        // Receive Validation Number from Host   
        public event VirtualEGMReceiveValidationNumberHandler VirtualEGMReceiveValidationNumber; public delegate void VirtualEGMReceiveValidationNumberHandler(byte address, byte validationSystemID,


                                                                                                                                                  byte[] validationNumber, EventArgs e);
        // Set Ticket Data 7D
        public event VirtualEGMSetTicketData7DHandler VirtualEGMSetTicketData7D; public delegate void VirtualEGMSetTicketData7DHandler(byte address, byte[] HostID,
                                                                                                                                       byte expiration,
                                                                                                                                       byte locationLength,
                                                                                                                                       byte[] locationData,
                                                                                                                                       byte address1Length,
                                                                                                                                       byte[] address1Data,
                                                                                                                                       byte address2Length,
                                                                                                                                       byte[] address2Data,
                                                                                                                                       EventArgs e);

        // Set Ticket Data 7D With 00
        public event VirtualEGMSetTicketData7DWith00Handler VirtualEGMSetTicketData7DWith00; public delegate void VirtualEGMSetTicketData7DWith00Handler(byte address,
                                                                                                                                                         EventArgs e);

        // Set Extended Ticket Data
        public event VirtualEGMSetExtendedTicketDataHandler VirtualEGMSetExtendedTicketData; public delegate void VirtualEGMSetExtendedTicketDataHandler(byte address, List<Tuple<byte, string>> data_elements, EventArgs e);

        // Send Ticket Validation Data
        public event VirtualEGMSendTicketValidationDataHandler VirtualEGMSendTicketValidationData; public delegate void VirtualEGMSendTicketValidationDataHandler(byte address, EventArgs e);

        // Redeem Ticket
        public event VirtualEGMRedeemTicket4Handler VirtualEGMRedeemTicket4; public delegate void VirtualEGMRedeemTicket4Handler(byte address, byte transferCode,
                                                                                                                              byte[] transferAmount,
                                                                                                                              byte parsingCode,
                                                                                                                              byte[] validationData, EventArgs e);
        // Configure Bill Denominations Command
        public event VirtualEGMConfigureBillDenominationsHandler VirtualEGMConfigureBillDenomination; public delegate void VirtualEGMConfigureBillDenominationsHandler(byte address, byte[] billDenominations,
                                                                                                                                                                       byte billAcceptor, EventArgs e);
        // Redeem Ticket
        public event VirtualEGMRedeemTicket1Handler VirtualEGMRedeemTicket1; public delegate void VirtualEGMRedeemTicket1Handler(byte address, byte transferCode, EventArgs e);
        // Redeem Ticket
        public event VirtualEGMSetSecureEnhancedValidationIDHandler VirtualEGMSetSecureEnhancedValidationID; public delegate void VirtualEGMSetSecureEnhancedValidationIDHandler(byte address, byte[] machineID,
                                                                                                                                                                                 byte[] sequenceNumber, EventArgs e);
        // Send Total Cancelled Credits
        public event VirtualEGMSendTotalCancelledCreditsHandler VirtualEGMSendTotalCancelledCredits; public delegate void VirtualEGMSendTotalCancelledCreditsHandler(byte address, EventArgs e);
        // Send Total Coin In Meter
        public event VirtualEGMSendTotalCoinInMeterHandler VirtualEGMSendTotalCoinInMeter; public delegate void VirtualEGMSendTotalCoinInMeterHandler(byte address, EventArgs e);
        // Send Total Coin Out Meter
        public event VirtualEGMSendTotalCoinOutMeterHandler VirtualEGMSendTotalCoinOutMeter; public delegate void VirtualEGMSendTotalCoinOutMeterHandler(byte address, EventArgs e);
        //Send Total Drop Meter
        public event VirtualEGMSendTotalDropMeterHandler VirtualEGMSendTotalDropMeter; public delegate void VirtualEGMSendTotalDropMeterHandler(byte address, EventArgs e);
        //  Send Total Jackpot Meter
        public event VirtualEGMSendTotalJackpotMeterHandler VirtualEGMSendTotalJackpotMeter; public delegate void VirtualEGMSendTotalJackpotMeterHandler(byte address, EventArgs e);
        // Send Games Played Meter
        public event VirtualEGMSendGamesPlayedMeterHandler VirtualEGMSendGamesPlayedMeter; public delegate void VirtualEGMSendGamesPlayedMeterHandler(byte address, EventArgs e);
        //  Send Games Won Meter
        public event VirtualEGMSendGamesWonMeterHandler VirtualEGMSendGamesWonMeter; public delegate void VirtualEGMSendGamesWonMeterHandler(byte address, EventArgs e);
        // Virtual EGM Send Games Lost
        public event VirtualEGMSendGamesLostMeterHandler VirtualEGMSendGamesLostMeter; public delegate void VirtualEGMSendGamesLostMeterHandler(byte address, EventArgs e);
        // Virtual EGM Bill Meter Requested
        public event BillMeterRequestedHandler BillMeterRequested; public delegate void BillMeterRequestedHandler(byte address, int bill, EventArgs e);
        // Virtual EGM Current Player Denomination
        public event VirtualEGMSendCurrentPlayerDenominationHandler VirtualEGMSendCurrentPlayerDenomination; public delegate void VirtualEGMSendCurrentPlayerDenominationHandler(byte address, EventArgs e);
        // Virtual EGM Enabled Games Number
        public event VirtualEGMSendEnabledGameNumbersHandler VirtualEGMSendEnabledGameNumbers; public delegate void VirtualEGMSendEnabledGameNumbersHandler(byte address, EventArgs e);
        // Virtual EGM Current Selected Game
        public event VirtualEGMSendSelectedGameHandler VirtualEGMSendSelectedGame; public delegate void VirtualEGMSendSelectedGameHandler(byte address, EventArgs e);
        // Virtual EGM Send Enabled Denominations
        public event VirtualEGMSendEnabledPlayerDenominationsHandler VirtualEGMSendEnabledPlayerDenominations; public delegate void VirtualEGMSendEnabledPlayerDenominationsHandler(byte address, EventArgs e);
        // Virtual EGM Send Token Denomination
        public event VirtualEGMSendTokenDenominationHandler VirtualEGMSendTokenDenomination; public delegate void VirtualEGMSendTokenDenominationHandler(byte address, EventArgs e);
        // Virtual EGM Send Cumulative Meters
        public event VirtualEGMSendCumulativeMetersHandler VirtualEGMSendCumulativeMeters; public delegate void VirtualEGMSendCumulativeMetersHandler(byte address, EventArgs e);
        // Virtual EGM Long poll 53
        public event VirtualEGMSendGameNConfigurationHandler VirtualEGMSendGameNConfiguration; public delegate void VirtualEGMSendGameNConfigurationHandler(byte address, byte[] gameNumber,
                                                                                                                                                            EventArgs e);
        // Virtual EGM Enqueue Exception
        public event GeneralLongPollHandler GeneralLongPoll; public delegate void GeneralLongPollHandler(EventArgs e);

        // Virtual EGM Apply Real Time
        public event ApplyRealTimeHandler ApplyRealTime; public delegate void ApplyRealTimeHandler(byte[] buffer, EventArgs e);

        // Virtual EGM Long poll 80
        public event VirtualEGMLP80Handler VirtualEGMLP80; public delegate void VirtualEGMLP80Handler(bool broadcast,
                                                                                                      byte group,
                                                                                                      byte level,
                                                                                                      byte[] amount,
                                                                                                      EventArgs e);
        // Virtual EGM Long poll 84
        public event VirtualEGMLP84Handler VirtualEGMLP84; public delegate void VirtualEGMLP84Handler(byte address, EventArgs e);

        // Virtual EGM Long poll 85
        public event VirtualEGMLP85Handler VirtualEGMLP85; public delegate void VirtualEGMLP85Handler(byte address, EventArgs e);
        // Virtual EGM Long poll 86
        public event VirtualEGMLP86Handler VirtualEGMLP86; public delegate void VirtualEGMLP86Handler(byte address, bool broadcast,
                                                                                                      byte group,
                                                                                                      List<Tuple<byte, byte[]>> amountsByLevel,
                                                                                                      EventArgs e);
        // Virtual EGM Long poll 87
        public event VirtualEGMLP87Handler VirtualEGMLP87; public delegate void VirtualEGMLP87Handler(byte address, EventArgs e);

        // Virtual EGM long poll 8A
        public event VirtualEGMLP8AHandler VirtualEGMLP8A; public delegate void VirtualEGMLP8AHandler(byte address,
                                                                                                      byte[] bonusAmount,
                                                                                                      byte taxStatus,
                                                                                                      EventArgs e);

        // Virtual EGM Long poll 83
        public event VirtualEGMLP83Handler VirtualEGMLP83; public delegate void VirtualEGMLP83Handler(byte address, byte[] gameNumber,
                                                                                                      EventArgs e);
        // Virtual EGM Long poll 2E
        public event VirtualEGMLP2EHandler VirtualEGMLP2E; public delegate void VirtualEGMLP2EHandler(byte address, byte[] bufferAmount,
                                                                                                      EventArgs e);
        // Virtual EGM Long poll 9A
        public event VirtualEGMLP9AHandler VirtualEGMLP9A; public delegate void VirtualEGMLP9AHandler(byte address, byte[] gameNumber,
                                                                                                      EventArgs e);
        // Virtual EGM Long poll 72 (Transfers)                                                                                            
        public event VirtualEGMLP72Handler VirtualEGMLP72; public delegate void VirtualEGMLP72Handler(byte address, byte transferCode, 
                                                                                                                    byte transactionIndex, 
                                                                                                                    byte transferType,
                                                                                                                    byte[] cashableAmount, 
                                                                                                                    byte[] restrictedAmount, 
                                                                                                                    byte[] nonRestrictedAmount, 
                                                                                                                    byte tranferFlags,
                                                                                                                    byte[] assetNumber,  
                                                                                                                    byte[] registrationKey,  
                                                                                                                    byte[] transactionID, 
                                                                                                                    byte[] expiration, 
                                                                                                                    byte[] poolID, 
                                                                                                                    byte[] receiptData,
                                                                                                                    byte[] lockTimeout, EventArgs ee); 
         // Virtual EGM Long poll 72 (Interrogate)                                                                                            
        public event VirtualEGMLP72IntHandler VirtualEGMLP72Int; public delegate void VirtualEGMLP72IntHandler(byte address, byte transferCode, 
                                                                                                                             byte transactionIndex, EventArgs ee); 
        // Event throwed when a long poll response is sent
        public event CommandSentHandler CommandSent; public delegate void CommandSentHandler(string cmd, bool crc, bool retry, EventArgs e);
        // Event throwed when a long poll is received
        public event CommandReceivedHandler CommandReceived; public delegate void CommandReceivedHandler(string cmd, bool crc, EventArgs e);
        // Event when smib link is down, when there is not a conection with SMIB
        public event SmibLinkDownHandler SmibLinkDown; public delegate void SmibLinkDownHandler(bool truth, EventArgs e);
        #endregion


        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /***********************************************     PRIVATE METHODS    ******************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region PRIVATE METHODS

        /// <summary>
        /// Returns in miliseconds the difference between the current timestamp and last sent or received message timestamp 
        /// </summary>
        /// <returns></returns>
        int GetDifferenceFromLastTimeOfMSG()
        {
            if (LastMSGRecvdOrSend == null)
            {
                LastMSGRecvdOrSend = DateTime.Now;
                return 0;
            }
            DateTime dt1 = DateTime.Now;
            DateTime dt2 = LastMSGRecvdOrSend;
            TimeSpan span = dt1 - dt2;
            LastMSGRecvdOrSend = dt1;
            int ms = (int)span.TotalMilliseconds;
            return ms;
        }


        /// <summary>
        /// Returns the VirtualEGM address. The address in this case serves as ACK
        /// </summary>
        /// <returns></returns>
        private byte GetACKByte()
        {
            try
            {
                return ACKByte.Value;
            }
            catch
            {
                if (ACKByte == null)
                    ACKByte = GetAddress();
                return ACKByte.Value;
            }
        }


        /// <summary>
        /// Gives the result of 0x80 | (or) the address of VirtualEGM. This serves as NACK
        /// </summary>
        /// <returns></returns>
        private byte GetNACKByte()
        {
            try
            {
                return NACKByte.Value;
            }
            catch
            {
                if (NACKByte == null)
                    NACKByte = (byte)(GetAddress() | 0x80);
                return NACKByte.Value;
            }

        }

        /// <summary>
        /// Function returning the general poll byte, that is equals to byte of NACK
        /// </summary>
        /// <returns></returns>
        private byte GetGeneralPollByte()
        {
            try
            {
                return GeneralPollByte.Value;
            }
            catch
            {
                if (GeneralPollByte == null)
                    GeneralPollByte = GetNACKByte();
                return GeneralPollByte.Value;
            }

        }

        /// <summary>
        /// Function returning the address
        /// </summary>
        /// <returns></returns>
        private byte GetAddress()
        {
            try
            {
                return AddressByte.Value;
            }
            catch
            {
                if (AddressByte == null)
                    AddressByte = VirtualEGMGetAddress(ee);
                return AddressByte.Value;
            }

        }

        /// <summary>
        /// Función que retorna true si está activado el Real Time, o false en caso contrario
        /// Function that returns true if the real time is activated, false otherse
        /// </summary>
        /// <returns></returns>
        private bool GetEnabledRealTime()
        {
            try
            {
                return RealTimeBool.Value;
            }
            catch
            {
                if (RealTimeBool == null)
                    RealTimeBool = VirtualEGMGetEnabledRealTime(ee);
                return RealTimeBool.Value;
            }

        }

        /// <summary>
        ///  Function that captures the indexes out of array length. If the index is greather than the buffer size, returns null, otherwise returns the element of the buffer index
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private byte? GetByteFromArrayIndex(byte[] buf, int index)
        {
            try
            {
                return buf[index];
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Start to chirp, once that the chirping interval is elapsed, this functions starts to running
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void StartChirp(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Habilita el timer de chirpeo
            // Enable the chirping timer
            if (!stopped)
            {
                if (chirpStatus != ChirpingStatus.Chirping)
                {
                    chirpStatus = ChirpingStatus.Chirping;
                }
                TimerChirping.Start();
            }
            // If it is the first time of the chirp, disables this timer
            TimerMaxForStartChirp.Stop();
        }

        /// <summary>
        /// Event that is launched if the link with SMIB is down
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void LinkDown(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (communication != false)
            {
                communication = false;
                Console.WriteLine($"[{DateTime.Now}] {smib_communication_link_down}");
            }
            try { SmibLinkDown(true, e); } catch { }
            TimerMaxForData.Stop();

        }

        /// <summary>
        /// Function that compares two long polls, it they are equals or not
        /// </summary>
        /// <param name="buf1"></param>
        /// <param name="buf2"></param>
        /// <returns></returns>
        private bool LPEquals(byte[] buf1, byte[] buf2)
        {
            if ((buf1 != null && buf2 == null)
             || (buf1 == null && buf2 != null))
                return false;
            if (buf1 == null && buf2 == null)
                return true;
            if (buf1 != null && buf2 != null)
                return buf1.SequenceEqual(buf2);
            return false;
        }

        /// <summary>
        /// Starts to listen on serial port
        /// </summary>
        private void Listen()
        {
            while (!stopped)
            {
                if (!writing)
                    ReadMessage();
            }
        }

        #endregion


        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /***********************************************     CLIENT CONTROL    ******************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region CLIENT CONTROL
        /// <summary>
        /// Chequeo de CRC, toma los dos últimos bytes y lo compara con el crc del resto del array de bytes
        /// CRC Checker. Takes the last two bytes and compares them with the crc of the rest of byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool CheckCRC(byte[] bytes)
        {
            byte[] b = bytes;
            if (b.Length > 2)
            {
                b = b.Take(b.Count() - 2).ToArray();
                CyclicalRedundancyCheck crc = new CyclicalRedundancyCheck();
                byte[] CRCResult = crc.GetCRCBytes(b);
                return (bytes[bytes.Count() - 2] == CRCResult[0] &&
                    bytes[bytes.Count() - 1] == CRCResult[1]);
            }
            else if (b.Length == 1)
            {
                if (b[0] < 0x80)
                {
                    return false;
                }
            }
            return true;

        }
        #endregion


        /********************************************/
        /************** WRITING *********************/
        /********************************************/

        #region WRITING

        /// <summary>
        /// Serial port writing. Sends to the SMIB the content of buffer and prints a log depending of print1 value, if it is activated or not
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="print1"></param>
        public void writePort(byte[] buffer, bool print1)
        {
            if (!writing)
            {
                writing = true; 
                // Computes ms as the response processing time, in miliseconds
                if (print) ms = GetDifferenceFromLastTimeOfMSG();
                int ms1 = ms;
                try { CommandSent(BitConverter.ToString(buffer), CheckCRC(buffer), false, ee); } catch { }
                // Writes on serial port
                writeBytes(buffer, buffer.Length, SerialPortFileDescriptor);
                // Computes ms as the long poll sending time, in milseconds
                if (print) ms = GetDifferenceFromLastTimeOfMSG();
                int ms2 = ms;
                last_byteArr_sent = buffer;
                if (print1) Console.WriteLine($"[{ms1}ms] RESPONSE: {buffer.Count()} -->{BitConverter.ToString(buffer)} [{ms2}ms]");
                // Thread.Sleep(17);
                // flushPort(SerialPortFileDescriptor);
                writing = false;
            }

        }
        #endregion



        /********************************************/
        /**************  CHIRP ********************/
        /********************************************/

        #region CHIRP
        /// <summary>
        /// Event, Chirp, changes the parity to mark, sends the addess and changes the parity to space
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void Chirp(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Sets the parity bit to 1
            set_wakeup(SerialPortFileDescriptor);
            Thread.Sleep(1);
            // Sends its direction
            byte[] singleton = { GetAddress() };
            writePort(singleton, print);
            Thread.Sleep(1);
            // Sets the space bit to 1
            set_space(SerialPortFileDescriptor);
        }

        // Chirp
        /// <summary>
        /// Method, Chirp, changes the parity to mark, sends the addess and changes the parity to space
        /// </summary>
        public void Chirp()
        {
            // Sets the parity bit to 1
            set_wakeup(SerialPortFileDescriptor);
            Thread.Sleep(1);
            // Sends its direction
            byte[] singleton = { GetAddress() };
            writePort(singleton, print);
            Thread.Sleep(1);
            // Sets the space bit to 1
            set_space(SerialPortFileDescriptor);
        }
        #endregion


        /********************************************/
        /**************  READING ********************/
        /********************************************/


        #region READING

        /// <summary>
        /// Reads a message and once the long poll is arrived, the method parses it 
        /// </summary>
        private void ReadMessage()
        {

            if (print) ms = GetDifferenceFromLastTimeOfMSG();
            int ms1 = ms;
            // Read the serial port and save the value into buffer
            byte[] buffer = readPort();
            int howmuch = buffer.Length;
            if (print) ms = GetDifferenceFromLastTimeOfMSG();
            int ms2 = ms;
            // It converts the buffer in a hexa array, taking the count of read bytes
            buffer = buffer.ToList().Take(howmuch).ToArray();
            if (howmuch > 0)
            {
                string bufferStr = BitConverter.ToString(buffer);
                bool crcValid = CheckCRC(buffer);
                if (chirpStatus == ChirpingStatus.Chirping)
                {
                    chirpStatus = ChirpingStatus.PollingNoSync;

                }
                TimerChirping.Stop();
                if (TimerMaxForData.Enabled == true)
                {
                    TimerMaxForData.Stop();
                    if (communication != true)
                    {
                        communication = true;
                        Console.WriteLine($"[{DateTime.Now}] {smib_communication_enabled_msg}");
                    }
                    try { SmibLinkDown(false, null); } catch { }

                }
                TimerMaxForStartChirp.Stop();
                try { CommandReceived(bufferStr, crcValid, ee); } catch (Exception e) {
                    // if (print)
                    Console.WriteLine($"error");
                    Console.WriteLine($"{e}");
                }
                // Check the CRC error
                if (crcValid)
                {
                    if (print) Console.WriteLine($"[{ms1}ms] {howmuch} -->{bufferStr} [{ms2}ms] ");
                    // If it is in the PollingNoSync state, then waits a long poll 80 or a long poll with different address
                    if (chirpStatus == ChirpingStatus.PollingNoSync)
                    {
                        byte? FirstByte = GetByteFromArrayIndex(buffer, 0);
                        if ((FirstByte != GetGeneralPollByte())
                         && (FirstByte == 0x80 // ReSYNC
                          || FirstByte != GetAddress())) // Not an exception, it is a long poll. Check that the direction is not equals to address. 
                        {
                            chirpStatus = ChirpingStatus.PollingSync;
                        }
                    }
                    if (chirpStatus == ChirpingStatus.PollingSync)
                    {
                        byte? FirstByte = GetByteFromArrayIndex(buffer, 0); 
                        // If the message recently received (buffer) IS NOT equals to byte array received before
                        if (!LPEquals(buffer, last_byteArr_received))
                        {
                            // Updates the byte array received with buffer
                            last_byteArr_received = buffer;
                            // Reset the last sent byte array
                            last_byteArr_sent = null;
                            if (!disabled)
                            {
                                // We take the last byte array as an ACK 
                                try { LPDistinct(null); } catch { }
                                // And the buffer gets processed
                                // Response by cases
                                // If it is in the PollingSync state, waits an 81 or longpoll with same address
                                // If a long poll with same adress arrives, and client is on realtime, apply realtime in VirtualEGM
                                if ((FirstByte == GetAddress()
                                || FirstByte == 0x00) && RealTimeBool == true)
                                {
                                    ApplyRealTime(buffer, ee);
                                }
                                else
                                {
                                    if (FirstByte == GetGeneralPollByte())
                                    {
                                        // 81 received
                                        // Pop from stack the above element
                                        GeneralLongPoll(ee);

                                    }
                                    else
                                    {
                                        //Not an exception, it is a longpoll. Check if direction is the same
                                        if (FirstByte == GetAddress()
                                        || FirstByte == 0x00)
                                        {
                                            AnalyzeLongPoll(buffer);
                                        }
                                    }
                                }
                            }
                        }
                        // Si el mensaje recién recibido (buffer) ES igual al byte array recibido anteriormente, 
                        // If the message is recently received is equals to byte array received before
                        else if (FirstByte != 0x80)
                        {
                            if (!disabled)
                            {
                                // se toma como un NACK al byte array anterior, 
                                // take as a nack the last byte array
                                try
                                { // el último byte array enviado no es vacío
                                 // the last byte array sent is not empty
                                    if (last_byteArr_sent != null)
                                    {
                                        int last_byteArr_sentCount = last_byteArr_sent.Count();
                                        byte? LastArrFirstByte = GetByteFromArrayIndex(last_byteArr_sent, 0);
                                        // el último byte array enviado tiene más de un byte, y el primer byte es la dirección o es un 0x00
                                        // the last sent byte array has more than one byte, and the first byte is the  direction or is a 0x00 
                                        if ((last_byteArr_sentCount > 1 && (LastArrFirstByte == GetAddress() || LastArrFirstByte == 0x00))
                                        // el último byte array tiene un sólo elemento, y el primer byte es la dirección
                                        // the last byte array has one element, and the first byte is the direction
                                        || (last_byteArr_sentCount == 1 && LastArrFirstByte != GetAddress()))
                                            writePort(last_byteArr_sent, print);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                else
                {
                    if (print) Console.WriteLine($"{howmuch} -- {bufferStr} -- CRCError! [{ms}ms]");
                }
                if (!stopped)
                {
                    TimerMaxForStartChirp.Start();
                    TimerMaxForData.Start();
                }
            }
        }
        #endregion


        /********************************************/
        /**************  RUNNING ********************/
        /********************************************/

        #region RUNNING

        /// <summary>
        /// Inicialización del client, con los diferentes timers
        /// Initialization of SAS Client, and the different timers
        /// </summary>
        /// <param name="print_"></param>
        public SASClient(bool print_)
        {

            print = print_;
            // TimerMaxForStartChirp: Tiempo máximo para empezar a chirpear
            //Max time  to start to chirp
            TimerMaxForStartChirp = new System.Timers.Timer(5000);
            // TimerMaxForStartChirp: Tiempo máximo para esperar data
            // TimerMaxForStartChipr: Max time to wait data
            TimerMaxForData = new System.Timers.Timer(1500);
            // TimerChirping: Intervalo de chirpeo
            // Chirping interval
            TimerChirping = new System.Timers.Timer(200);
            TimerMaxForStartChirp.Elapsed += StartChirp;
            TimerMaxForData.Elapsed += LinkDown;
            TimerChirping.Elapsed += Chirp;
        }

        /// <summary>
        /// Comienzo de ejecución del client. Abre el puerto en el que se pasa como parámetro y comienza a escuchar
        /// Client execution starting. Opens the serial port passed as argument and starts to listening it
        /// </summary>
        /// <param name="port"></param>
        public void Start(string port)
        {
            stopped = false;
            disabled = true;
            TimerMaxForStartChirp.Start();
            chirpStatus = ChirpingStatus.Chirping;
            TimerChirping.Start();
            SerialPortFileDescriptor = OpenPort(port);
            Task.Run(() => Listen());
        }

        /// <summary>
        /// Detiene la ejecución del ciclo
        /// Stop the loop execution
        /// </summary>
        public void Stop()
        {
            stopped = true;
            TimerMaxForStartChirp.Stop();
            TimerMaxForData.Stop();
            chirpStatus = ChirpingStatus.Stopped;
            TimerChirping.Stop();
            ClosePort(SerialPortFileDescriptor);
        }

        /// <summary>
        /// Deshabilita el procesamiento de long polls
        /// Disables the long poll processing
        /// </summary>
        public void DisableClient()
        {
            disabled = true;
        }

        /// <summary>
        /// Habilita el procesamiento de long polls
        /// Enables the long poll processing
        /// </summary>
        public void EnableClient()
        {
            disabled = false;
        }

        #endregion


        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************     ANALYZE LONGPOLLS  (ORDERED BY LONGPOLL)    *********************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        // This method takes a single byte 'h' and converts it to a string representation using the BitConverter class.
        public string printField(byte h)
        {
            // BitConverter.ToString is used to convert the byte to a string.
            return BitConverter.ToString(new byte[] { h });
        }

        // This method takes an array of bytes 'h' and converts it to a string representation using the BitConverter class.
        public string printField(byte[] h)
        {
            // BitConverter.ToString is used to convert the byte array to a string.
            return BitConverter.ToString(h);
        }

        // Returning the result of the method call.
        // Note: The commented text inside the code is for explanatory purposes and does not affect the execution of the code.


        #region "Analyze Longpolls"

        /// <summary>
        /// PARA EL 6F: Dada una lista de meter codes, un buffer de entrada (la petición del host), un número start y un índice l 
        /// For 6F: Given a meter code list, a buffer , a start number and an index
        /// </summary>
        /// <param name="meterCodes"></param>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        private bool InsertAdditionalMeterCodes(ref List<byte[]> meterCodes, byte[] buffer, byte start, byte counter)
        {
            // Si el contador es negativo o 0, devuelve true
            // If counter is less or equals to 0, returns true
            if (counter <= 0)
                return true;
            // Si el índice es un número par
            // If the index is pair
            if (counter % 2 == 0)
            {
                bool check = true;
                byte[] requestedMeterCode = new byte[2];
                // De la posición start a la posición start+1, guarda el valor de cada byte en requestedMeterCode
                // From position start to position start+1, saves the value from each byte in requested meter code
                for (int i = start; i < start + 2; i++)
                {
                    byte? val = GetByteFromArrayIndex(buffer, i);
                    if (val == null) check = false;
                    else requestedMeterCode[i - start] = val.Value;
                }
                meterCodes.Add(requestedMeterCode);
                // Si pasó la validación, y se insertó satisfactoriamente, se llama recursivamente incrementando en 2 el inicio y restando el 2 el contador
                // If validation is passed and is inserted succesfully, calls recursively incrementing in 2 the init and substracting 2 the counter
                if (check)
                    return InsertAdditionalMeterCodes(ref meterCodes, buffer, (byte)((int)(start) + 2), (byte)((int)(counter) - 0x02));
                else
                    return check;

            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// PARA EL 7C: Dada una lista de datos de ticket, un buffer de entrada (la petición del host), un número start y un índice l 
        /// For 7C: Given a ticket data list, a buffer, a start number and an index
        /// </summary>
        /// <param name="data_elements"></param>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        private bool InsertExtendedTicketData(ref List<Tuple<byte, string>> data_elements, byte[] buffer, byte start, byte counter)
        {
            if (counter > 0)
            {
                byte? data_code = GetByteFromArrayIndex(buffer, start);
                counter--;
                start++;
                byte? data_length = GetByteFromArrayIndex(buffer, start);
                counter--;
                start++;
                if (data_code != null)
                {
                    if (data_length > 0)
                    {
                        bool check = true;
                        byte[] data = new byte[(int)data_length.Value];
                        byte start_1 = start;
                        for (int i = start_1; i < (int)start_1 + (int)data_length; i++)
                        {
                            counter--;
                            start++;
                            byte? val = GetByteFromArrayIndex(buffer, i);
                            if (val == null) check = false;
                            else data[i - start_1] = val.Value;
                        }
                        if (check)
                        {
                            Encoding encoding = Encoding.Default;
                            string dataStr = encoding.GetString(data);
                            data_elements.Add(new Tuple<byte, string>(data_code.Value, dataStr));
                            return InsertExtendedTicketData(ref data_elements, buffer, start, counter);
                        }
                        else return check;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (counter == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Intérprete de long polls
        /// Long poll interpreter
        /// </summary>
        /// <param name="buffer"></param>
        public void AnalyzeLongPoll(byte[] buffer)
        {
            //Me fijo el segundo byte.
            // Check the second byte
            switch (GetByteFromArrayIndex(buffer, 1))
            {
                case 0x01:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM01(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x02:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM02(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x03:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM03(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x04:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM04(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x06:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM06(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x07:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM07(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x08:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] billDenominations = new byte[] { buffer[2], buffer[3], buffer[4], buffer[5] };
                            byte billAcceptorFlag = buffer[6];
                            VirtualEGMConfigureBillDenomination(address, billDenominations, billAcceptorFlag, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x0E:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte Enable_Disable = buffer[2];
                            VirtualEGM0E(address, Enable_Disable, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x0F:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGM0F(address, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x10:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTotalCancelledCredits(address, ee); } catch { }
                        break;
                    }
                case 0x11:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTotalCoinInMeter(address, ee); } catch { }
                        break;

                    }
                case 0x12:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTotalCoinOutMeter(address, ee); } catch { }
                        break;
                    }
                case 0x13:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTotalDropMeter(address, ee); } catch { }
                        break;
                    }
                case 0x14:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTotalJackpotMeter(address, ee); } catch { }
                        break;
                    }
                case 0x15:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendGamesPlayedMeter(address, ee); } catch { }
                        break;

                    }
                case 0x16:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendGamesWonMeter(address, ee); } catch { }
                        break;

                    }
                case 0x17:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendGamesLostMeter(address, ee); } catch { }

                        break;

                    }
                case 0x18:
                    {
                        try { byte address = buffer[0]; VirtualEGMLP18(address, ee); } catch { }

                        break;
                    }
                case 0x19:
                    {
                        try { byte address = buffer[0]; VirtualEGMLP19(address, ee); } catch { }
                        break;
                    }
                case 0x1A:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte single_meter_accounting_long_poll = buffer[1];
                            VirtualEGMHostSingleMeterAccounting(address, single_meter_accounting_long_poll, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x1B:
                    {
                        try { byte address = buffer[0]; VirtualEGM1B(address, ee); } catch { }
                        break;
                    }
                case 0x1C: // Recibe un long poll  1C
                    {
                        try { byte address = buffer[0]; VirtualEGMSendMeters(address, ee); } catch { }
                        break;
                    }
                case 0x1D:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendCumulativeMeters(address, ee); } catch { }
                        break;
                    }
                case 0x1F:
                    {
                        try { byte address = buffer[0]; VirtualEGM1F(address, ee); } catch { }
                        break;
                    }
                case 0x1E:
                    {
                        try { byte address = buffer[0]; VirtualEGM1E(address, ee); } catch { }
                        break;
                    }
                case 0x20:
                    {
                        try { byte address = buffer[0]; VirtualEGMLP20(address, ee); } catch { }
                        break;
                    }
                case 0x21:
                    {
                        try
                        {
                            Console.WriteLine($"LP21 received 0x21 request in SASClient");
                            byte address = buffer[0];
                            byte[] seedValue = new byte[] { buffer[2], buffer[3] };
                            VirtualEGM21(address, seedValue, ee);
                        }
                        catch {}
                        break;
                    }
                case 0x27:
                    {
                        try { byte address = buffer[0]; VirtualEGMLP27(address, ee); } catch { }
                        break;
                    }
                case 0x28:
                    {
                        try { byte address = buffer[0]; VirtualEGMLP28(address, ee); } catch {}
                        break;
                    }
                case 0x2A:
                    {
                        try { byte address = buffer[0]; VirtualEGM2A(address, ee); } catch { }
                        break;
                    }
                case 0x2B:
                    {
                        try { byte address = buffer[0]; VirtualEGM2B(address, ee); } catch { }
                        break;
                    }
                case 0x2D:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMSendHandPaidCancelledCredits(address, gameNumber, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x2E:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] bufferAmount = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMLP2E(address, bufferAmount, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x2F:
                    {
                        try
                        {
                            byte address = buffer[0];
                            int length = buffer[2];
                            byte[] gameNumber = new byte[] { buffer[3], buffer[4] };
                            List<byte> metersList = new List<byte>();
                            for (int i = 0; i < length - 2; i++)
                            {
                                metersList.Add(buffer[i + 5]);
                            }
                            byte[] meters = metersList.ToArray();
                            VirtualEGMSendSelectedMeters(address, gameNumber,
                                                         meters,
                                                         ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x31:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 1, ee); } catch { }
                        break;
                    }
                case 0x32:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 2, ee); } catch { }
                        break;
                    }
                case 0x33:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 5, ee); } catch { }
                        break;
                    }
                case 0x34:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 10, ee); } catch { }
                        break;
                    }
                case 0x35:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 20, ee); } catch { }
                        break;
                    }
                case 0x36:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 50, ee); } catch { }
                        break;
                    }
                case 0x37:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 100, ee); } catch { }
                        break;
                    }
                case 0x38:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 500, ee); } catch { }
                        break;
                    }
                case 0x39:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 1000, ee); } catch { }
                        break;
                    }
                case 0x3A:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 200, ee); } catch { }
                        break;
                    }
                case 0x3B:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 25, ee); } catch { }
                        break;
                    }
                case 0x3C:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 2000, ee); } catch { }
                        break;
                    }
                case 0x3E:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 2500, ee); } catch { }
                        break;
                    }
                case 0x3F:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 5000, ee); } catch { }
                        break;
                    }
                case 0x40:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 10000, ee); } catch { }
                        break;
                    }
                case 0x41:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 20000, ee); } catch { }
                        break;
                    }
                case 0x42:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 25000, ee); } catch { }
                        break;
                    }
                case 0x43:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 50000, ee); } catch { }
                        break;
                    }
                case 0x44:
                    {
                        try { byte address = buffer[0]; BillMeterRequested(address, 100000, ee); } catch { }
                        break;
                    }
                case 0x48:
                    {
                        try { byte address = buffer[0]; VirtualEGMLP48(address, ee); } catch { }
                        break;
                    }
                case 0x4C:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] machineID = new byte[] { buffer[2], buffer[3], buffer[4] };
                            byte[] sequenceNumber = new byte[] { buffer[5], buffer[6], buffer[7] };
                            VirtualEGMSetSecureEnhancedValidationID(address, machineID, sequenceNumber, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x4D:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte functionCode = GetByteFromArrayIndex(buffer, 2).Value;
                            VirtualEGM4D(address, functionCode, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x50:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte validationType = buffer[2];
                            VirtualEGM50(address, validationType, ee);
                        }
                        catch { }
                        break;
                    }
                case 0x51:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendNumberOfGamesImplemented(address, ee); } catch { }
                        break;
                    }
                case 0x53:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMSendGameNConfiguration(address, gameNumber, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x54:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendSASVersionIDAndGameSerialNumber(address, ee); } catch { }
                        break;
                    }
                case 0x55:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendSelectedGame(address, ee); } catch { }
                        break;
                    }
                case 0x56:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendEnabledGameNumbers(address, ee); } catch { }
                        break;
                    }
                case 0x57: // Recibe un long poll  57 -- Received a long poll 57
                    {
                        try { byte address = buffer[0]; VirtualEGMResponsePendingCashoutInformation(address, ee); } catch { }
                        break;
                    }
                case 0x58: // Recibe un long poll  58 -- Received a long poll 58
                    {
                        byte address = buffer[0];
                        bool check = true;
                        byte? validationSystemIdCode = null;
                        byte[] validationNumber = new byte[8];

                        validationSystemIdCode = GetByteFromArrayIndex(buffer, 2);
                        if (validationSystemIdCode == null) check = false;
                        /* Si en la lectura, algùn byte es nulo o no existe, interrumpe la operación */
                        /* If the reading, there is a null or non existing byte, interrupts the operation */
                        // De la posición 3 a la posición 7, guarda el valor de cada byte en validationNumber
                        // From third position to seventh position, saves the value of each byte in validationNumber
                        for (int i = 3; i < 11; i++)
                        {
                            byte? val = GetByteFromArrayIndex(buffer, i);
                            if (val == null) check = false;
                            else validationNumber[i - 3] = val.Value;
                        }

                        // Si pasó todo,
                        // all points are validated and passed
                        if (check) VirtualEGMReceiveValidationNumber(address, validationSystemIdCode.Value,
                                                    validationNumber, ee);
                        break;
                    }
                case 0x6F: // Recibe un long poll 6F -- Received a long poll 6F
                    {
                        byte address = buffer[0];
                        byte? length = GetByteFromArrayIndex(buffer, 2);
                        if (length >= 0x04 && length <= 0x1A)
                        {
                            bool check = true;
                            byte[] gameNumber = new byte[2];
                            byte[] requestedMeterCode = new byte[2];
                            /* Si en la lectura, algùn byte es nulo o no existe, interrumpe la operación */
                            /* If in the reading, there is a null or non existing byte, interrupts the operation */
                            // De la posición 3 a la posición 4, guarda el valor de cada byte en gameNumber
                            // From position 3 to position 4, saves the value of each byte in ganeNumber
                            for (int i = 3; i < 5; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else gameNumber[i - 3] = val.Value;
                            }
                            // De la posición 5 a la posición 6, guarda el valor de cada byte en requestedMeterCode
                            // From position 5 to position 6, saves the value of each byte in requestedMeterCode
                            for (int i = 5; i < 7; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else requestedMeterCode[i - 5] = val.Value;
                            }
                            List<byte[]> additionalMeterCodes = new List<byte[]>();
                            // Si el tamaño es mayor estricto que 4, entonces hay meters adicionales
                            // If the length is greater than 4, then there is additional meters
                            if (length > 4)
                            {
                                byte remain_length = (byte)((int)length - 4);
                                check = InsertAdditionalMeterCodes(ref additionalMeterCodes, buffer, 7, remain_length);

                            }

                            if (check) VirtualEGMSendExtendedMeters(address, 0x6F, gameNumber, requestedMeterCode, additionalMeterCodes, ee);

                        }
                        break;
                    }
                case 0x70:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTicketValidationData(address, ee); } catch { }
                        break;
                    }
                case 0x71:
                    {
                        byte address = buffer[0];
                        byte? length = GetByteFromArrayIndex(buffer, 2);
                        byte? transferCode = GetByteFromArrayIndex(buffer, 3);
                        try
                        {

                            byte[] transferAmount = new byte[] { buffer[4], buffer[5], buffer[6], buffer[7], buffer[8] };
                            byte? parsingCode = GetByteFromArrayIndex(buffer, 9);
                            List<byte> validationDataList = new List<byte>();
                            for (int i = 0; i < length - 7; i++)
                            {
                                validationDataList.Add(buffer[10 + i]);
                            }
                            byte[] validationData = validationDataList.ToArray();
                            VirtualEGMRedeemTicket4(address, transferCode.Value,
                                                    transferAmount,
                                                    parsingCode.Value,
                                                    validationData, ee);
                        }
                        catch
                        {
                            VirtualEGMRedeemTicket1(address, transferCode.Value, ee);
                        }
                        break;
                    }
                case 0x72:
                    {
                        byte address = buffer[0];
                        byte? length = GetByteFromArrayIndex(buffer, 2);
                        byte? transferCode = GetByteFromArrayIndex(buffer, 3);
                        if (transferCode == 0x00 || transferCode == 0x01 || transferCode == 0x80)
                        {      
                            byte? transactionIndex = GetByteFromArrayIndex(buffer, 4);
                            byte? transferType = GetByteFromArrayIndex(buffer, 5);
                            try
                            {

                                byte[] cashableAmount = new byte[] { buffer[6], buffer[7], buffer[8], buffer[9], buffer[10] };
                                byte[] restrictedAmount = new byte[] {buffer[11], buffer[12], buffer[13], buffer[14], buffer[15]};
                                byte[] nonRestrictedAmount = new byte[] {buffer[16], buffer[17], buffer[18], buffer[19], buffer[20]};
                                byte tranferFlags = buffer[21];
                                byte[] assetNumber = new byte[] {buffer[22], buffer[23], buffer[24], buffer[25] };
                                byte[] registrationKey = new byte[] { buffer[26], buffer[27], buffer[28], buffer[29], buffer[30], buffer[31], buffer[32], buffer[33], buffer[34], buffer[35], buffer[36], buffer[37], buffer[38], buffer[39], buffer[40], buffer[41], buffer[42], buffer[43], buffer[44], buffer[45]};
                                byte transactionIDLength = buffer[46];
                                int startindex=47;
                                // transactionID
                                List<byte> transactionIDList = new List<byte>();
                                for (int index = startindex; index < startindex + transactionIDLength; index++)
                                {
                                    transactionIDList.Add(buffer[index]);
                                }
                                byte[] transactionID = transactionIDList.ToArray();
                                int currentindex = startindex+transactionIDLength;
                                byte[] expiration = new byte[] {buffer[currentindex], buffer[currentindex+1], buffer[currentindex+2],buffer[currentindex+3]};
                                byte[] poolID = new byte[] {buffer[currentindex+4], buffer[currentindex+5]};
                                byte receiptDataLength = buffer[currentindex+6];
                                startindex = currentindex+7;
                                // receiptData
                                List<byte> receiptDataList = new List<byte>();
                                for (int index = startindex; index < startindex + receiptDataLength; index++)
                                {
                                    receiptDataList.Add(buffer[index]);
                                }
                                byte[] receiptData = receiptDataList.ToArray();
                                currentindex = startindex+receiptDataLength;
                                byte[] lockTimeout = new byte[] {};

                    
                                VirtualEGMLP72(address, transferCode.Value, // byte
                                                        transactionIndex.Value, // byte
                                                        transferType.Value, // byte
                                                        cashableAmount, // byte[]
                                                        restrictedAmount, // byte[]
                                                        nonRestrictedAmount, //byte[]
                                                        tranferFlags, // byte
                                                        assetNumber,  // byte[]
                                                        registrationKey,  // byte[]
                                                        transactionID, // byte[]
                                                        expiration, // byte[]
                                                        poolID, // byte[]
                                                        receiptData, // byte[]
                                                        lockTimeout, ee); // byte
                            }
                            catch (Exception ex)
                            {
                                    Console.WriteLine(ex.Message);
                            }
                        }
                        else if (transferCode == 0xFE || transferCode == 0xFF)
                        {
                            byte? transactionIndex = GetByteFromArrayIndex(buffer, 4);
                            VirtualEGMLP72Int(address, transferCode.Value, transactionIndex.Value, ee);

                        }
                        break;
                    }
                case 0x73: // Recibe un long poll  73 -- Received a long poll 73
                    {
                        byte address = buffer[0];
                        if (GetByteFromArrayIndex(buffer, 2) == 0x01) /* 1 parameter , 1 byte*/
                        {
                            bool check = true;
                            // Espera un código de registración
                            // Awaits a registration code
                            byte? regCode = GetByteFromArrayIndex(buffer, 3);
                            if (regCode == null) check = false; // Si es nulo, pasa de largo -- If it is null, skips all the code

                            // Si pasó todo, registra la máquina
                            if (check) VirtualEGMRegisterGamingMachine(address, regCode.Value, ee);
                        }
                        else if (GetByteFromArrayIndex(buffer, 2) == 0x1D) /* 4 parameters, 29 bytes*/
                        {
                            bool check = true;
                            // Espera un código de registración
                            // Awaits a registration code
                            byte? regCode = GetByteFromArrayIndex(buffer, 3);
                            if (regCode == null) check = false; // Si es nulo, pasa de largo -- If it is null, skips all the code
                            byte[] assetNumber = new byte[4];
                            byte[] regKey = new byte[20];
                            byte[] PODid = new byte[4];
                            /* Si en la lectura, algùn byte es nulo o no existe, interrumpe la operación */
                            /* If in the reading, there is a null or non existing byte, interrupts the operation */
                            // De la posición 4 a la posición 7, guarda el valor de cada byte en AssetNumber
                            // From third position to seventh position, saves the value of each byte in validationNumber                          
                            for (int i = 4; i < 8; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else assetNumber[i - 4] = val.Value;
                            }
                            // De la posición 8 a la posición 27, guarda el valor de cada byte en RegKey
                            // From position 8 to position 27, saves the value of each byte in RegKey
                            for (int i = 8; i < 28; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else regKey[i - 8] = val.Value;
                            }
                            // De la posición 28 a la posición 31, guarda el valor de cada byte en PODid
                            // From position 28 to position 31, saves the value of each byte in PODid
                            for (int i = 28; i < 32; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else PODid[i - 28] = val.Value;
                            }
                            // Si pasó todo, registra la máquina
                            // If all is checked, register the machine
                            if (check) VirtualEGMRegisterGamingMachine1(address, regCode.Value,
                                assetNumber,
                                regKey,
                                PODid, ee);

                        }
                        break;
                    }
                case 0x74:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte lockCode = buffer[2];
                            byte transferCondition = buffer[3];
                            byte[] lockTimeout = new byte[] { buffer[4], buffer[5] };
                            VirtualEGM74(address, lockCode, transferCondition, lockTimeout, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x7B:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] controlMask = new byte[] { buffer[3], buffer[4] };
                            byte[] statusBitControlStates = new byte[] { buffer[5], buffer[6] };
                            byte[] cashableTicketAndReceiptExpiration = new byte[] { buffer[7], buffer[8] };
                            byte[] restrictedTicketDefaultExpiration = new byte[] { buffer[9], buffer[10] };
                            VirtualEGMExtendedValidationStatus(address, controlMask, statusBitControlStates, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration, ee);

                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x7C: // Recibe un long poll 7C
                    {
                        try
                        {
                            byte address = buffer[0];
                            bool check = true;
                            byte? length = GetByteFromArrayIndex(buffer, 2);
                            if (length != null)
                            {
                                int l = (int)length;
                                List<Tuple<byte, string>> data_elements = new List<Tuple<byte, string>>();
                                if (InsertExtendedTicketData(ref data_elements, buffer, 3, length.Value))
                                {
                                    VirtualEGMSetExtendedTicketData(address, data_elements, ee);
                                }
                            }
                        }
                        catch { }
                        break;
                    }
                case 0x7D: // Recibe un long poll 7D
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte? length = GetByteFromArrayIndex(buffer, 2);
                            if (length > 0x00)
                            {
                                byte[] hostID = new byte[] { buffer[3], buffer[4] };
                                byte? expiration = GetByteFromArrayIndex(buffer, 5);
                                int start = 0;
                                // Location
                                byte locationLengthByte = GetByteFromArrayIndex(buffer, 6).Value;
                                int locationlength = (int)locationLengthByte;
                                start = 7;
                                List<byte> locationByteList = new List<byte>();
                                for (int index = 0; index < locationlength; index++)
                                {
                                    locationByteList.Add(buffer[start + index]);
                                }
                                byte[] location = locationByteList.ToArray();

                                // Address1
                                byte address1lengthByte = GetByteFromArrayIndex(buffer, start + locationlength).Value;
                                int address1length = (int)address1lengthByte;
                                start = start + locationlength + 1;
                                List<byte> address1ByteList = new List<byte>();
                                for (int index = 0; index < address1length; index++)
                                {
                                    address1ByteList.Add(buffer[start + index]);
                                }
                                byte[] address1 = address1ByteList.ToArray();

                                // Address2
                                byte address2lengthByte = GetByteFromArrayIndex(buffer, start + address1length).Value;
                                int address2length = (int)address2lengthByte;
                                start = start + address1length + 1;
                                List<byte> address2ByteList = new List<byte>();
                                for (int index = 0; index < address2length; index++)
                                {
                                    address2ByteList.Add(buffer[start + index]);
                                }
                                byte[] address2 = address2ByteList.ToArray();

                                VirtualEGMSetTicketData7D(address, hostID,
                                                        expiration.Value,
                                                        locationLengthByte,
                                                        location,
                                                        address1lengthByte,
                                                        address1,
                                                        address2lengthByte,
                                                        address2, ee);
                            }
                            else if (length == 0x00)
                            {
                                VirtualEGMSetTicketData7DWith00(address, ee);
                            }

                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x7E:
                    {
                        try { byte address = buffer[0]; VirtualEGM7E(address, ee); } catch { }
                        break;
                    }
                case 0x7F:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] date = new byte[] { buffer[2], buffer[3], buffer[4], buffer[5] };
                            byte[] time = new byte[] { buffer[6], buffer[7], buffer[8] };
                            VirtualEGMReceiveDateAndTime(address, date, time, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x80:
                    {
                        try
                        {
                            bool broadcast = buffer[0] == 0x00;
                            byte group = buffer[2];
                            byte level = buffer[3];
                            byte[] amount = new byte[] { buffer[4], buffer[5], buffer[6], buffer[7], buffer[8] };
                            VirtualEGMLP80(broadcast, group, level, amount, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x83:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMLP83(address, gameNumber, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x84:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGMLP84(address, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x85:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGMLP85(address, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x86:
                    {
                        try
                        {
                            byte address = buffer[0];
                            bool broadcast = buffer[0] == 0x00;
                            byte length = buffer[2];
                            byte group = buffer[3];
                            int N = (int)length;
                            List<Tuple<byte, byte[]>> amountsByLevel = new List<Tuple<byte, byte[]>>();
                            byte level = buffer[4];
                            byte[] amount = new byte[] { buffer[5], buffer[6], buffer[7], buffer[8], buffer[9] };
                            amountsByLevel.Add(new Tuple<byte, byte[]>(level, amount));
                            N = N - 7;
                            int i = 1;
                            while (N >= 6)
                            {
                                level = buffer[4 + i * 6];
                                amount = new byte[] { buffer[5 + i * 6], buffer[6 + i * 6], buffer[7 + i * 6], buffer[8 + i * 6], buffer[9 + i * 6] };
                                amountsByLevel.Add(new Tuple<byte, byte[]>(level, amount));
                                i++;
                                N = N - 6;
                            }
                            VirtualEGMLP86(address, broadcast, group, amountsByLevel, ee);

                        }
                        catch
                        {

                        }

                        break;
                    }
                case 0x87:
                    {
                        try
                        {
                            byte address = buffer[0];
                            VirtualEGMLP87(address, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x8A:
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] amount = new byte[] { buffer[2], buffer[3], buffer[4], buffer[5] };
                            byte taxStatus = buffer[6];
                            VirtualEGMLP8A(address, amount, taxStatus, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0x94:
                    {
                        try { byte address = buffer[0]; VirtualEGMResetHandpay(address, ee); } catch { }
                        break;
                    }
                case 0x9A:
                    {
                        try {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMLP9A(address, gameNumber, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0xA0:  // Recibe un long poll  A0 -- Received a long poll A0
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMA0(address, gameNumber, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0xA4: // Recibe un long poll A4 -- Received a long poll A4
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMA4(address, gameNumber, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0xA8: // Recibe un long poll A8 -- Received a long poll A8
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte resetMethod = buffer[2];
                            VirtualEGMA8(address, resetMethod, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0xAF: // Recibe un long poll AF -- Received a long poll AF
                    {
                        byte address = buffer[0];
                        byte? length = GetByteFromArrayIndex(buffer, 2);
                        if (length >= 0x04 && length <= 0x1A)
                        {
                            bool check = true;
                            byte[] gameNumber = new byte[2];
                            byte[] requestedMeterCode = new byte[2];
                            /* Si en la lectura, algùn byte es nulo o no existe, interrumpe la operación */
                            /* If in the reading, there is a null or non existing byte, interrupts the operation */
                            // De la posición 3 a la posición 4, guarda el valor de cada byte en gameNumber
                            // From position 3 to position 4, saves the value of each byte in ganeNumber
                            for (int i = 3; i < 5; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else gameNumber[i - 3] = val.Value;
                            }
                            // De la posición 5 a la posición 6, guarda el valor de cada byte en requestedMeterCode
                            // From position 5 to position 6, saves the value of each byte in requestedMeterCode
                            for (int i = 5; i < 7; i++)
                            {
                                byte? val = GetByteFromArrayIndex(buffer, i);
                                if (val == null) check = false;
                                else requestedMeterCode[i - 5] = val.Value;
                            }
                            List<byte[]> additionalMeterCodes = new List<byte[]>();
                            // Si el tamaño es mayor estricto que 4, entonces hay meters adicionales
                            // If the length is greater than 4, then there is additional meters
                            if (length > 4)
                            {
                                byte remain_length = (byte)((int)length - 4);
                                check = InsertAdditionalMeterCodes(ref additionalMeterCodes, buffer, 7, remain_length);

                            }

                            if (check) VirtualEGMSendExtendedMeters(address, 0xAF, gameNumber, requestedMeterCode, additionalMeterCodes, ee);

                        }
                        break;
                    }
                case 0xB1:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendCurrentPlayerDenomination(address, ee); } catch { }
                        break;
                    }
                case 0xB2:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendEnabledPlayerDenominations(address, ee); } catch { }
                        break;
                    }
                case 0xB3:
                    {
                        try { byte address = buffer[0]; VirtualEGMSendTokenDenomination(address, ee); } catch { }
                        break;
                    }
                 case 0xB4: // Received a long poll B4
                    {
                        try 
                        { 
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3]};
                            byte[] wagerCategory = new byte[] { buffer[4], buffer[5] };
                            VirtualEGMSendWagerCategoryInformation(address, gameNumber, wagerCategory, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case 0xB5: // Recibe un long poll B5 -- Received a long poll B5
                    {
                        try
                        {
                            byte address = buffer[0];
                            byte[] gameNumber = new byte[] { buffer[2], buffer[3] };
                            VirtualEGMSendExtendedGameInformation(address, gameNumber, ee);
                        }
                        catch
                        {

                        }
                        break;
                    }
                default:
                    break;
            }
        }


        #endregion


        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************     RESPONSES    (ORDER BY LONGPOLL)            *******************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region "Responses"
        // Response 0F
        public void SendMultipleMeters0FResponse(byte address,
                                                 byte[] total_cancelled_credits,
                                                 byte[] total_coin_in,
                                                 byte[] total_coin_out,
                                                 byte[] total_drop,
                                                 byte[] total_jackpot,
                                                 byte[] games_played)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP0F(address,
                                                                           total_cancelled_credits,
                                                                           total_coin_in,
                                                                           total_coin_out,
                                                                           total_drop,
                                                                           total_jackpot,
                                                                           games_played);
            writePort(response, print);
        }

        // Response 18
        public void SendGamesPlayedSinceLast_18(byte address,
                                                byte[] last_power_up,
                                                byte[] last_slot_door_closure)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP18(address,
                                                                           last_power_up,
                                                                           last_slot_door_closure);
            writePort(response, print);
        }

        // Response 19
        public void SendMultipleMeters19Response(byte address,
                                                 byte[] total_coin_in,
                                                 byte[] total_coin_out,
                                                 byte[] total_drop,
                                                 byte[] total_jackpot,
                                                 byte[] games_played)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP19(address,
                                                                           total_coin_in,
                                                                           total_coin_out,
                                                                           total_drop,
                                                                           total_jackpot,
                                                                           games_played);
            writePort(response, print);
        }

        // Response 1B
        public void SendHandpayInformation(byte address,
                                           byte progressiveGroup,
                                           byte level,
                                           byte[] amount,
                                           byte[] partialPay,
                                           byte resetID)
        {
            byte[] response = LongPollFactory.Singleton.GetResponse1B(address,
                                                                      progressiveGroup,
                                                                      level,
                                                                      amount,
                                                                      partialPay,
                                                                      resetID);
            writePort(response, print);

        }

        // Response 1C
        public void Send1CMeters(byte address,
            byte[] total_coin_in,
            byte[] total_coin_out,
            byte[] total_drop,
            byte[] total_jackpot,
            byte[] games_played,
            byte[] games_won,
            byte[] slot_door_opened,
            byte[] power_reset)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseMeters(address,
                total_coin_in,
                total_coin_out,
                total_drop,
                total_jackpot,
                games_played,
                games_won,
                slot_door_opened,
                power_reset);
            writePort(response, print);
        }

        // Response 1D
        public void SendCumulativeMeters(byte address,
                                         byte[] CumulativePromoCreditsToGM,
                                         byte[] CumulativeNonCashableCreditsToGM,
                                         byte[] CumulativeCreditsToHost,
                                         byte[] CumulativeCashableCreditsToGM)
        {
            byte[] response = LongPollFactory.Singleton.GetResponse1D(address, CumulativePromoCreditsToGM, CumulativeNonCashableCreditsToGM, CumulativeCreditsToHost, CumulativeCashableCreditsToGM);
            writePort(response, print);
        }

        // Response 1E
        public void MultipleMetersGamingMachineLP1E(byte address,
                                                    byte[] Bills1Accepted,
                                                    byte[] Bills5Accepted,
                                                    byte[] Bills10Accepted,
                                                    byte[] Bills20Accepted,
                                                    byte[] Bills50Accepted,
                                                    byte[] Bills100Accepted)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForMultipleMetersLP1E(address,
                                                                                         Bills1Accepted,
                                                                                         Bills5Accepted,
                                                                                         Bills10Accepted,
                                                                                         Bills20Accepted,
                                                                                         Bills50Accepted,
                                                                                         Bills100Accepted);
            writePort(response, print);
        }

        // Response 1F
        public void SendGamingMachineIDandInformationLongPoll(byte address,
                                                              byte[] gameId,
                                                              byte[] additionalId,
                                                              byte denomination,
                                                              byte maxBet,
                                                              byte progressiveGroup,
                                                              byte[] gameOptions,
                                                              byte[] paytableId,
                                                              byte[] BasePercentage)

        {

            byte[] response = LongPollFactory.Singleton.GetResponseSendGamingMachineIDandInformationLongPoll(address,
                                                                                                             gameId,
                                                                                                             additionalId,
                                                                                                             denomination,
                                                                                                             maxBet,
                                                                                                             progressiveGroup,
                                                                                                             gameOptions,
                                                                                                             paytableId,
                                                                                                             BasePercentage);
            writePort(response, print);
        }

        //Response 20
        public void SendDollarValueForBillsMeter(byte address,
                                                 byte[] dollar_value)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP20(address,
                                                                           dollar_value);
            writePort(response, print);
        }

        // Response 21
        public void ROMSignatureVerificationResponse(byte address,
                                                     byte[] romSignature)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForROMSignature(address, romSignature);
            writePort(response, print);
        }

        //Response 27
        public void SendCurrentPromotionalCredits(byte address,
                                                  byte[] value)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP27(address,
                                                                           value);
            writePort(response, print);
        }

        //Response 28
        public void SendTransferLog(byte address,
                                    byte[] raw_logs)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP28(address,
                                                                           raw_logs);
            writePort(response, print);
        }

        // Response 2D
        public void GetResponseSendHandPaidCancelledCredits(byte address, byte[] gameNumber, byte[] value)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseSendHandPaidCancelledCredits(address, gameNumber, value);
            writePort(response, print);
        }

        // Response 2F
        public void SendSelectedMeters(byte address,
                                       byte[] gameNumber,
                                       byte[] metersResult)
        {
            byte[] response = LongPollFactory.Singleton.GetResponse2F(address, gameNumber, metersResult);

            writePort(response, print);

        }

        // Response 48
        public void GetResponseSendLastBillAcceptedInformation(byte address,
                                                               byte countryCode,
                                                               byte denominationCode,
                                                               byte[] billMeter)
        {
            byte[] response = LongPollFactory.Singleton.GetResponse48(address, countryCode, denominationCode, billMeter);
            writePort(response, print);
        }

        // Response 4C
        public void SetSecureEnhancedValidationIDResponse(byte address,
                                                          byte[] machineID,
                                                          byte[] sequenceNumber)
        {
            byte[] response = LongPollFactory.Singleton.BuildSetSecureEnhancedValidationID(address, machineID, sequenceNumber);
            writePort(response, print);
        }

        // Response 4D
        public void SendEnhancedValidationInformation(byte address,
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
            byte[] response = LongPollFactory.Singleton.GetResponseEnhancedValidationInformation(address, validationType,
                                                                                                         indexNumber,
                                                                                                         date,
                                                                                                         time,
                                                                                                         validationNumber,
                                                                                                         amount,
                                                                                                         ticketNumber,
                                                                                                         validationSystemId,
                                                                                                         expiration,
                                                                                                         poolId);

            writePort(response, print);

        }

        // Response 50
        public void SendValidationMetersResponse(byte address,
                                                 byte validationType,
                                                 int totalValidations,
                                                 int cumulativeAmount)
        {
            byte[] response = LongPollFactory.Singleton.GetResponse50(address, validationType, totalValidations, cumulativeAmount);
            writePort(response, print);
        }

        // Response 51
        public void SendNumberOfGamesImplemented(byte address,
                                                 uint numberOfGames)
        {
            byte[] response = LongPollFactory.Singleton.GetResponse51(address, numberOfGames);
            writePort(response, print);
        }

        // Response 53
        public void SendGameNConfiguration(byte address,
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
            byte[] response = LongPollFactory.Singleton.GetResponseSendGameNConfiguration(address,
                                                                                          gameNumber,
                                                                                          gameId,
                                                                                          additionalId,
                                                                                          denomination,
                                                                                          maxBet,
                                                                                          progressiveGroup,
                                                                                          gameOptions,
                                                                                          paytableId,
                                                                                          BasePercentage);
            writePort(response, print);
        }

        // Response 54
        public void SendSASVersionIDAndGameSerialNumber(byte address, byte[] SASVersion, byte[] GamingMachineSerialNumber)
        {
            if (GamingMachineSerialNumber.Count() <= 40)
            {
                byte[] response = LongPollFactory.Singleton.GetResponseSendSASVersionIDAndGamingMachineSerialNumber(address, SASVersion, GamingMachineSerialNumber);
                writePort(response, print);
            }
        }

        // Response 55
        public void SendSelectedGameNumber(byte address,
                                           byte[] selected_game_number)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP55(address,
                                                                           selected_game_number);
            writePort(response, print);
        }

        // Response 56
        public void SendEnabledGameNumbers(byte address,
                                           List<byte[]> EnabledGameNumbers)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLP56(address,
                                                                           EnabledGameNumbers);
            writePort(response, print);
        }

        // Response 57
        public void PendingCashoutInformationResponse(byte address,
                                                      byte cashoutType,
                                                      uint cashoutAmount)
        {
            var aux = LongPollFactory.Singleton.intToBCD5(cashoutAmount).ToList();
            aux.Reverse();
            byte[] response = LongPollFactory.Singleton.GetResponsePendingCashoutInformation(address,
                                                                                            cashoutType,
                                                                                            aux.ToArray());
            writePort(response, print);
        }

        // Response 58
        public void ReceiveValidationNumberResponse(byte address, byte status)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseReceiveValidationNumber(address,
                                                                                          status);
            writePort(response, print);
        }

        // Response 70
        public void SendTicketValidationData(byte address,
                                             byte ticketStatus,
                                             int ticketAmount,
                                             byte parsingCode,
                                             byte[] validationData)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseTicketValidationData(address, ticketStatus, ticketAmount, parsingCode, validationData);

            writePort(response, print);

        }

        // Response 71
        public void RedeemTicket(byte address,
                                byte ticketStatus,
                                byte[] ticketAmount,
                                byte[] parsingCode,
                                byte[] validationData)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseRedeemTicketGM(address, ticketStatus, ticketAmount, parsingCode, validationData);

            writePort(response, print);

        }


        // Response 72
        public void Send72Response(byte address,
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
            byte[] response = LongPollFactory.Singleton.GetResponseLP72( address,
                                                                        transactionbufferposition,
                                                                        transferStatus,
                                                                        receiptStatus,
                                                                        transferType,
                                                                        cashableAmount,
                                                                        restrictedAmount,
                                                                        nonRestrictedAmount,
                                                                        transferFlags,
                                                                        assetNumber,
                                                                        transactionID,
                                                                        TransactionDateTime,
                                                                        Expiration,
                                                                        poolId,
                                                                        cumulativeCashableAmount,
                                                                        cumulativeRestrictedAmount,
                                                                        cumulativeNonRestrictedAmount);

            writePort(response, print);

        }

        // Response 74
        public void Send74Response(byte address,
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
            byte[] response = LongPollFactory.Singleton.GetResponseLP74(address, 
                                                                        assetNumber, 
                                                                        gameLockStatus, 
                                                                        availableTrasfers, 
                                                                        hostCashoutStatus, 
                                                                        aftStatus, 
                                                                        maxBuferIndex, 
                                                                        currentCashableAmount, 
                                                                        currentRestrictedAmount, 
                                                                        currentNonRestrictedAmount,
                                                                        gamingMachineTransferLimit, 
                                                                        restrictedExpiration, 
                                                                        restrictedPoolID);


            writePort(response, print);
        }
        // Response 7B
        public void SendExtendedValidationStatusGamingMachine(byte address,
                                                              byte[] assetNumber,
                                                              byte[] statusBits,
                                                              byte[] cashableTicketAndReceiptExpiration,
                                                              byte[] restrictedTicketDefaultExpiration)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseExtendedValidationStatusGamingMachine(address, assetNumber, statusBits, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration);
            writePort(response, print);
        }



        // Response 7C
        public void SetExtendedTicketDataResponse(byte address,
                                                  byte flag)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseExtendedTicketData(address, flag);
            writePort(response, print);
        }

        // Response 7D
        public void SetTicketDataResponse(byte address,
                                          byte flag)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseTicketData(address, flag);
            writePort(response, print);
        }

        // Response 7E
        public void SendCurrentDateTimeGamingMachineLP7E(byte address,
                                                         int month,
                                                         int year,
                                                         int day,
                                                         int hour,
                                                         int minute,
                                                         int second)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForDateTimeLP7E(address,
                                                                                   month,
                                                                                   year,
                                                                                   day,
                                                                                   hour,
                                                                                   minute,
                                                                                   second);
            writePort(response, print);
        }

        // Response 83
        public void SendCumulativeProgressiveWinsAmounts(byte address,
                                                         byte[] gameNumber,
                                                         byte[] CumulativeProgressiveWins)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseLP83(address,
                                                                        gameNumber,
                                                                        CumulativeProgressiveWins);
            writePort(response, print);
        }

        // Response 84
        public void SendProgressiveWinAmount(byte address,
                                             byte group,
                                             byte level,
                                             int amount)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseLP84(address,
                                                                        group,
                                                                        level,
                                                                        amount);
            writePort(response, print);
        }

        // Response 85
        public void SendSASProgressiveWinAmount(byte address,
                                                byte group,
                                                byte level,
                                                int amount)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseLP85(address,
                                                                        group,
                                                                        level,
                                                                        amount);
            writePort(response, print);
        }

        // Response 87
        public void SendMultipleSASProgressiveWinAmounts(byte address,
                                                         byte group,
                                                         List<Tuple<byte, int>> amounts)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseLP87(address,
                                                                        group,
                                                                        amounts);
            writePort(response, print);
        }

        // Response 94
        public void SendResetHandpayGamingMachineResponse(byte address,
                                                          byte resetCode)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseResetHandpayGaming(address, resetCode);
            writePort(response, print);
        }

        // Response 9A
        public void Response9A(byte address,
                               byte[] gameNumber,
                               byte[] deductible,
                               byte[] nonDeductible,
                               byte[] wagetMatch)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseLP9A(address, gameNumber, deductible, nonDeductible, wagetMatch);
            writePort(response, print);
        }

        // Response A0 for game 00
        public void SendA0Response(byte address,
            byte[] features1,
            byte[] features2,
            byte[] features3)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseA0(address,
                features1,
                features2,
                features3);
            writePort(response, print);
        }
        // Response A0 for game selected
        public void SendEnabledFeatures(byte address,
                                        byte[] gameNumber,
                                        byte feat1,
                                        byte feat2,
                                        byte feat3)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseSendEnabledFeatures(address, gameNumber, feat1, feat2, feat3);
            writePort(response, print);
        }

        // Response A4
        public void SendCashoutLimitGamingMachine(byte address, byte[] gameNumber, byte[] cashoutLimit)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseSendCashoutLimitGamingMachine(address, gameNumber, cashoutLimit);
            writePort(response, print);
        }

        // Response A8
        public void EnableJackpotHandpayResetMethod(byte address, byte ACKCode)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseEnableJackpotHandpayResetMethod(address, ACKCode);
            writePort(response, print);
        }

        // Response B1
        public void SendCurrentPlayerDenomination(byte address,
                                                  byte current_player_denomination)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLPB1(address,
                                                                           current_player_denomination);
            writePort(response, print);
        }

        // Response B2
        public void SendEnabledPlayerDenominations(byte address,
                                                   byte NumberOfDenominations,
                                                   byte[] playerDenominations)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLPB2(address, NumberOfDenominations, playerDenominations);
            writePort(response, print);
        }

        // Response B3
        public void SendTokenDenomination(byte address,
                                          byte TokenDenomination)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLPB3(address, TokenDenomination);
            writePort(response, print);
        }
         // Response B4
        public void SendWagerCategoryInformation(byte address,
                                                         byte[] gameNumber,
                                                         byte[] wagerCategory,
                                                         byte[] paybackPercentage,
                                                         byte[] coinInMeterValue)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseForLPB4(address,
                                                                           gameNumber,
                                                                           wagerCategory,
                                                                           paybackPercentage,
                                                                           coinInMeterValue);
            writePort(response, print);
        }

        // Response B5
        public void SendExtendedGameNInformationResponse(byte address,
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
            byte[] response = LongPollFactory.Singleton.GetResponseExtendedGameInformation(address,
                                                                                           gameNumber,
                                                                                           maxBet,
                                                                                           progressiveGroup,
                                                                                           progressiveLevels,
                                                                                           gameNameLength,
                                                                                           gameName,
                                                                                           paytableLength,
                                                                                           paytableName,
                                                                                           wagerCategories);
            writePort(response, print);
        }

        // Response Registration
        public void SendRegistrationResponse(byte address,
                                          byte command,
                                          byte reg_status,
                                          byte[] asset_number,
                                          byte[] registration_key,
                                          byte[] pos_id)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseRegistration(address,
                                                                                 command,
                                                                                 reg_status,
                                                                                 asset_number,
                                                                                 registration_key,
                                                                                 pos_id);
            writePort(response, print);
        }

        // Response Registration
        public void SendExtendedMetersResponse(byte command,
                                                byte address,
                                                byte[] gameNumber,
                                                byte[] meters)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseExtendedMeters(command,
                                                                                  address,
                                                                                  gameNumber,
                                                                                  meters);
            writePort(response, print);
        }

        // Response for single meter
        public void SendGamingMachineSingleMeterAccountingResponse(byte address,
                                                                   byte single_meter_accounting_long_poll,
                                                                   byte[] meter)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseHostSingleMeterAccounting(address, single_meter_accounting_long_poll, meter);
            writePort(response, print);
        }

        // Send ACK
        public void SendACK()
        {
            writePort(new byte[] { GetACKByte() }, print);
        }

        // Send NACK
        public void SendNACK()
        {
            writePort(new byte[] { GetNACKByte() }, print);
        }

        public void SendException(byte address, byte exception, byte[] data)
        {
            byte[] response = LongPollFactory.Singleton.GetResponseException(address, exception, data, GetEnabledRealTime());
            writePort(response, print);
        }

        #endregion


        // Update Enabled RealTime
        public void UpdateEnabledRealTime(bool arg)
        {
            RealTimeBool = arg;
        }












    }
}

