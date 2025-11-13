using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SASComms
{

    /// <summary>
    /// Handler de las responses de la EGM (SAS)
    /// Handler of EGM Responses (SAS)
    /// </summary>
    public sealed class SASResponseHandler
    {

        /// <summary>
        /// Función privada, que retorna el tamaño mínimo que puede tomar el meter de código code_.
        /// Se usa para determinar el límite de parseo y de lectura de las respuestas de meters. Y para determinar cuantos bytes responder cuando consultan determinado meter
        /// Private function, which returns the minimum size that the code meter can take
        /// It is used to determine the parsing and reading limit of meter responses. And to determine how many bytes to answer when they query a certain meter
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public int MinSize(byte code)
        {
            /*Si el code está entre el 00 y el 0D, retorno 4 */
            /* If the code is between the 00 and 0D, return 4 */
            if (0x00 <= code && code < 0x0D)
                return 4;
            /* Si el code está entre el 0D y el 10, retorno 5 */
            /* If the meter code is between the 0D and 10, return 5 */
            if (0x0D <= code && code < 0x11)
                return 5;
            /* Si el code está entre el 11 y el 7E, retorno 4*/
            /* If the meter code is between the 11 and 7E, return 4*/
            if (0x11 <= code && code <= 0x7F)
                return 4;
            byte[] arr = new byte[] {0x80, 0x82, 0x84, 0x86, 0x88, 0x8A, 0x8C, 0x8E, 
                                     0x90, 0x92, 0xA0, 0xA2, 0xA4, 0xA6, 0xA8, 0xAA,
                                     0xAC, 0xAE, 0xB0, 0xB8, 0xBA, 0xBC};
            /* Si el array está entre el 80, 82, 84, 86, 88, 8A, 8C, 8E, 90, 92 */
            /* If the array is among the meter codes 80, 82, 84, 86, 88, 8A, 8C, 8E, 90, 92 */
            if (arr.Contains(code))
            {
                return 5; /* Retorno 5 */
            }
            else
            {
                return 4; /* Si no, retorno 4 */
            }

        }
        
        private bool? realtime = false;
        public EventArgs e = null;

        #region DEFINICIÓN DE CLASES QUE SERVIRAN COMO PAQUETES PARA ENVIAR AL MAINCONTROLLER

        /// <summary>
        /// La clase Meter
        /// Meter class
        /// </summary>
        public class Meter
        {
            public string meterName; // Nombre del meter -- Meter Name
            public byte? meterCode; // Código en byte del meter -- Meter byte code 
            public int value; // El valor del meter -- Meter value
            public byte[] gameNumber; // El número de juego -- Game number
        }

        /// <summary>
        /// La clase GameInfo -- Game info class
        /// </summary>
        public class GamingInfo
        {
            public string _gameID;
            public string _additionalID;
            public byte _denomination;
            public byte _maxBet;
            public byte _progressiveGroup;
            public byte[] _gameOptions;
            public string _paytableID;
            public string _basePercentage;
        }

        /// <summary>
        ///  La clase Response70Parameters, que representa una response al long poll 70 
        /// The Respons70Parameter class, representing the response to the long poll 70
        /// </summary>
        public class Response70Parameters
        {
            public byte ticketStatus;
            public int ticketAmount;
            public byte parsingCode;
            public byte[] validationData;
        }

        /// <summary>
        /// La clase RedeemTicket, que representa el ticket de una redemption
        /// The RedeemTicket class, representing the ticket of a redemption
        /// </summary>
        public class RedeemTicket
        {
            public byte machineStatus;
            public int amount;
            public byte parsingCode;
            public byte[] validationData;
        }

        /// <summary>
        ///  La clase PendingCashoutInformation, que representa la información de un cashout pendiente 
        /// The PendingCashoutInformation class, representing the information of a pending cashout
        /// </summary>
        public class PendingCashoutInformation
        {
            public byte _cashoutType;
            public int _amount;
        }

        #endregion


        #region SASRESPONSEHANDLER EVENTS

        #region "Exceptions"

        public event DefaultExceptionHandler DefaultException; public delegate void DefaultExceptionHandler(byte exception, byte[] exceptionData, EventArgs e);

        public event OperatorChangedOptions3CHandler OperatorChangedOptions3C; public delegate void OperatorChangedOptions3CHandler(EventArgs e);
        public event TransferReady69Handler TransferReady69; public delegate void TransferReady69Handler(EventArgs e);
        public event HandpayPending51Handler HandpayPending51; public delegate void HandpayPending51Handler(EventArgs e);
        public event HandpayReset52Handler HandpayReset52; public delegate void HandpayReset52Handler(EventArgs e);
        public event ValidationNeeded57Handler ValidationNeeded57; public delegate void ValidationNeeded57Handler(EventArgs e);
        public event AnyBillInsertedHandler AnyBillInserted; public delegate void AnyBillInsertedHandler(EventArgs e);
        public event CashoutTicketHasBeenPrinted3DHandler CashoutTicketHasBeenPrinted3D; public delegate void CashoutTicketHasBeenPrinted3DHandler(EventArgs e);
        public event TicketInserted67Handler TicketInserted67; public delegate void TicketInserted67Handler(EventArgs e);
        public event Spin7FHandler Spin7F; public delegate void Spin7FHandler(EventArgs e);
        public event E6FCompletedHandler E6FCompleted; public delegate void E6FCompletedHandler(EventArgs e);
        public event TicketTransferCompleted68Handler TicketTransferCompleted68; public delegate void TicketTransferCompleted68Handler(EventArgs e);
        public event ValidationIDNotConfigured3FHandler ValidationIDNotConfigured3F; public delegate void ValidationIDNotConfigured3FHandler(EventArgs e);
        public event SelectedGame8CHandler SelectedGame8C; public delegate void SelectedGame8CHandler(EventArgs e);
        public event HandpayValidated3EHandler HandpayValidated3E; public delegate void HandpayValidated3EHandler(EventArgs e);

        #endregion

        #region "Con Parámetros"

        public event LPReceivedHandler LPReceived; public delegate void LPReceivedHandler(byte lpcode, EventArgs e);

        public event MeterUpdatedHandler MeterUpdated; public delegate void MeterUpdatedHandler(Meter m, EventArgs e);

        public event InfoUpdatedHandler InfoUpdated; public delegate void InfoUpdatedHandler(GamingInfo gi, EventArgs e);

        public event CashoutLimitReceivedHandler CashoutLimitReceived; public delegate void CashoutLimitReceivedHandler(byte[] gameNumber, byte[] cashoutLimit, EventArgs e);

        public event TransactionReceivedHandler TransactionReceived; public delegate void TransactionReceivedHandler(byte[] transaction, EventArgs e);

        public event PendingCashoutInformationSentHandler PendingCashoutSent; public delegate void PendingCashoutInformationSentHandler(PendingCashoutInformation i, EventArgs e);

        public event ValidationNumberReceivedHandler ValidationNumberReceived; public delegate void ValidationNumberReceivedHandler(byte status, EventArgs e);

        public event TicketValidationReceivedHandler TicketValidationReceived; public delegate void TicketValidationReceivedHandler(Response70Parameters data, EventArgs e);

        public event RedeemTicketReceivedHandler RedeemTicketReceived; public delegate void RedeemTicketReceivedHandler(RedeemTicket ticket, EventArgs e);
        public event HandpayInformationReceivedHandler HandpayInformationReceived; public delegate void HandpayInformationReceivedHandler(byte progressiveGroup,
                                                                                                                                          byte level,
                                                                                                                                          byte[] amount,
                                                                                                                                          byte[] partialPay,
                                                                                                                                          byte resetID, EventArgs e);

        public event SendEnhancedValidationInformationResponseHandler SendEnhancedValidationInformationResponse; public delegate void SendEnhancedValidationInformationResponseHandler(byte validationType,
                                                                                                                                                                                       byte indexNumber,
                                                                                                                                                                                       byte[] date,
                                                                                                                                                                                       byte[] time,
                                                                                                                                                                                       byte[] validationNumber,
                                                                                                                                                                                       byte[] amount,
                                                                                                                                                                                       byte[] ticketNumber,
                                                                                                                                                                                       byte validationSystemId,
                                                                                                                                                                                       byte[] expiration,
                                                                                                                                                                                       byte[] poolId, EventArgs e);

        public event SendEnabledFeaturesResponseHandler SendEnabledFeaturesResponse; public delegate void SendEnabledFeaturesResponseHandler(byte[] gameNumber,
                                                                                                                                             byte features1,
                                                                                                                                             byte features2,
                                                                                                                                             byte features3,
                                                                                                                                             EventArgs e);
        public event SendNumberOfGamesImplementedHandler SendNumberOfGamesImplemented; public delegate void SendNumberOfGamesImplementedHandler(byte[] numberOfGames,
                                                                                                                                                EventArgs e);

        public event SendWagerCategoryInformationHandler SendWagerCategoryInformationResponse; public delegate void SendWagerCategoryInformationHandler(byte[] gameNumber,
                                                                                                                                                        byte[] wagerCategory,
                                                                                                                                                        byte[] paybackPercentage,
                                                                                                                                                        byte[] coinInMeterValue,
                                                                                                                                                        EventArgs e); 
                                                                                                                                                      
        public event SendGameNExtendedInformationResponseHandler SendGameNExtendedInformationResponse; public delegate void SendGameNExtendedInformationResponseHandler(byte[] gameNumber,
                                                                                                                                                                        byte[] maxBet,
                                                                                                                                                                        byte progressiveGroup,
                                                                                                                                                                        byte[] progressiveLevels,
                                                                                                                                                                        byte gameNameLength,
                                                                                                                                                                        byte[] gameName,
                                                                                                                                                                        byte paytableLength,
                                                                                                                                                                        byte[] paytableName,
                                                                                                                                                                        byte[] wagerCategories,
                                                                                                                                                                        EventArgs e);

        public event SendDateTimeGamingMachineResponseHandler SendDateTimeGamingMachineResponse; public delegate void SendDateTimeGamingMachineResponseHandler(byte[] date,
                                                                                                                                                               byte[] time,
                                                                                                                                                               EventArgs e);
        public event SendSelectedGameNumberHandler SendSelectedGameNumber; public delegate void SendSelectedGameNumberHandler(byte[] gameNumber,
                                                                                                                              EventArgs e);

        public event SendEnabledGamesNumbersHandler SendEnabledGamesNumbers; public delegate void SendEnabledGamesNumbersHandler(List<byte[]> EnabledGames,
                                                                                                                                 EventArgs e);

        public event SendCurrentPlayerDenominationHandler SendCurrentPlayerDenomination; public delegate void SendCurrentPlayerDenominationHandler(byte currentPlayerDenomination,
                                                                                                                                                   EventArgs e);

        public event SendDollarsValueOfBillsMeterHandler SendDollarsValueOfBillsMeter; public delegate void SendDollarsValueOfBillsMeterHandler(int DollarsValue,
                                                                                                                                                EventArgs e);

        public event SendEnabledPlayerDenominationsHandler SendEnabledPlayerDenominations; public delegate void SendEnabledPlayerDenominationsHandler(byte numberOfDenominations,
                                                                                                                                                      byte[] playerDenominations,
                                                                                                                                                      EventArgs e);
        public event SendTokenDenominationHandler SendTokenDenomination; public delegate void SendTokenDenominationHandler(byte tokenDenomination,
                                                                                                                           EventArgs e);

        public event SendTournamentGamesPlayedHandler SendTournamentGamesPlayed; public delegate void SendTournamentGamesPlayedHandler(byte[] gameNumber,
                                                                                                                                       byte[] meter,
                                                                                                                                       EventArgs e);

        public event SendTournamentGamesWonHandler SendTournamentGamesWon; public delegate void SendTournamentGamesWonHandler(byte[] gameNumber,
                                                                                                                              byte[] meter,
                                                                                                                              EventArgs e);

        public event SendTournamentCreditsWageredHandler SendTournamentCreditsWagered; public delegate void SendTournamentCreditsWageredHandler(byte[] gameNumber,
                                                                                                                                                byte[] meter,
                                                                                                                                                EventArgs e);

        public event SendTournamentCreditsWonHandler SendTournamentCreditsWon; public delegate void SendTournamentCreditsWonHandler(byte[] gameNumber,
                                                                                                                                    byte[] meter,
                                                                                                                                    EventArgs e);

        public event SendTournamentMetersHandler SendTournamentMeters; public delegate void SendTournamentMetersHandler(byte[] gameNumber,
                                                                                                                        byte[] gamesplayed,
                                                                                                                        byte[] gameswon,
                                                                                                                        byte[] creditswagered,
                                                                                                                        byte[] creditswon,
                                                                                                                        EventArgs e);

        public event AFTRegisterGamingMachineResponseHandler AFTRegisterGamingMachineResponse; public delegate void  AFTRegisterGamingMachineResponseHandler(byte reg_status,
                                                                                                                                                             byte[] assetNumber,
                                                                                                                                                             byte[] registrationKey,
                                                                                                                                                             byte[] regPosId,
                                                                                                                                                             EventArgs e);
        public event SendSASVersionIDAndGamingMachineSerialNumberHandler SendSASVersionIDAndGamingMachineSerialNumber; public delegate void SendSASVersionIDAndGamingMachineSerialNumberHandler(byte[] SASVersion,
                                                                                                                                                                                                byte[] GMSerialNumber,
                                                                                                                                                                                                EventArgs e);

        public event SendLastBillAcceptedInformationResponseHandler SendLastBillAcceptedInformationResponse; public delegate void SendLastBillAcceptedInformationResponseHandler(byte countryCode,
                                                                                                                                                                                 byte denominationCode,
                                                                                                                                                                                 byte[] billMeter,
                                                                                                                                                                                 EventArgs e);

        public event ROMSignatureVerificationResponseHandler ROMSignatureVerificationResponse; public delegate void ROMSignatureVerificationResponseHandler(byte[] romSignature,
                                                                                                                                                            EventArgs e);
        public event ExtendedValidationStatusResponseHandler ExtendedValidationStatusResponse; public delegate void ExtendedValidationStatusResponseHandler(byte[] assetNumber, 
                                                                                                                                                            byte[] statusBits, 
                                                                                                                                                            byte[] cashableTicketAndReceiptExpiration, 
                                                                                                                                                            byte[] restrictedTicketDefaultExpiration,
                                                                                                                                                            EventArgs e);

        public event AFTLockAndStatusRequestGamingMachineResponseHandler AFTLockAndStatusRequestGamingMachineResponse; public delegate void AFTLockAndStatusRequestGamingMachineResponseHandler(byte[] assetNumber, 
                                                                                                                                                                                                byte gameLockStatus, 
                                                                                                                                                                                                byte availableTransfers, 
                                                                                                                                                                                                byte hostCashoutStatus,
                                                                                                                                                                                                byte AFTStatus,
                                                                                                                                                                                                byte maxBufferIndex,
                                                                                                                                                                                                byte[] currentCashableAmount,
                                                                                                                                                                                                byte[] currentRestrictedAmount,
                                                                                                                                                                                                byte[] currentNonRestrictedAmount,
                                                                                                                                                                                                byte[] gamingMachineTransferLimit,
                                                                                                                                                                                                byte[] restrictedExpiration,
                                                                                                                                                                                                byte[] restrictedPoolId,
                                                                                                                                                                                                EventArgs e);
                                                                                                                                                            
        public event RealTimeEventHandler RealTimeEvent; public delegate void RealTimeEventHandler(byte rt, EventArgs e);
        public event AllMetersUpdatedHandler AllMetersUpdated; public delegate void AllMetersUpdatedHandler(EventArgs e);

        #endregion

        #endregion

        private SASResponseHandler()
        {

        }


        /// <summary>
        /// Constructor Singleton */
        /// Singleton Constructor
        /// </summary>
        private static readonly Lazy<SASResponseHandler> lazy = new Lazy<SASResponseHandler>(() => new SASResponseHandler());
        public static SASResponseHandler Singleton
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// Acceso a índice con handleo de errores: buf[index]
        /// Access to index with error handling: buf[index]
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
        ///  Método asincrónico que toma un byte, y un array de data y lanza la excepción 
        /// Asynchronous method that takes a byte, and a data array and throws the exception
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="exceptionData"></param>
        /// <returns></returns>
        public async Task<int> LaunchException(byte exception, byte[] exceptionData)
        {
            DefaultException(exception, exceptionData, e);
            return 0;
        }

        /// <summary>
        /// Método que procesa una cierta excepción y lanza el evento correspondiente 
        /// Method that processes a certain exception and throws the event associated
        /// </summary>
        /// <param name="exception"></param>
        public void ProcessException(byte? exception)
        {
            switch (exception)
            {
                case 0x3C:
                    OperatorChangedOptions3C(e);
                    break;
                case 0x3D:
                    CashoutTicketHasBeenPrinted3D(e);
                    break;
                case 0x3E:
                    HandpayValidated3E(e);
                    break;
                case 0x3F:
                    ValidationIDNotConfigured3F(e);
                    break;
                case 0x47:
                    AnyBillInserted(e);
                    break;
                case 0x48:
                    AnyBillInserted(e);
                    break;
                case 0x49:
                    AnyBillInserted(e);
                    break;
                case 0x4A:
                    AnyBillInserted(e);
                    break;
                case 0x4B:
                    AnyBillInserted(e);
                    break;
                case 0x4C:
                    AnyBillInserted(e);
                    break;
                case 0x4D:
                    AnyBillInserted(e);
                    break;
                case 0x4E:
                    AnyBillInserted(e);
                    break;
                case 0x4F:
                    AnyBillInserted(e);
                    break;
                case 0x50:
                    AnyBillInserted(e);
                    break;
                case 0x51:
                    HandpayPending51(e);
                    break;
                case 0x52:
                    HandpayReset52(e);
                    break;
                case 0x57:
                    ValidationNeeded57(e);
                    break;
                case 0x67:
                    TicketInserted67(e);
                    break;
                case 0x68:
                    TicketTransferCompleted68(e);
                    break;
                case 0x69:
                    TransferReady69(e);
                    break;
                case 0x7F:
                    Spin7F(e);
                    break;
                case 0x8C:
                    SelectedGame8C(e);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Método que lanza el evento RealTime con el valor proporcionado como parámetro
        /// Method that throws the event RealTime with the value passed as parameter
        /// </summary>
        /// <param name="b"></param>
        public void SendRealTimeEvent(byte b)
        {
            try
            {
                // Si el byte es 0x01 y el realtime es nulo, lo pasa a true
                // If the byte is 0x01 and realtime is null, set it as true
                if (b == 0x01 && realtime == null)
                {
                    realtime = true;
                    RealTimeEvent(b, e);
                }
                // Si el byte es 0x00 y el realtime es nulo, lo pasa a false
                // If the byte is 0x00, and realtime is null, set it as false
                else if (b == 0x00 && realtime == null)
                {
                    realtime = false;
                    RealTimeEvent(b, e);
                }
                // Si el byte es 0x01 y el realtime es false, lo pasa a true
                // If the byte is 0x01, and realtime is false, set it as true
                else if (b == 0x01 && realtime == false)
                {
                    realtime = true;
                    RealTimeEvent(b, e);
                }
                // Si el byte es 0x01 y el realtime es true, lo pasa a false
                // If the byte is 0x01, and realtime is true, set it as false
                else if (b == 0x00 && realtime == true)
                {
                    realtime = false;
                    RealTimeEvent(b, e);
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// Método asincrónico que analiza las responses de la EGM (EN ORDEN POR LONG POLL CODE)
        /// Asynchronous method that analyzes the response from EGM (ORDER BY LONG POLL CODE)
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<int> Analyze(byte[] response)
        {
            /* En el caso de que la response sea 1, lo procesa como excepción*/
            /* In case that the response is 1,  process it as exception */
            if (response.Length == 1)
            {
                ProcessException(GetByteFromArrayIndex(response, 0));
            }
            /* En el caso de que la response tenga un tamaño mayor a 1..*/
            /* In case that response has a length more than 1 element*/
            if (response.Length > 1)
            {
                /* Lee el primer byte */
                /* Read the first byte */
                switch (GetByteFromArrayIndex(response, 1))
                {
                    /* Response from FF*/
                    case 0xFF:
                        /* A veces llega el FF-FF, por lo que se chequea que el primer byte no sea FF */
                        /* Sometimes the FF-FF arrives, so it is checked that the first byte is not FF  */
                        if (GetByteFromArrayIndex(response, 0) != 0xFF) 
                        {
                            ProcessException(GetByteFromArrayIndex(response, 2));
                        }
                        break;
                    /* Response from 1B */
                    case 0x1B:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and send it*/
                                byte progressiveGroup = response[2];
                                byte level = response[3];
                                byte[] amount = new byte[] { response[4], response[5], response[6], response[7], response[8] };
                                byte[] partialPay = new byte[] { response[9], response[10] };
                                byte resetID = response[11];
                                HandpayInformationReceived(progressiveGroup,
                                                           level,
                                                           amount,
                                                           partialPay,
                                                           resetID, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 1C */
                    case 0x1C:
                        /*Parseo cada parte de la response y envío */
                        /* Parsing each part of the response and send it */
                        byte[] _totalCoinInMeter = { response[2], response[3], response[4], response[5] };
                        byte[] _totalCoinOutMeter = { response[6], response[7], response[8], response[9] };
                        byte[] _totalDropMeter = { response[10], response[11], response[12], response[13] };
                        byte[] _totalJackpotMeter = { response[14], response[15], response[16], response[17] };
                        byte[] _gamesPlayedMeter = { response[18], response[19], response[20], response[21] };
                        byte[] _gamesWonMeter = { response[22], response[23], response[24], response[25] };
                        byte[] _doorOpenMeter = { response[26], response[27], response[28], response[29] };
                        byte[] _powerResetMeter = { response[30], response[31], response[32], response[33] };
                        MeterUpdated(new Meter { meterName = "Total_Coin_In", value = int.Parse(BitConverter.ToString(_totalCoinInMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Total_Coin_Out", value = int.Parse(BitConverter.ToString(_totalCoinOutMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Total_Drop_Meter", value = int.Parse(BitConverter.ToString(_totalDropMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Total_Jackpot_Meter", value = int.Parse(BitConverter.ToString(_totalJackpotMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Games_Played_Meter", value = int.Parse(BitConverter.ToString(_gamesPlayedMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Games_Won_Meter", value = int.Parse(BitConverter.ToString(_gamesWonMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Door_Open_Meter", value = int.Parse(BitConverter.ToString(_doorOpenMeter).Replace("-", "")) }, e);
                        MeterUpdated(new Meter { meterName = "Power_Reset_Meter", value = int.Parse(BitConverter.ToString(_powerResetMeter).Replace("-", "")) }, e);
                        try { AllMetersUpdated(e); } catch { }
                        break;
                    /* Response from 1F */
                    case 0x1F:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            GamingInfo info = new GamingInfo();
                            info._gameID = System.Text.Encoding.ASCII.GetString(new byte[] { response[2], response[3] });
                            info._additionalID = System.Text.Encoding.ASCII.GetString(new byte[] { response[4], response[5], response[6] });
                            info._denomination = response[7];
                            info._maxBet = response[8];
                            info._progressiveGroup = response[9];
                            info._gameOptions = new byte[] { response[10], response[11] };
                            info._paytableID = System.Text.Encoding.ASCII.GetString(new byte[] { response[12], response[13], response[14], response[15], response[16], response[17] });
                            info._basePercentage = System.Text.Encoding.ASCII.GetString(new byte[] { response[18], response[19], response[20], response[21] });
                            InfoUpdated(info, e);

                            break;
                        }
                    /* Response from 20 */
                    case 0x20:
                        {
                        try{
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            byte[] DollarValue = new byte[] {response[2], response[3], response[4], response[5]};
                            MeterUpdated(new Meter { meterName = "TotalBillsInDollars", value = int.Parse(BitConverter.ToString(DollarValue).Replace("-", "")) }, e); 
                            try { AllMetersUpdated(e); } catch {}

                        }
                        catch{

                            }
                            break;
                        }

                    /* Response from 21 */
                    case 0x21:
                    {
                            try
                            {
                                byte[] romSignature = new byte[] { response[2], response[3]};
                                ROMSignatureVerificationResponse(romSignature, e);
                            }
                            catch
                            {

                            }
                            break;
                    }
                    /* Response from 2A */
                    case 0x2A:
                    {
                        try
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            byte[] value = new byte[] {response[2], response[3], response[4], response[5]};
                            MeterUpdated(new Meter { meterName = "TrueCoinIn", value = int.Parse(BitConverter.ToString(value).Replace("-", "")) }, e);
                            try { AllMetersUpdated(e); } catch {} 
 
                        }
                        catch
                        {

                        }
                        break;
                    }
                    /* Response from 2B */
                    case 0x2B:
                    {
                        try
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            byte[] value = new byte[] {response[2], response[3], response[4], response[5]};
                            MeterUpdated(new Meter { meterName = "TrueCoinOut", value = int.Parse(BitConverter.ToString(value).Replace("-", "")) }, e);
                            try { AllMetersUpdated(e); }catch { }

                        }
                        catch
                        {

                        }
                        break;
                    }
                    /* Response from 2D */
                    case 0x2D:
                    {
                        try{
                            /*Parseo cada parte de la response y envío */
                             /* Parsing each part of the response and sending it */
                            byte[] gameNumber = new byte[] { response[2], response[3]};
                            byte[] handpayCredits = new byte[] {response[4], response[5], response[6], response[7]};
                            MeterUpdated(new Meter { meterName = "", meterCode = 0x03, value = int.Parse(BitConverter.ToString(handpayCredits).Replace("-", "")), gameNumber = gameNumber }, e);
                            try { AllMetersUpdated(e); } catch { }
 
                        }
                        catch
                        {

                        }
                        break;
                    }
                    /* Response from 2F */
                    case 0x2F:
                        {
                            try { LPReceived(0x2F, e); } catch { }
                            /* Por caso... */
                            /* By cases... */

                            /* Si el sexto byte y el decimoprimer byte son 0C y 1B respectivamente, envío dos meters particulares para el current credits y el restricted*/
                            /* If the 6th byte and the 11th byte are 0C and 1B respectively, send two particular meters for the current credits and restricted credits */
                            if (GetByteFromArrayIndex(response, 5) == 0x0C && GetByteFromArrayIndex(response, 10) == 0x1B)
                            {
                                byte[] credits = { response[6], response[7], response[8], response[9] };
                                byte[] restricted_credits = { response[11], response[12], response[13], response[14] };
                                MeterUpdated(new Meter { meterName = "Current_Credits_Meter", value = int.Parse(BitConverter.ToString(credits).Replace("-", "")) }, e);
                                MeterUpdated(new Meter { meterName = "Current_Restricted_Credits_Meter", value = int.Parse(BitConverter.ToString(restricted_credits).Replace("-", "")) }, e);
                            }
                            else /* Caso contrario */ /* Otherwise */
                            {
                                // Sabemos que el init de los values es 5
                                // We know that the value init is 5
                                int init = 5;
                                // Obtenemos el length
                                // Obtain the length
                                int length = (int)GetByteFromArrayIndex(response, 2).Value;

                                byte[] gameNumber = new byte[] { 
                                                                 GetByteFromArrayIndex(response, 3).Value, 
                                                                 GetByteFromArrayIndex(response, 4).Value 
                                                               };
                                // Le restamos 2 (el CRC) al length
                                // Subtract 2 (CRC) to the length
                                length = length - 2;
                                // Ejemplo de response: 
                                // Response example
                                // [Dir 2F Length _ _ init _ _ _ ....]
                                // Mientras el init no supere a 4+length
                                // While the init doesn't exceed to 4+length
                                while (init <= 4 + length)
                                {
                                    /* En init está el primer meter, no debería ser null */
                                    // In init there is the first meter, it shouldn't be null
                                    if (GetByteFromArrayIndex(response, init) != null)
                                    {
                                        try
                                        {
                                            /* Obtenemos el meter code */
                                            /* Obtain the meter code */
                                            byte MeterCode = GetByteFromArrayIndex(response, init).Value;
                                            /* Obtenemos el min size de este meter*/
                                            /* Obtain the min size of this meter */
                                            int MinSize_ = MinSize(MeterCode);
                                            /* En valueList guardamos los bytes del valor que devolviera el meter */
                                            // In valueList we save the meter value byte array
                                            List<byte> valueList = new List<byte>();
                                            /* [{init} -> meter_code , {init+1+0,  init+1+1,  init+1+2 , ....., init+MinSize_} -> Value, init+MinSize_+1... ]*/
                                            /* Recorremos desde el elemento siguiente hasta el último y guardamos los bytes */
                                            /* Loop from the next element to the last and save the bytes*/
                                            for (int i = 0; i < MinSize_; i++)
                                            {
                                                try { valueList.Add(response[init + 1 + i]); } catch { }
                                            }
                                            /* Guardamos el valueList en un array y enviamos el meter */
                                            /* We save the value list in a array and send the meter */
                                            if (valueList.Count() > 0)
                                            {
                                                byte[] valueArray = valueList.ToArray();
                                                MeterUpdated(new Meter { meterName = "", meterCode = MeterCode, value = int.Parse(BitConverter.ToString(valueArray).Replace("-", "")), gameNumber = gameNumber  }, e);
                                            }
                                            /* Actualizamos el init con el valor de init+MinSize_+1 */
                                            /* Update the init with the value of init+MinSize_+1 */
                                            init += 1 + MinSize_;
                                        }
                                        catch
                                        {
                                            init = 666999666;
                                        }
                                    }
                                }

                            }
                            try { AllMetersUpdated(e); } catch { }
                            break;
                        }
                    /* Response from 48 */
                    case 0x48:
                        {
                            try{
                                byte countryCode =  response[2];
                                byte denominationCode = response[3];
                                byte[] billMeter = new byte[] { response[4], response[5], response[6], response[7]};
                                SendLastBillAcceptedInformationResponse(countryCode,
                                                                        denominationCode,
                                                                        billMeter, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 4D */
                    case 0x4D:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte validationType = response[2];
                                byte indexNumber = response[3];
                                byte[] date = new byte[] { response[4], response[5], response[6], response[7] };
                                byte[] time = new byte[] { response[8], response[9], response[10] };
                                byte[] validationNumber = new byte[] { response[11], response[12], response[13], response[14], response[15], response[16], response[17], response[18] };
                                byte[] amount = new byte[] { response[19], response[20], response[21], response[22], response[23] };
                                byte[] ticketNumber = new byte[] { response[24], response[25] };
                                byte validationSystemId = response[26];
                                byte[] expiration = new byte[] { response[27], response[28], response[29], response[30] };
                                byte[] poolId = new byte[] { response[31], response[32] };
                                SendEnhancedValidationInformationResponse(validationType,
                                                                          indexNumber,
                                                                          date,
                                                                          time,
                                                                          validationNumber,
                                                                          amount,
                                                                          ticketNumber,
                                                                          validationSystemId,
                                                                          expiration,
                                                                          poolId, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 51 */
                    case 0x51:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte[] numberOfGames = new byte[] { response[2], response[3] };
                                SendNumberOfGamesImplemented(numberOfGames, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 54 */
                    case 0x54:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte length = response[2];
                                byte[] SASVersion = new byte[] { response[3], response[4], response[5] };
                                List<byte> GamingSerialNumber = new List<byte>();
                                for (int i = 0; i < length - 3; i++)
                                {
                                    GamingSerialNumber.Add(response[6 + i]);
                                }
                                byte[] GamingSerialNumberArr = GamingSerialNumber.ToArray();
                                SendSASVersionIDAndGamingMachineSerialNumber(SASVersion, GamingSerialNumberArr, e);

                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 55 */
                    case 0x55:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                SendSelectedGameNumber(gameNumber, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 56 */
                    case 0x56:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte length = response[2];
                                byte numberOfGames = response[3];
                                List<byte[]> EnabledGames = new List<byte[]>();
                                for (int i = 0; i < (int)numberOfGames; i = i + 2)
                                {
                                    EnabledGames.Add(new byte[] { response[4 + i], response[5 + i] });
                                }
                                SendEnabledGamesNumbers(EnabledGames, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 57 */
                    case 0x57:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            PendingCashoutInformation info = new PendingCashoutInformation();
                            info._cashoutType = response[2];
                            byte[] amount = { response[3], response[4], response[5], response[6], response[7] };
                            info._amount = int.Parse(BitConverter.ToString(amount).Replace("-", ""));
                            PendingCashoutSent(info, e);
                            break;
                        }
                    /* Response from 58 */
                    case 0x58:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            byte status = response[3];
                            ValidationNumberReceived(status, e);
                            break;
                        }
                    /* Response from 6F */
                    case 0x6F:
                        {
                            /* Misma lógica que el 2F */
                            /* Same logic of 2F */
                            if (GetByteFromArrayIndex(response, 5) == 0x0C && GetByteFromArrayIndex(response, 10) == 0x1B)
                            {
                                byte[] credits = { response[6], response[7], response[8], response[9] };
                                byte[] restricted_credits = { response[11], response[12], response[13], response[14] };
                                MeterUpdated(new Meter { meterName = "Current_Credits_Meter", value = int.Parse(BitConverter.ToString(credits).Replace("-", "")) }, e);
                                MeterUpdated(new Meter { meterName = "Current_Restricted_Credits_Meter", value = int.Parse(BitConverter.ToString(restricted_credits).Replace("-", "")) }, e);
                            }
                            else
                            {
                                int init = 5;
                                int length = (int)GetByteFromArrayIndex(response, 2).Value;

                                byte[] gameNumber = new byte[] { 
                                                                 GetByteFromArrayIndex(response, 3).Value, 
                                                                 GetByteFromArrayIndex(response, 4).Value 
                                                               };
                                length = length - 2;
                                while (init <= 4 + length)
                                {
                                    if (GetByteFromArrayIndex(response, init + 1) != null)
                                    {
                                        try
                                        {
                                            byte MeterCode = GetByteFromArrayIndex(response, init + 1).Value;
                                            int MinSize_ = GetByteFromArrayIndex(response, init + 2).Value;
                                            List<byte> valueList = new List<byte>();
                                            for (int i = 0; i < MinSize_; i++)
                                            {
                                                try { valueList.Add(response[init + 3 + i]); } catch { }
                                            }
                                            if (valueList.Count() > 0)
                                            {
                                                byte[] valueArray = valueList.ToArray();
                                                MeterUpdated(new Meter { meterName = "", meterCode = MeterCode, value = int.Parse(BitConverter.ToString(valueArray).Replace("-", "")), gameNumber = gameNumber }, e);
                                            }
                                            init += 3 + MinSize_;
                                        }
                                        catch
                                        {
                                            init = 666999666;
                                        }
                                    }
                                }

                            }
                            try { AllMetersUpdated(e); } catch {}
                            try { E6FCompleted(e); } catch { }
                            break;
                        }
                    /* Response from 70 */
                    case 0x70:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */ 
                            Response70Parameters data = new Response70Parameters();
                            try
                            {
                                int length = GetByteFromArrayIndex(response, 2).Value;
                                data.ticketStatus = GetByteFromArrayIndex(response, 3).Value;
                                if (data.ticketStatus == 0x00)
                                {
                                    data.ticketAmount = int.Parse(BitConverter.ToString(new byte[] { response[4], response[5], response[6], response[7], response[8] }).Replace("-", ""));
                                    data.parsingCode = GetByteFromArrayIndex(response, 9).Value;

                                    data.validationData = new byte[length - 7];
                                    for (int i = 0; i < length - 7; i++)
                                    {
                                        data.validationData[i] = GetByteFromArrayIndex(response, i + 10).Value;
                                    }
                                }


                            }
                            catch
                            {

                            }
                            TicketValidationReceived(data, e);

                            break;
                        }
                    /* Response from 71 */
                    case 0x71:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            RedeemTicket ticket = new RedeemTicket();
                            try
                            {
                                int length = GetByteFromArrayIndex(response, 2).Value;
                                ticket.machineStatus = GetByteFromArrayIndex(response, 3).Value;
                                ticket.amount = int.Parse(BitConverter.ToString(new byte[] { response[4], response[5], response[6], response[7], response[8] }).Replace("-", ""));
                                ticket.parsingCode = GetByteFromArrayIndex(response, 9).Value;

                                ticket.validationData = new byte[length - 7];
                                int cuantos = 0;
                                for (int i = 0; i < length - 7; i++)
                                {
                                    cuantos++;
                                    ticket.validationData[i] = GetByteFromArrayIndex(response, i + 10).Value;
                                }
                            }
                            catch
                            {

                            }
                            RedeemTicketReceived(ticket, e);
                            break;
                        }
                    /* Response from 72 */
                    case 0x72:
                        {
                            TransactionReceived(response, e);
                            break;
                        }
                    /* Response from 73 */
                    case 0x73:
                        {
                            try
                            {
                                byte registrationStatus = response[3];
                                byte[] assetNumber = new byte[] {response[4], response[5], response[6], response[7]};
                                byte[] registrationKey = new byte[] {response[8], response[9], response[10], response[11], response[12], response[13], response[14], response[15], response[16], response[17], response[18], response[19], response[20], response[21], response[22], response[23], response[24], response[25], response[26], response[27]};
                                byte[] regPosId = new byte[] {response[28], response[29], response[30], response[31]};
                                AFTRegisterGamingMachineResponse(registrationStatus, assetNumber, registrationKey, regPosId, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 74 */
                    case 0x74:
                        {
                             /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            try
                            {
                                byte length = response[2];
                                byte[] assetNumber = new byte[] { response[3], response[4], response[5], response[6] };
                                byte gameLockStatus =  response[7];
                                byte availableTransfers = response[8];
                                byte hostCashoutStatus = response[9];
                                byte AFTStatus = response[10];
                                byte maxBufferIndex = response[11];
                                byte[] currentCashableAmount = new byte[] {response[12], response[13], response[14], response[15], response[16]};
                                byte[] currentRestrictedAmount = new byte[] {response[17], response[18], response[19], response[20], response[21]};
                                byte[] currentNonRestrictedAmount = new byte[] {response[22], response[23], response[24], response[25], response[26]};
                                byte[] gamingMachineTransferLimit = new byte[] { response[27], response[28], response[29], response[30], response[31]};
                                byte[] restrictedExpiration = new byte[] {response[32], response[33], response[34], response[35]};
                                byte[] restrictedPoolId = new byte[] {response[36], response[37]};

                                AFTLockAndStatusRequestGamingMachineResponse(assetNumber, 
                                                                             gameLockStatus, 
                                                                             availableTransfers, 
                                                                             hostCashoutStatus, 
                                                                             AFTStatus, 
                                                                             maxBufferIndex, 
                                                                             currentCashableAmount, 
                                                                             currentRestrictedAmount, 
                                                                             currentNonRestrictedAmount, 
                                                                             gamingMachineTransferLimit, 
                                                                             restrictedExpiration, 
                                                                             restrictedPoolId,
                                                                             e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 7B */
                    case 0x7B:
                        {
                             /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            try
                            {
                                byte length = response[2];
                                byte[] assetNumber = new byte[] { response[3], response[4], response[5], response[6] };
                                byte[] statusBits = new byte[] { response[7], response[8] };
                                byte[] cashableTicketAndReceiptExpiration = new byte[] { response[9], response[10] };
                                byte[] restrictedTicketDefaultExpiration = new byte[] { response[11], response[12] };
                                
                                ExtendedValidationStatusResponse(assetNumber, statusBits, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 7E */
                    case 0x7E:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            try
                            {
                                byte[] date = new byte[] { response[2], response[3], response[4], response[5] };
                                byte[] time = new byte[] { response[6], response[7], response[8] };
                                SendDateTimeGamingMachineResponse(date, time, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    // Response from 95 */
                    case 0x95:
                        {
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] meter = new byte[] { response[4], response[5], response[6], response[7] };
                                SendTournamentGamesPlayed(gameNumber, meter, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    // Response from 96 */
                    case 0x96:
                        {
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] meter = new byte[] { response[4], response[5], response[6], response[7] };
                                SendTournamentGamesWon(gameNumber, meter, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    // Response from 97 */
                    case 0x97:
                        {
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] meter = new byte[] { response[4], response[5], response[6], response[7] };
                                SendTournamentCreditsWagered(gameNumber, meter, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    // Response from 98 */
                    case 0x98:
                        {
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] meter = new byte[] { response[4], response[5], response[6], response[7] };
                                SendTournamentCreditsWon(gameNumber, meter, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 99 */
                    case 0x99:
                        {
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] gamesplayed = new byte[] { response[4], response[5], response[6], response[7] };
                                byte[] gameswon = new byte[] { response[8], response[9], response[10], response[11] };
                                byte[] creditswagered = new byte[] { response[12], response[13], response[14], response[15] };
                                byte[] creditswon = new byte[] { response[16], response[17], response[18], response[19] };
                                SendTournamentMeters(gameNumber, gamesplayed, gameswon, creditswagered, creditswon, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from 9A */
                    case 0x9A:
                        {
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] deductible = new byte[] { response[4], response[5], response[6], response[7] };
                                MeterUpdated(new Meter { meterName = "BonusingDeductible", value = int.Parse(BitConverter.ToString(deductible).Replace("-", "")), gameNumber = gameNumber }, e);
                                byte[] nondeductible = new byte[] { response[8], response[9], response[10], response[11] };
                                MeterUpdated(new Meter { meterName = "BonusingNoDeductible", value = int.Parse(BitConverter.ToString(nondeductible).Replace("-", "")), gameNumber = gameNumber }, e);
                                byte[] wagerwatch = new byte[] { response[12], response[13], response[14], response[15] };
                                MeterUpdated(new Meter { meterName = "BonusingWagerMatch", value = int.Parse(BitConverter.ToString(wagerwatch).Replace("-", "")), gameNumber = gameNumber }, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from A0 */
                    case 0xA0:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte feat1 = response[4];
                                byte feat2 = response[5];
                                byte feat3 = response[6];
                                SendEnabledFeaturesResponse(gameNumber, feat1, feat2, feat3, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from A4 */
                    case 0xA4:
                        {
                            /*Parseo cada parte de la response y envío */
                            /* Parsing each part of the response and sending it */
                            try
                            {
                                byte[] gameNumber = new byte[] { response[2], response[3] };
                                byte[] cashoutLimit = new byte[] { response[4], response[5] };
                                CashoutLimitReceived(gameNumber, cashoutLimit, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from B1 */
                    case 0xB1:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte currentPlayerDenomination = response[2];
                                SendCurrentPlayerDenomination(currentPlayerDenomination, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from B2 */
                    case 0xB2:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte length = response[2];
                                byte numberOfDenominations = response[3];
                                List<byte> denominations = new List<byte>();
                                for (int i = 0; i < length - 1; i++)
                                {
                                    denominations.Add(response[4 + i]);
                                }
                                byte[] playerDenominations = denominations.ToArray();
                                SendEnabledPlayerDenominations(numberOfDenominations, playerDenominations, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from B3 */
                    case 0xB3:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte tokenDenomination = response[2];
                                SendTokenDenomination(tokenDenomination, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                     /* Response from B4 */
                    case 0xB4:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                               byte[] gameNumber = new byte[] { response[3], response[4] };
                               byte[] wagerCategory = new byte[] { response[5], response[6] };
                               byte[] paybackPercentage = new byte[] { response[7], response[8], response[9], response[10] };
                               byte coinInMeterSize = response[11];
                               List<byte> digits = new List<byte>();
                               for(int i = 0; i < coinInMeterSize; i++)
                               {
                                    digits.Add(response[12+i]);
                               }
                               byte[] coinInMeter = digits.ToArray();
                                SendWagerCategoryInformationResponse(gameNumber, wagerCategory, paybackPercentage, coinInMeter, e);
                            }
                            catch
                            {

                            }
                            break;
                        }
                    /* Response from B5 */
                    case 0xB5:
                        {
                            try
                            {
                                /*Parseo cada parte de la response y envío */
                                /* Parsing each part of the response and sending it */
                                byte length = response[2];
                                byte[] gameNumber = new byte[] { response[3], response[4] };
                                byte[] maxBet = new byte[] { response[5], response[6] };
                                byte progressiveGroup = response[7];
                                byte[] progressiveLevels = new byte[] { response[8], response[9], response[10], response[11] };
                                byte gameNameLength = response[12];
                                byte[] gameName = new byte[gameNameLength];
                                for (int i = 0; i < (int)gameNameLength; i++)
                                {
                                    gameName[i] = GetByteFromArrayIndex(response, 13 + i).Value;
                                }
                                byte paytableNameLength = response[13 + (int)gameNameLength];
                                byte[] paytableName = new byte[paytableNameLength];
                                for (int i = 0; i < (int)paytableNameLength; i++)
                                {
                                    paytableName[i] = GetByteFromArrayIndex(response, 14 + (int)gameNameLength + i).Value;
                                }
                                byte[] wagerCategories = new byte[] { response[14 + (int)gameNameLength + (int)paytableNameLength], response[14 + (int)gameNameLength + (int)paytableNameLength + 1] };
                                SendGameNExtendedInformationResponse(gameNumber,
                                                                     maxBet,
                                                                     progressiveGroup,
                                                                     progressiveLevels,
                                                                     gameNameLength,
                                                                     gameName,
                                                                     paytableNameLength,
                                                                     paytableName,
                                                                     wagerCategories,
                                                                     e);

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
            return 0;
        }

    }
}
