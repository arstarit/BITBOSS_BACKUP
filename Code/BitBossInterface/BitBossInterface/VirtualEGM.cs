using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SASComms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BitbossInterface
{
    /// <summary>
    /// The VirtualEGM. This has several components
    /// </summary>
    public class VirtualEGM
    {
        // The Virtual EGM has several components

        // The EGM Accounting
        private EGMAccounting _EGMAccounting;
        // The EGM Status
        private EGMStatus _EGMStatus;
        // The Settings of EGM
        private EGMSettings _EGMSettings;
        // EGM Information
        private EGMInfo _EGMInfo;
        // The associated SAS Client
        private SASClient _client;
        // A flag that indicates that handpay is in process
        private bool HandpayInProcess = false;
        // A flag that indicates the transfers are enabled
        public bool TransfersDisabled = true;
        // A flag that indicates if a transfer is in progress
        public bool TransferInProgress = false;
        // La  current ticket redemption
        public TicketRedemption current_ticket_redemption;
        // La current ticket validation
        public TicketValidation current_ticket_validation;
        // Un timer que establece el intervalo de lanzamiento de la excepción 51
        private static System.Timers.Timer Timer51;
        // Un timer que establece el intervalo de lanzamiento de la excepción 3D
        private static System.Timers.Timer Timer3D;
        // Un timer que establece el intervalo de lanzamiento de la excepción 3E
        private static System.Timers.Timer Timer3E;
        // Un timer que establece el intervalo de lanzamiento de la excepción 67
        private static System.Timers.Timer Timer67;
        // Un timer que establece el intervalo de lanzamiento de la excepción 68
        private static System.Timers.Timer Timer68;
        // Un timer que establece el intervalo de lanzamiento de la excepción 69
        private static System.Timers.Timer Timer69;
        // Una bandera que indica que el último 51 fué enviado y confirmado (Aún no se envió el próximo 51)
        private bool Current51IssuedAndAcknowledged = true;
        // Eventos que la VirtualEGM lanza
        // Un evento que se lanza cuando se envía un long poll
        public event CommandSentHandler CommandSent; public delegate void CommandSentHandler(string cmd, bool crc, bool retry, EventArgs e);
        // Un evento que se lanza cuando se recibe un long poll
        public event CommandReceivedHandler CommandReceived; public delegate void CommandReceivedHandler(string cmd, bool crc, EventArgs e);
        // Un evento que se lanza cuando no hay conexión con la Smib
        public event SmibLinkDownHandler SmibLinkDown; public delegate void SmibLinkDownHandler(bool truth, EventArgs e);
        // Un evento que se lanza cuando se quiere enviar un log
        public event LaunchLogHandler LaunchLog; public delegate void LaunchLogHandler(string[] tags, string message, EventArgs e);

        public event LP80Handler LP80; public delegate void LP80Handler(bool broadcast, byte group, byte level, byte[] amount, EventArgs e);
        public event LP86Handler LP86; public delegate void LP86Handler(bool broadcast, byte group, List<Tuple<byte, byte[]>> amountsandlevels, EventArgs e);
        public event LP7CHandler LP7C; public delegate void LP7CHandler(byte code, string data, EventArgs e);
        public event LP7DHandler LP7D; public delegate void LP7DHandler(byte[] HostId, byte expiration, byte[] location, byte[] address1, byte[] address2, EventArgs e);
        public event LP71Handler LP71; public delegate void LP71Handler(byte transferCode, int amount, byte parsingCode, byte[] validationData, EventArgs e);
        public event LP58Handler LP58; public delegate void LP58Handler(byte validationSystemID, byte[] validationNumber, EventArgs e);
        public event LP08Handler LP08; public delegate void LP08Handler(byte[] billDenominations, byte billAcceptorFlag, EventArgs e);
        public event LP4CHandler LP4C; public delegate void LP4CHandler(byte[] machineID, byte[] sequenceNumber, EventArgs e);
        public event LP7FHandler LP7F; public delegate void LP7FHandler(byte[] date, byte[] time, EventArgs e);
        public event LPA8Handler LPA8; public delegate void LPA8Handler(byte resetMethod, EventArgs e);
        public event LP0EHandler LP0E; public delegate void LP0EHandler(byte enable_disable, EventArgs e);
        public event LP2EHandler LP2E; public delegate void LP2EHandler(byte[] bufferAmount, EventArgs e);
        public event LP01Handler LP01; public delegate void LP01Handler(EventArgs e);
        public event LP02Handler LP02; public delegate void LP02Handler(EventArgs e);
        public event LP03Handler LP03; public delegate void LP03Handler(EventArgs e);
        public event LP04Handler LP04; public delegate void LP04Handler(EventArgs e);
        public event LP06Handler LP06; public delegate void LP06Handler(EventArgs e);
        public event LP07Handler LP07; public delegate void LP07Handler(EventArgs e);
        public event LP94Handler LP94; public delegate void LP94Handler(EventArgs e);
        public event LP8AHandler LP8A; public delegate void LP8AHandler(byte[] bonusAmount, byte taxStatus, EventArgs e);
        public event LP72Handler LP72; public delegate void LP72Handler(byte transferCode, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte[] transactionID, byte[] registrationKey, byte[] expiration, EventArgs e);
        public event LP73Handler LP73; public delegate void LP73Handler(byte registrationCode, byte[] registrationKey, byte[] posId, EventArgs e);
        public event LP74Handler LP74; public delegate void LP74Handler(byte lockCode, byte lockCondition, byte[] lockTimeout, EventArgs e);
        public event LP72InterrogationHandler LP72Interrogation; public delegate void LP72InterrogationHandler(EventArgs e);
        public event LP4DHandler LP4D; public delegate void LP4DHandler(byte functionCode, EventArgs e);
        public event LP21Handler LP21; public delegate void LP21Handler(byte[] seedValue, EventArgs e);

        ////
        public static event LP4DResponseSuccessfulHandler LP4DResponseSuccessful; public delegate void LP4DResponseSuccessfulHandler(EventArgs e);


        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /***********************************************  PRIVATE METHODS **************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/


        #region PRIVATEMETHODS

        #region Auxiliar

        private GameAccounting HandleGetGameAccounting(string longpoll, byte[] gameNumber)
        {
            // Retrieve the GameAccounting object by calling the GetGameAccounting method of the _EGMAccounting object with the specified gameNumber.
            GameAccounting gameAccounting = _EGMAccounting.GetGameAccounting(gameNumber);

            // Prepare the log message based on the longpoll value.
            string lpmessage = (longpoll == "" || longpoll == null) ? "" : $" from SMIB through long poll {longpoll}";

            // If the gameAccounting is null, log an error indicating that the requested game doesn't exist.
            if (gameAccounting == null)
            {
                try
                {
                    // Logging the error to the LaunchLog method, indicating the game number that doesn't exist.
                    LaunchLog(new string[] { "<- SMIB" }, $"Error trying to access game {BitConverter.ToString(gameNumber)}{lpmessage}. This game doesn't exist", new EventArgs());
                }
                catch
                {
                    // Catch any exceptions that occur during the logging process.
                }
            }

            // Return the retrieved GameAccounting object.
            return gameAccounting;
        }

        /// <summary>
        /// It is used at registering method, at lp74 processing.
        /// It is used when updating the long poll 74 response info, from EGM to VirtualEGM persistence.
        /// It is used when we setting features. for A0
        /// Given a byte and a position, obtain the bit of that position.
        /// </summary>
        /// <param name="b">Un byte</param>
        /// <param name="pos">Una posición</param>
        /// <returns>El bit de b en esa posición</returns>
        private static byte GetByteWithPos(byte b, int pos)
        {
            return (byte)(b & (1 << pos));
        }

        /// <summary>
        /// *** USED FOR HANDPAY PROCESS ***
        /// It is used by other private method, HandpayLPDistinctMethod
        /// It is used by other private method, Send1BResponseFromQueue
        /// Method that applies an action to the first element Handpay to the queue, without removing it
        /// </summary>
        /// <param name="act"></param>
        private void ApplyToPeek(Action<Handpay> act)
        {
            Handpay a_ = null;
            if (_EGMStatus.Handpayqueue.TryPeek(out a_))
            {
                act(a_);
            }
        }

        /// <summary>
        /// *** USED FOR HANDPAY PROCESS ***
        /// It is used by other private method, HandpayLPDistinctMethod
        /// It is used by a public method, called HandpayReset
        /// Method that applies an action to the first element Handpay of the queue, removing it
        /// </summary>
        /// <param name="act"></param>
        private void ApplyToDequeue(Action<Handpay> act)
        {
            Handpay a_ = null;
            if (_EGMStatus.Handpayqueue.TryDequeue(out a_))
            {
                act(a_);
            }

        }


        /// <summary>
        ///  *** USED FOR HANDPAY PROCESS ***
        /// It is used by other private method, called HandpayLPDistinctMethod
        /// It is used on StartVirtualEGM, checking the handpay queue
        /// Determines that all handpays of the queue are confirmed with ACK
        /// </summary>
        /// <returns></returns>
        private bool AllAcknoledged()
        {
            return (_EGMStatus.Handpayqueue.Where(hp => hp.LP1BAcknowledged == false).Count() == 0);
        }


        #endregion

        #region Exceptions

        /// <summary>
        /// EXCPT 3D
        /// *** PART OF VALIDATION PROCESS ***
        /// Method that sends the exception 3D. First, it enqueues if there is a validation in process
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SendException3D(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (current_ticket_validation != null)
                EnqueueException(0x3D, new byte[] { });
        }

        /// <summary>
        /// EXCPT 3E
        /// *** PART OF VALIDATION PROCESS ***
        /// Method that sends the exception 3E. First, it enqueues if there is a validation in process
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SendException3E(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (current_ticket_validation != null)
                EnqueueException(0x3E, new byte[] { });
        }


        /// <summary>
        /// EXCPT 51
        /// ****** PART OF HANDPAY PROCESS ******
        /// Method that sends the exception 51. First, it enqueues the exception
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SendException51(Object source, System.Timers.ElapsedEventArgs e)
        {
            EnqueueException(0x51, new byte[] { });
            if (Current51IssuedAndAcknowledged)
            {
                Current51IssuedAndAcknowledged = false;
            }
        }

        /// <summary>
        /// EXCPT 51 / EXCPT 51
        /// ***** PART OF HANDPAY PROCESS ***
        /// It is used when we send the 1B response from Queue. 
        /// Method that is executed when there is an ACK from host side. Sends the exceptions 51 and 52 depending on certain conditions 
        /// </summary>
        /// <param name="e"></param>
        private void HandpayLPDistinctMethod(EventArgs e)
        {
            // Detengo el timer51 // Stop the timer51
            try { Timer51.Stop(); } catch { }
            // Desuscribo este método // Unsubscribe this method
            _client.LPDistinct -= HandpayLPDistinctMethod;
            // Actualizo el primer elemento de la cola // Update the first element of the queue
            ApplyToPeek(hp =>
            {
                // Con el ACK
                hp.LP1BAcknowledged = true;
                // El 51 actual ya fué enviado y acknowledgeado // The current 51 was already sent and acknowledged
                Current51IssuedAndAcknowledged = true;
                // Si está resetado y acknowledgeado // If it is reset and acknowledged
                if (hp.status == HandpaySMStatus.HandpayReset && hp.LP1BAcknowledged)
                {
                    // Envío el 52 y remuevo el handpay de la cola // Send the 52 and remove the handpay from the queue
                    ApplyToDequeue(hp1 => { EnqueueException(0x52, new byte[] { }); });
                }
            });

            // Guardo todo // Save all
            SaveEGMStatus();

            // Si no todos los handpays están acknowledgeados // If not all handpays are acknowledged
            if (!AllAcknoledged())
            {
                // Comienzo el Timer51 // Start the Timer51
                try { Timer51.Start(); } catch { }
            }
        }

        /// <summary>
        /// EXCPT 67
        /// **** PART OF REDEMPTION PROCESS ***
        /// Method that sends the exception 67, first it enqueues if there is a redemption in process transitioning to state TicketInserted
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SendException67(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (current_ticket_redemption != null)
            {
                if (current_ticket_redemption.status == TicketRedemptionStatus.TicketInserted)
                    EnqueueException(0x67, new byte[] { });
            }
        }


        /// Sending the exception 68 as priority
        /// <summary>
        /// EXCPT 68
        /// **** PART OF REDEMPTION PROCESS ***
        /// Method that sends the exception 68, first it enqueues it if there is a redemption in process transitioning to state Sending68
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SendException68(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (current_ticket_redemption != null)
            {
                if (current_ticket_redemption.status == TicketRedemptionStatus.Sending68)
                    EnqueueException(0x68, new byte[] { });
            }
        }

        /// Sending the exception 69 as priority
        /// <summary>
        /// EXCPT 69
        /// *** PART OF AFT PROCESS **
        /// Method that sends the exception 69, first it enqueues it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SendException69(Object source, System.Timers.ElapsedEventArgs e)
        {
            // if (_EGMStatus.aftcollection != null)
            // {
            //     AFTOperation lastop = _EGMStatus.aftcollection.currentoperation;
            //     if (lastop != null)
            //     {
            //         if (lastop.AckStatus == AFTOperationStatus.OpFinished && lastop.InternalStatus == 0x00)
            //         {
            EnqueueException(0x69, new byte[] { });
            //         }
            //     }
            // }
        }

        #endregion

        #region Long Polls Auxiliar methods

        /// <summary>
        /// *** 1B Response *** *** HANDPAY ***
        /// Enqueues the 1B response versión 1, instantiating a handpay to zeroes and adding it to the queue. It is used at Handpay Pending public method
        /// </summary>
        private void Enqueue1BResponse()
        {
            Handpay hp = new Handpay();
            hp.LastTransitionTS = DateTime.Now;
            // Guardo valores por default // Save default values
            byte[] Data1B = new byte[] { _EGMSettings.GetAddress(), // 0
                                        0x00, // 1
                                        0x00, // 2
                                        0x00, 0x00, 0x00, 0x40, 0x00, // 3 4 5 6 7
                                        0x00, 0x99, // 8 9
                                        0x00 }; // 10
            hp.data = Data1B;
            // Encolo el handpay // Enqueue the handpay
            _EGMStatus.Handpayqueue.Enqueue(hp);
            SaveEGMStatus();
        }

        /// <summary>
        /// *** LP72 Response *** *** AFT ***
        /// Get the status code based on the different parameters of long poll 72. It is used by lp72 processing at registering methods / events
        /// </summary>
        /// <returns></returns>
        private byte GetTransferStatusCode(ref bool relay, byte transferCode, byte transactionIndex, byte transferType, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte tranferFlags, byte[] assetNumber, byte[] registrationKey, byte[] transactionID, byte[] expiration, byte[] poolID, byte[] receiptData, byte[] lockTimeout)
        {
            byte result = 0x88; // Not registered
            /** TransfersDisabled **/
            if (TransfersDisabled)
            {
                result = 0x87;
                relay = false;
                return result;
            }
            /** Check if there is not a current transfer in progress **/
            if (TransferInProgress)
            {
                result = 0xC0;
                relay = false;
                return result;
            }
            /** Check that AssetNumber is correct **/
            if (!_EGMSettings.assetId.SequenceEqual(assetNumber))
            {
                result = 0x93;
                relay = false;
                return result;
            }
            /** Check that registration key is correct **/
            if (transferType != 0x00 && transferType != 0x80)
            {
                if (_EGMStatus.regKey != null)
                {
                    if (!_EGMStatus.regKey.SequenceEqual(registrationKey))
                    {
                        result = 0x89;
                        relay = false;
                        return result;
                    }
                }
            }
            /** Check that amounts are well formatted **/
            try
            {
                long longcashableAmount = long.Parse(BitConverter.ToString(cashableAmount).Replace("-", ""));
                long longrestrictedAmount = long.Parse(BitConverter.ToString(restrictedAmount).Replace("-", ""));
                long lonnonRestrictedAmount = long.Parse(BitConverter.ToString(nonRestrictedAmount).Replace("-", ""));
            }
            catch
            {
                result = 0x83;
                relay = false;
                return result;
            }
            /** check that transfer type is 00 or 80**/
            if (transferType != 0x00 && transferType != 0x80)
            {
                result = 0x82;
                relay = false;
                return result;
            }


            /** If RegStatus is 01 **/
            //if (_EGMStatus.regStatus == 0x01)
            if (true)
            {
                result = 0x40;
                relay = true;
            }
            return result;
        }

        /// <summary>
        /// *** LP73 RESPONSE *** *** AFT ***
        /// Cancel Registration. It is used by processing the lp73 at registering methods / events
        /// </summary>
        private void CancelRegistration()
        {
            byte laststatus = _EGMStatus.regStatus;
            // El estado será 80 // The status will be 80
            _EGMStatus.setStatus(0x80);
            _client.SendRegistrationResponse(_EGMSettings.GetAddress(),
                                             0x73,
                                             _EGMStatus.regStatus,
                                             _EGMSettings.assetId,
                                              _EGMStatus.regKey == null ? new byte[] { } : _EGMStatus.regKey,
                                              _EGMStatus.regPOSId == null ? new byte[] { } : _EGMStatus.regPOSId);
            if (laststatus != 0x80)
            {
                //EnqueueException(0x6E, new byte[]{});
            }
        }

        /// <summary>
        /// *** LP74 RESPONSE ***
        /// BCD format to Time. It is used by long poll 74 processing, at registering methods / events
        /// </summary>
        /// <param name="bcdtime"></param>
        /// <param name="defaultdays"></param>
        /// <returns></returns>
        private static DateTime BCDToTime(byte[] bcdtime, int defaultdays)
        {
            DateTime result = DateTime.Now.AddDays(defaultdays);
            // Or set the restricted expiration 
            if (bcdtime != null)
            {
                if (bcdtime.Length == 4)
                {
                    byte month = bcdtime[0];
                    byte day = bcdtime[1];
                    byte[] year = new byte[] { bcdtime[2], bcdtime[3] };
                    if (month != 0x00)
                        result = new DateTime(int.Parse(BitConverter.ToString(year).Replace("-", "")),
                                                 int.Parse(BitConverter.ToString(new byte[] { month }).Replace("-", "")),
                                                 int.Parse(BitConverter.ToString(new byte[] { day }).Replace("-", "")));
                }
            }

            return result;
        }

        /// <summary>
        ///  *** 1B Response *** *** HANDPAY ***
        ///  Enqueues the response to 1B version 2, instantiating a Handpay with the parameters and adding it to the queue
        /// </summary>
        /// <param name="progressiveGroup"></param>
        /// <param name="level"></param>
        /// <param name="amount"></param>
        /// <param name="partialPay"></param>
        /// <param name="resetID"></param>
        private void Enqueue1BResponse(byte progressiveGroup,
                                       byte level,
                                       byte[] amount,
                                       byte[] partialPay,
                                       byte resetID)
        {
            Handpay hp = new Handpay();
            hp.LastTransitionTS = DateTime.Now;
            // Guardo los valores en la data del 1B // Save the values in the 1B data
            byte[] Data1B = new byte[] {_EGMSettings.GetAddress(), // 0
                                        progressiveGroup, // 1
                                        level, // 2
                                        amount[0], amount[1], amount[2], amount[3], amount[4], // 3 4 5 6 7
                                        partialPay[0], partialPay[1], // 8 9
                                        resetID }; // 10
            hp.data = Data1B;
            // Encolo el handpay // Enqueue the handpay
            _EGMStatus.Handpayqueue.Enqueue(hp);
            // Guardo todo // Save all
            SaveEGMStatus();
        }

        /// <summary>
        ///  *** 1B Response *** *** HANDPAY ***
        /// Método que envía la response del 1B de la cola // Method that sends the 1B response from the queue
        /// </summary>
        private void Send1BResponseFromQueue()
        {

            // Si hay algún elemento en la cola de handpay // If there is any element in the handpay queue
            if (_EGMStatus.Handpayqueue.Count() > 0)
            {
                // Si la current 51 fué enviada y acknowledgeada // If the current 51 was sent and acknowledged
                if (Current51IssuedAndAcknowledged)
                {
                    // Envío todos 0s
                    _client.SendHandpayInformation(0x00,
                                                   0x00,
                                                   0x00,
                                                   new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
                                                   new byte[] { 0x00, 0x00 },
                                                   0x00);
                }
                else // Si la current 51 no fué enviada o no fué acknowledgeada // If the current 51 was not sent or was not acknowledged
                {
                    // Send the long poll of the queue // Envío el long poll de la cola
                    ApplyToPeek(hp =>
                    {
                        _client.SendHandpayInformation(hp.data[0],
                                                       hp.data[1],
                                                       hp.data[2],
                                                       new byte[] { hp.data[3], hp.data[4], hp.data[5], hp.data[6], hp.data[7] },
                                                       new byte[] { hp.data[8], hp.data[9] },
                                                       hp.data[10]);
                        // ACKnowledged
                        _client.LPDistinct += HandpayLPDistinctMethod;

                    });
                }
            }
        }

        /// <summary>
        /// *** LP7E RESPONSE ***
        /// Given a datetime value in unix format, it returns a value of DateTime type. It is used at long poll 7E processing  at registering methods / event
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds((double)unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        #endregion


        /********************************************/
        /**************  VALIDATION  ****************/
        /********************************************/

        #region validation

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="a"></param>
        ///// <param name="b"></param>
        ///// <param name="c"></param>
        ///// <param name="pos"></param>
        ///// <returns></returns>
        //private static byte UpdateValidationStatusBit(byte a, byte b, byte c, int pos)
        //{
        //    if (GetByteWithPos(a, pos) == 0)
        //    {
        //        return GetByteWithPos(c, pos);
        //    }
        //    else
        //    {
        //        return GetByteWithPos(b, pos);
        //    }
        //}

        //

        #endregion


        /********************************************/
        /**************  HANDPAY  *******************/
        /********************************************/

        #region hanpday











        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="mask"></param>
        ///// <param name="status"></param>
        ///// <param name="current"></param>
        ///// <returns></returns>
        //private byte[] UpdateValidationStatusBits(byte[] mask, byte[] status, byte[] current)
        //{
        //    return new byte[] {
        //        (byte)(UpdateValidationStatusBit(mask[0], status[0], current[0], 7) | UpdateValidationStatusBit(mask[0], status[0], current[0], 6)  | UpdateValidationStatusBit(mask[0], status[0], current[0], 5) | UpdateValidationStatusBit(mask[0], status[0], current[0], 4) | UpdateValidationStatusBit(mask[0], status[0], current[0], 3) | UpdateValidationStatusBit(mask[0], status[0], current[0], 2) | UpdateValidationStatusBit(mask[0], status[0], current[0], 1) | UpdateValidationStatusBit(mask[0], status[0], current[0], 0) ),
        //        (byte)(UpdateValidationStatusBit(mask[1], status[1], current[1], 7) | UpdateValidationStatusBit(mask[1], status[1], current[1], 6)  | UpdateValidationStatusBit(mask[1], status[1], current[1], 5) | UpdateValidationStatusBit(mask[1], status[1], current[1], 4) | UpdateValidationStatusBit(mask[1], status[1], current[1], 3) | UpdateValidationStatusBit(mask[1], status[1], current[1], 2) | UpdateValidationStatusBit(mask[1], status[1], current[1], 1) | UpdateValidationStatusBit(mask[1], status[1], current[1], 0) )
        //    };
        //}




        #endregion


        /********************************************/
        /**************  REGISTER METHODS ************/
        /********************************************/

        #region register methods

        /// <summary>
        /// El método principal, que registra los métodos, captura los eventos del client // The main method, which registers the methods, captures the events of the client
        /// </summary>
        private void RegisterMethods()
        {
            /**********************************************************************************************************/
            /**********************************************************************************************************/
            /**********************************************************************************************************/
            /*************************************   Eventos del Client   *********************************************/
            /**********************************************************************************************************/
            /**********************************************************************************************************/
            /**********************************************************************************************************/
            // Command Sent
            _client.CommandSent += new SASClient.CommandSentHandler((cmd, crc, retry, e) =>
            {
                // Lanzo el evento CommandSent al controller // I launch the CommandSent event to the controller
                CommandSent(cmd, crc, retry, e);
            });
            // Command Received
            _client.CommandReceived += new SASClient.CommandReceivedHandler((cmd, crc, e) =>
            {
                // Lanzo el evento CommandReceived al controller // I launch the CommandReceived event to the controller
                CommandReceived(cmd, crc, e);
            });
            // Smib Link Down
            _client.SmibLinkDown += new SASClient.SmibLinkDownHandler((truth, e) =>
            {
                // Lanzo el evento SmibLinkDown con el parámetro "truth" al controller // I launch the SmibLinkDown event with the "truth" parameter to the controller
                // Si truth es verdadero, está caído, sino, está establecida la conexión con la SMIB // If truth is true, it is down, otherwise, the connection with the SMIB is established
                try { SmibLinkDown(truth, e); } catch { }
            });

            // Shutdown game Command --lp01
            _client.VirtualEGM01 += new SASClient.VirtualEGM01Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendACK();
                // Manda al controller el event LP01 // Send the event LP01 to the controller
                LP01(ee);
            });
            // Enable game Command --lp02
            _client.VirtualEGM02 += new SASClient.VirtualEGM02Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendACK();
                // Manda al controller el event LP02 // Send the event LP02 to the controller
                LP02(ee);
            });
            // Sound off command -- lp03
            _client.VirtualEGM03 += new SASClient.VirtualEGM03Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendACK();
                // Manda al controller el event LP03 // Send the event LP03 to the controller
                LP03(ee);
            });
            // Sound off command -- lp04
            _client.VirtualEGM04 += new SASClient.VirtualEGM04Handler((address, ee) =>
            {
                if (address != 0x00)
                    // Respondo por el client // I respond for the client
                    _client.SendACK();
                // Manda al controller el event LP04 // Send the event LP04 to the controller
                LP04(ee);
            });
            // Response al long poll 06
            _client.VirtualEGM06 += new SASClient.VirtualEGM06Handler((address, ee) =>
            {
                if (address != 0x00) _client.SendACK();
                // Lanzo al client el event LP06 // I launch the event LP06 to the client
                LP06(ee);
            });
            // Response al long poll 07
            _client.VirtualEGM07 += new SASClient.VirtualEGM07Handler((address, ee) =>
            {
                if (address != 0x00) _client.SendACK();
                // Lanzo al client el event LP07
                LP07(ee);
            });
            // Configure Bill Denomination from VirtualEGM -- 08
            _client.VirtualEGMConfigureBillDenomination += new SASClient.VirtualEGMConfigureBillDenominationsHandler((address, billDenominations, billAcceptorFlag, e) =>
            {
                if (address != 0x00) _client.SendACK();
                // Mando el evento LP08 al controller con el billDenominations y el billAcceptorFlag // I send the LP08 event to the controller with the billDenominations and the billAcceptorFlag
                LP08(billDenominations, billAcceptorFlag, e);
            });
            //Enable / Disable Real Time Event Reporting -- lp0E
            _client.VirtualEGM0E += new SASClient.VirtualEGM0EHandler((address, enable_disable, ee) =>
            {
                if (address != 0x00)
                    _client.SendACK();
                // Manda al controller el evento SetEnabledRealTime con el booleano para activar o desactivar // Send the SetEnabledRealTime event to the controller with the boolean to activate or deactivate
                SetEnabledRealTime(enable_disable);
                // Manda al controller el event LP0E con el booleano para activar o desactiva // Send the LP0E event to the controller with the boolean to activate or deactivate
                LP0E(enable_disable, ee);

            });
            // Response al longpoll 0F 
            _client.VirtualEGM0F += new SASClient.VirtualEGM0FHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendMultipleMeters0FResponse(_EGMSettings.GetAddress(),
                                                         _EGMAccounting.GetValueOfMeter(new byte[] { 0x04, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                         _EGMAccounting.GetValueOfMeter(new byte[] { 0x00, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                         _EGMAccounting.GetValueOfMeter(new byte[] { 0x01, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                         _EGMAccounting.GetValueOfMeter(new byte[] { 0x24, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                         _EGMAccounting.GetValueOfMeter(new byte[] { 0x02, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                         _EGMAccounting.GetValueOfMeter(new byte[] { 0x05, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false));


            });
            // Response al long poll 10
            _client.VirtualEGMSendTotalCancelledCredits += new SASClient.VirtualEGMSendTotalCancelledCreditsHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x10,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x04, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));

            });
            // Response al long poll 11
            _client.VirtualEGMSendTotalCoinInMeter += new SASClient.VirtualEGMSendTotalCoinInMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x11,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x00, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));
            });
            // Response al long poll 12
            _client.VirtualEGMSendTotalCoinOutMeter += new SASClient.VirtualEGMSendTotalCoinOutMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x12,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x01, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));
            });
            // Response al long poll 13
            _client.VirtualEGMSendTotalDropMeter += new SASClient.VirtualEGMSendTotalDropMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x13,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x24, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));
            });
            // Response al long poll 14
            _client.VirtualEGMSendTotalJackpotMeter += new SASClient.VirtualEGMSendTotalJackpotMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x14,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x02, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));
            });
            // Response al long poll 15
            _client.VirtualEGMSendGamesPlayedMeter += new SASClient.VirtualEGMSendGamesPlayedMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x15,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x05, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));
            });
            // Response al long poll 16
            _client.VirtualEGMSendGamesWonMeter += new SASClient.VirtualEGMSendGamesWonMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x16,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x06, 0x00 },
                                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                        false));
            });
            // Response al long poll 17
            _client.VirtualEGMSendGamesLostMeter += new SASClient.VirtualEGMSendGamesLostMeterHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x17,
                                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x07, 0x00 },
                                                                                                           _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                           false));
            });


            // Response al longpoll 18
            _client.VirtualEGMLP18 += new SASClient.VirtualEGMLP18Handler((address, ee) =>
            {
                byte[] last_power_reset = _EGMAccounting.GetValueOfMeter(new byte[] { 0x25, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false);
                byte[] last_door_closed = _EGMAccounting.GetValueOfMeter(new byte[] { 0x26, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false);
                if (address != 0x00)
                    _client.SendGamesPlayedSinceLast_18(_EGMSettings.GetAddress(),
                                                        new byte[] { last_power_reset[2], last_power_reset[3] },
                                                        new byte[] { last_door_closed[2], last_door_closed[3] });
            });

            // Response al longpoll 19
            _client.VirtualEGMLP19 += new SASClient.VirtualEGMLP19Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendMultipleMeters19Response(_EGMSettings.GetAddress(),
                                                        _EGMAccounting.GetValueOfMeter(new byte[] { 0x00, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false), // Total Coin In
                                                        _EGMAccounting.GetValueOfMeter(new byte[] { 0x01, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false), // Total Coin Out
                                                        _EGMAccounting.GetValueOfMeter(new byte[] { 0x24, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false), // Total Drop
                                                        _EGMAccounting.GetValueOfMeter(new byte[] { 0x02, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false), // Total Jackpot
                                                        _EGMAccounting.GetValueOfMeter(new byte[] { 0x05, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false)); // Games Played)
            });

            // Host Single Meter Long Poll -- lp1A
            _client.VirtualEGMHostSingleMeterAccounting += new SASClient.VirtualEGMHostSingleMeterAccountingHandler((address, single_meter_accounting_long_poll, ee) =>
            {
                GameAccounting gameAccounting = null;
                // En la EGMAccounting busco el accounting correspondiente al gameNumber // In the EGMAccounting I look for the accounting corresponding to the gameNumber
                gameAccounting = _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 });

                if (gameAccounting != null)
                {
                    //Devuelvo el meter 0C de la accounting // I return the meter 0C of the accounting
                    byte[] value = _EGMAccounting.GetValueOfMeter(new byte[] { 0x0C, 0x00 }, gameAccounting, false);
                    if (address != 0x00)
                        // Respondo por el client // I answer for the client
                        _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            single_meter_accounting_long_poll,
                                                                            value);
                }

            });

            // Handler for lp1B -- lp1B
            _client.VirtualEGM1B += new SASClient.VirtualEGM1BHandler((address, e) =>
            {
                if (address != 0x00)
                    Send1BResponseFromQueue();
            });

            // Send Meters from VirtualEGM -- lp1C
            _client.VirtualEGMSendMeters += new SASClient.VirtualEGMSendMetersHandler((address, e) =>
            {
                GameAccounting gameAccounting = null;
                gameAccounting = _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 });

                if (gameAccounting != null)
                {

                    // Envía los meters del 1C
                    if (address != 0x00)
                        _client.Send1CMeters(_EGMSettings.GetAddress(),
                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x00, 0x00 }, gameAccounting, false), // Total Coin In
                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x01, 0x00 }, gameAccounting, false), // Total Coin Out
                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x24, 0x00 }, gameAccounting, false), // Total Drop
                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x02, 0x00 }, gameAccounting, false), // Total Jackpot
                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x05, 0x00 }, gameAccounting, false), // Games Played
                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x06, 0x00 }, gameAccounting, false), // Games Won 
                                _EGMAccounting.GetValueOfMeter("SlotDoorOpen", gameAccounting), // Slot Doot Opened
                                _EGMAccounting.GetValueOfMeter("PowerReset", gameAccounting)); // Power reset
                }
            });

            // Response al long poll 1D
            _client.VirtualEGMSendCumulativeMeters += new SASClient.VirtualEGMSendCumulativeMetersHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendCumulativeMeters(_EGMSettings.GetAddress(),
                                                 new byte[] { 0x00, 0x00, 0x00, 0x00 },
                                                 new byte[] { 0x00, 0x00, 0x00, 0x00 },
                                                 new byte[] { 0x00, 0x00, 0x00, 0x00 },
                                                 new byte[] { 0x00, 0x00, 0x00, 0x00 });

            });


            // Response al longpoll 1E
            _client.VirtualEGM1E += new SASClient.VirtualEGM1EHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.MultipleMetersGamingMachineLP1E(_EGMSettings.GetAddress(),
                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x40, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x42, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x43, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x44, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x46, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false),
                                                            _EGMAccounting.GetValueOfMeter(new byte[] { 0x47, 0x00 }, _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false));
            });

            // Send Gaming Machine -- lp1F

            _client.VirtualEGM1F += new SASClient.VirtualEGM1FHandler((address, e) =>
            {
                Encoding encoding = Encoding.Default;
                if (address != 0x00)
                    _client.SendGamingMachineIDandInformationLongPoll(_EGMSettings.GetAddress(),
                                                                     _EGMInfo.GameID == null ? new byte[] { 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.GameID),
                                                                     _EGMInfo.AdditionalID == null ? new byte[] { 0x00, 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.AdditionalID),
                                                                     _EGMInfo.Denomination,
                                                                     _EGMSettings.MaxBet,
                                                                     _EGMSettings.ProgressiveGroup,
                                                                     _EGMSettings.GameOptions == null ? new byte[] { 0x00, 0x00 } : _EGMSettings.GameOptions,
                                                                     _EGMInfo.PayTableID == null ? new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.PayTableID),
                                                                     _EGMInfo.BasePercentage == null ? new byte[] { 0x00, 0x00, 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.BasePercentage));

            });

            // Response al long poll 20
            _client.VirtualEGMLP20 += new SASClient.VirtualEGMLP20Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendDollarValueForBillsMeter(_EGMSettings.GetAddress(),
                                                        _EGMAccounting.GetValueOfMeter("TotalBillsInDollars",
                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 })));

            });


            // Response al long poll 27
            _client.VirtualEGMLP27 += new SASClient.VirtualEGMLP27Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendCurrentPromotionalCredits(_EGMSettings.GetAddress(),
                                                          new byte[] { 0x00, 0x00, 0x00, 0x00 });
            });

            // Response al long poll 28
            _client.VirtualEGMLP28 += new SASClient.VirtualEGMLP28Handler((address, ee) =>
            {
                byte[] result = new byte[] { };
                // En este momento, me creo 5 transfer logs en 0. // In this moment, I create 5 transfer logs in 0.
                List<EGMTransferLog> logs = new List<EGMTransferLog>();
                logs.Add(new EGMTransferLog());
                logs.Add(new EGMTransferLog());
                logs.Add(new EGMTransferLog());
                logs.Add(new EGMTransferLog());
                logs.Add(new EGMTransferLog());

                // Para esos 5 logs, los concateno en un resultado para poder mandarlos como respuesta // For those 5 logs, I concatenate them in a result to send them as response
                foreach (EGMTransferLog log in logs)
                {
                    result = join(result,
                                  new byte[] { log.commandID },
                                  new byte[] { log.transactionID },
                                  new byte[] { log.ackFlag },
                                  new byte[] { log.machineStatus },
                                  log.amount);
                }
                if (address != 0x00)
                    _client.SendTransferLog(_EGMSettings.GetAddress(),
                                            result);
            });


            // Response al long poll 2A
            _client.VirtualEGM2A += new SASClient.VirtualEGM2AHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x2A,
                                                                            _EGMAccounting.GetValueOfMeter("TrueCoinIn",
                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 })));
            });
            // Response al long poll 2B
            _client.VirtualEGM2B += new SASClient.VirtualEGM2BHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                            0x2B,
                                                                            _EGMAccounting.GetValueOfMeter("TrueCoinOut",
                                                                                        _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 })));
            });

            // Send handpay cancelled credits from VirtualEGM --lp2D
            _client.VirtualEGMSendHandPaidCancelledCredits += new SASClient.VirtualEGMSendHandPaidCancelledCreditsHandler((address, gameNumber, ee) =>
            {
                GameAccounting gmaccounting = HandleGetGameAccounting("2D", gameNumber);
                if (address != 0x00)
                    _client.GetResponseSendHandPaidCancelledCredits(_EGMSettings.GetAddress(),
                                                                     gameNumber,
                                                                     _EGMAccounting.GetValueOfMeter(new byte[] { 0x03, 0x00 }, gmaccounting, false));
            });

            // Response al long poll 2E
            _client.VirtualEGMLP2E += new SASClient.VirtualEGMLP2EHandler((address, bufferAmount, ee) =>
            {
                if (address != 0x00) _client.SendACK();
                // Lanzo al client el event LP2E con el parámetro bufferAmount // I launch to the client the event LP2E with the parameter bufferAmount
                LP2E(bufferAmount, ee);
            });

            // Send selected meters from Virtual EGM -- lp2F
            _client.VirtualEGMSendSelectedMeters += new SASClient.VirtualEGMSendSelectedMetersHandler((address, gameNumber, meters, e) =>
            {

                GameAccounting gameAccounting = null;
                // En la EGMAccounting busco el accounting correspondiente al gameNumber // In the EGMAccounting I look for the accounting corresponding to the gameNumber
                gameAccounting = HandleGetGameAccounting("2F", gameNumber);
                if (gameAccounting != null)
                {
                    byte[] result = new byte[] { };

                    // Concateno la serialización de MeterCode + Size + Value al array de bytes *meter* de los metercodes restantes solicitados // I concatenate the serialization of MeterCode + Size + Value to the array of bytes *meter* of the remaining metercodes requested
                    foreach (byte b in meters)
                    {
                        result = result.ToList().Concat(new byte[] { b }).ToArray();
                        result = result.ToList().Concat(_EGMAccounting.GetValueOfMeter(new byte[] { b, 0x00 }, gameAccounting, false).ToList()).ToArray();
                    }
                    if (address != 0x00)
                        _client.SendSelectedMeters(_EGMSettings.GetAddress(),
                                                gameNumber,
                                                result);
                }

            });

            // Response a los long polls de bill -- lp31 TO lp44
            _client.BillMeterRequested += new SASClient.BillMeterRequestedHandler((address, bill, ee) =>
            {
                if (address != 0x00)
                {
                    switch (bill)
                    {
                        case 1:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x31,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x40, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 2:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x32,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x41, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 5:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x33,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x42, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 10:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x34,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x43, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 20:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x35,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x44, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 25:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x3B,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x45, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 50:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x36,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x46, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 100:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x37,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x47, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 200:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x3A,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x48, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 500:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x38,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x4A, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 1000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x39,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x4B, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 2000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x3C,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x4C, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 2500:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x3E,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x4D, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 5000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x3F,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x4E, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 10000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x40,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x4F, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 20000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x41,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x50, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 25000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x42,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x51, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 50000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x43,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x52, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        case 100000:
                            {
                                _client.SendGamingMachineSingleMeterAccountingResponse(_EGMSettings.GetAddress(),
                                                                                0x44,
                                                                                _EGMAccounting.GetValueOfMeter(new byte[] { 0x53, 0x00 },
                                                                                                            _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }),
                                                                                                            false));
                                break;
                            }
                        default:
                            break;
                    }
                }
            });

            // Response al long poll 48
            _client.VirtualEGMLP48 += new SASClient.VirtualEGMLP48Handler((address, ee) =>
            {
                byte[] lastbillinformation = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                if (_EGMStatus.LastBillInformation != null)
                    lastbillinformation = _EGMStatus.LastBillInformation;

                if (address != 0x00)
                    _client.GetResponseSendLastBillAcceptedInformation(_EGMSettings.GetAddress(),
                                                                       lastbillinformation[0],
                                                                       lastbillinformation[1],
                                                                       new byte[] { lastbillinformation[2], lastbillinformation[3], lastbillinformation[4], lastbillinformation[5] });
            });

            // Set Secure Enhanced Validation ID (Waiting two parameters: machineID, sequenceNumber) -- lp4C
            _client.VirtualEGMSetSecureEnhancedValidationID += new SASClient.VirtualEGMSetSecureEnhancedValidationIDHandler((address, machineID, sequenceNumber, e) =>
            {
                if (address != 0x00)
                    _client.SetSecureEnhancedValidationIDResponse(_EGMSettings.GetAddress(),
                                                                   machineID,
                                                                   sequenceNumber);
                // Mando el evento LP4C al controller con el machineID y el sequenceNumber // I Send the LP4C event to the controller with the machineID and the sequenceNumber
                LP4C(machineID, sequenceNumber, e);
            });

            _client.VirtualEGM21 +=  new SASClient.VirtualEGM21Handler((address, seedValue, e) => 
            {
                if (address != 0x00)
                {
                    Console.WriteLine($"LP21 received in VirtualEGM, send ACK and emit LP21 event");
                    _client.SendACK();
                    LP21(seedValue, e);
                }
            });

            // Send Enhanced Validation Information from VirtualEGM -- lp4D
            _client.VirtualEGM4D += new SASClient.VirtualEGM4DHandler((address, functionCode, e) =>
            {
                // Chequeo que la current validation esté en proceso, y si la functionCode es 0x00 o 0xFF // I check that the current validation is in process, and if the functionCode is 0x00 or 0xFF
                if (current_ticket_validation != null && (functionCode == 0x00 || functionCode == 0xFF))
                {
                    if (address != 0x00)
                        // Respondo por el client // I respond for the client
                        _client.SendEnhancedValidationInformation(_EGMSettings.GetAddress(),
                                                                current_ticket_validation.validationType,
                                                                current_ticket_validation.indexNumber,
                                                                current_ticket_validation.date,
                                                                current_ticket_validation.time,
                                                                current_ticket_validation.validationNumber,
                                                                current_ticket_validation.amount,
                                                                current_ticket_validation.ticketNumber,
                                                                current_ticket_validation.validationSystemID,
                                                                current_ticket_validation.expiration,
                                                                current_ticket_validation.poolId);
                }
                // Si la current validation no está en proceso // 
                // O si la functionCode no es 0x00 ni 0xFF
                // If the current validation is not in process or if the functionCode is not 0x00 or 0xFF
                else
                {
                    if (address != 0x00)
                        // Manda al controller el event LP4D con todos los datos que me vienen del client // I send the LP4D event to the controller with all the data that comes to me from the client
                        LP4D(functionCode, e);

                }
                // Si la functionCode es 0x00 // If the functionCode is 0x00
                if (functionCode == 0x00)
                {
                    // Si puedo transicionar a ReceivedLP4D // If I can transition to ReceivedLP4D
                    if (current_ticket_validation.Transition(TicketValidationStatus.ReceivedLP4D))  // Transiciono a ReceivedLP4D // 
                    {
                        // Si puedo transicionar a Completed // If I can transition to Completed
                        if (current_ticket_validation.Transition(TicketValidationStatus.Completed)) // Transiciono a Completed // I transition to Completed
                        {
                            // Manda al controller el event LP4DResponseSuccesful // I send the LP4DResponseSuccesful event to the controller
                            LP4DResponseSuccessful(e);
                            // Detengo el timer de las exceptions 3D y 3E // I stop the timer of the exceptions 3D and 3E
                            Timer3D.Stop();
                            Timer3E.Stop();
                            // Reseteo la current validation // I reset the current validation
                            current_ticket_validation = null;
                        }
                    }
                }

            });

            // Send Validation Meters from VirtualEGM -- lp50
            _client.VirtualEGM50 += new SASClient.VirtualEGM50Handler((address, validationType, ee) =>
            {
                // Declaración de función que manda al client cierta data en función de la validationtype // Declaration of function that sends the client certain data depending on the validationtype
                Action<byte, int, int> SendValidationMeterIf = ((validationType_, quantity, cents) =>
                {
                    if (validationType == validationType_)
                        if (address != 0x00)
                            _client.SendValidationMetersResponse(_EGMSettings.GetAddress(),
                                                                validationType_,
                                                                quantity,
                                                                cents);
                });
                GameAccounting gameAccounting = null;
                // En la EGMAccounting busco el accounting correspondiente al gameNumber // In the EGMAccounting I look for the accounting corresponding to the gameNumber
                gameAccounting = _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 });

                if (gameAccounting != null)
                {
                    SendValidationMeterIf(0x00, gameAccounting.Meter_0087.Value, gameAccounting.Meter_0086.Value);
                    SendValidationMeterIf(0x01, gameAccounting.Meter_0089.Value, gameAccounting.Meter_0088.Value);
                    SendValidationMeterIf(0x04, gameAccounting.Meter_008B.Value, gameAccounting.Meter_008A.Value);
                    SendValidationMeterIf(0x10, gameAccounting.Meter_008D.Value, gameAccounting.Meter_008C.Value);
                    SendValidationMeterIf(0x20, gameAccounting.Meter_008F.Value, gameAccounting.Meter_008E.Value);
                    SendValidationMeterIf(0x40, gameAccounting.Meter_0091.Value, gameAccounting.Meter_0090.Value);
                    SendValidationMeterIf(0x60, gameAccounting.Meter_0093.Value, gameAccounting.Meter_0092.Value);
                    SendValidationMeterIf(0x80, gameAccounting.Meter_0081.Value, gameAccounting.Meter_0080.Value);
                    SendValidationMeterIf(0x81, gameAccounting.Meter_0083.Value, gameAccounting.Meter_0082.Value);
                    SendValidationMeterIf(0x82, gameAccounting.Meter_0085.Value, gameAccounting.Meter_0084.Value);
                }

            });

            // Send Number of Games Implemented from VirtualEGM -- lp51
            _client.VirtualEGMSendNumberOfGamesImplemented += new SASClient.VirtualEGMSendNumberOfGamesImplementedHandler((address, e) =>
            {
                if (address != 0x00)
                    // Respondo por el client // I respond for the client
                    _client.SendNumberOfGamesImplemented(_EGMSettings.GetAddress(),
                                                        (uint)_EGMInfo.NumberOfGamesImplemented);
            });

            // Response al long poll 53
            _client.VirtualEGMSendGameNConfiguration += new SASClient.VirtualEGMSendGameNConfigurationHandler((address, gn, ee) =>
            {
                Encoding encoding = Encoding.Default;
                if (address != 0x00)
                    _client.SendGameNConfiguration(_EGMSettings.GetAddress(),
                                                    gn,
                                                _EGMInfo.GameID == null ? new byte[] { 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.GameID),
                                                _EGMInfo.AdditionalID == null ? new byte[] { 0x00, 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.AdditionalID),
                                                _EGMInfo.Denomination,
                                                _EGMSettings.MaxBet,
                                                _EGMSettings.ProgressiveGroup,
                                                _EGMSettings.GameOptions == null ? new byte[] { 0x00, 0x00 } : _EGMSettings.GameOptions,
                                                _EGMInfo.PayTableID == null ? new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.PayTableID),
                                                _EGMInfo.BasePercentage == null ? new byte[] { 0x00, 0x00, 0x00, 0x00 } : encoding.GetBytes(_EGMInfo.BasePercentage));
            });

            // Send SASVersionID And Game Serial Number -- lp54
            _client.VirtualEGMSendSASVersionIDAndGameSerialNumber += new SASClient.VirtualEGMSendSASVersionIDAndGameSerialNumberHandler((address, e) =>
            {
                Encoding encoding = Encoding.Default;
                if (address != 0x00)
                    // Respondo por el client // I respond for the client
                    _client.SendSASVersionIDAndGameSerialNumber(_EGMSettings.GetAddress(),
                                                                _EGMInfo.SASVersion,
                                                                _EGMInfo.GMSerialNumber);
            });

            // Response al long poll 55
            _client.VirtualEGMSendSelectedGame += new SASClient.VirtualEGMSendSelectedGameHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendSelectedGameNumber(_EGMSettings.GetAddress(),
                                                _EGMStatus.CurrentGameNumber);
            });

            // Response al long poll 56
            _client.VirtualEGMSendEnabledGameNumbers += new SASClient.VirtualEGMSendEnabledGameNumbersHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendEnabledGameNumbers(_EGMSettings.GetAddress(),
                                                _EGMSettings.EnabledGameNumbers);
            });

            // Response Pending Cashout Information from VirtualEGM -- lp57
            _client.VirtualEGMResponsePendingCashoutInformation += new SASClient.VirtualEGMResponsePendingCashoutInformationHandler((address, e) =>
            {
                // Chequeo que la current validation esté en proceso // I check that the current validation is in process
                if (current_ticket_validation != null)
                {
                    // Si puedo transicionar a ReceivedLP57 // If I can transition to ReceivedLP57
                    if (current_ticket_validation.Transition(TicketValidationStatus.ReceivedLP57)) // Transiciono a ReceivedLP57 // I transition to ReceivedLP57
                    {
                        // Si puedo transicionar a ReceivedToLP57 // If I can transition to ReceivedToLP57
                        if (current_ticket_validation.Transition(TicketValidationStatus.ResponseToLP57)) // Transiciono a ReceivedToLP57 // I transition to ReceivedToLP57
                        {
                            if (address != 0x00)
                                // Respondo por el client // I respond for the client
                                _client.PendingCashoutInformationResponse(_EGMSettings.GetAddress(),
                                                                        current_ticket_validation.cashoutType,
                                                                        current_ticket_validation.amount);
                        }
                    }
                }
            });

            // Receive Validation Number from Host   -- lp58 
            _client.VirtualEGMReceiveValidationNumber += new SASClient.VirtualEGMReceiveValidationNumberHandler((address, validationCode, validationNumber, e) =>
            {
                // Chequeo que la current validation esté en proceso // I check that the current validation is in process
                if (current_ticket_validation != null)
                {
                    // Si puedo transicionar a ReceivedLP58 // If I can transition to ReceivedLP58 
                    if (current_ticket_validation.Transition(TicketValidationStatus.ReceivedLP58))  // Transiciono a ReceivedLP58 // I transition to ReceivedLP58
                    {
                        // Si puedo transicionar a ResponseToLP58 // If I can transition to ResponseToLP58
                        if (current_ticket_validation.Transition(TicketValidationStatus.ResponseToLP58))  // Transiciono a ResponseToLP58 // I transition to ResponseToLP58
                        {
                            if (address != 0x00)
                                // Respondo por el client // I respond for the client
                                _client.ReceiveValidationNumberResponse(_EGMSettings.GetAddress(),
                                                                        0x00);
                            // Manda al controller el event LP58 con todos los datos que me vienen del client //  Send to the controller the LP58 event with all the data coming from the client.                                          
                            LP58(validationCode, validationNumber, e);
                        }
                    }
                }
            });

            // Getting Virtual EGM Extended Meters -- lp6F // lpAF
            _client.VirtualEGMSendExtendedMeters += new SASClient.VirtualEGMSendExtendedMetersHandler((address,
                                                                                                       command,
                                                                                                       gameNumber,
                                                                                                       requestedMeterCode,
                                                                                                       additionalMeterCodes,
                                                                                                       e) =>
            {

                byte[] meters = new byte[] { };

                GameAccounting gameAccounting = null;
                gameAccounting = HandleGetGameAccounting(BitConverter.ToString(new byte[] { command }), gameNumber);

                if (gameAccounting != null)
                {
                    // Concateno la serialización de MeterCode + Size + Value al array de bytes *meter* // I concatenate the serialization of MeterCode + Size + Value to the array of bytes *meter*
                    meters = meters.ToList().Concat(_EGMAccounting.GetValueOfMeter(requestedMeterCode, gameAccounting, true).ToList()).ToArray();

                    // Concateno la serialización de MeterCode + Size + Value al array de bytes *meter* de los metercodes restantes solicitados // I concatenate the serialization of MeterCode + Size + Value to the array of bytes *meter* of the remaining metercodes requested
                    foreach (byte[] b in additionalMeterCodes)
                    {
                        meters = meters.ToList().Concat(_EGMAccounting.GetValueOfMeter(b, gameAccounting, true).ToList()).ToArray();
                    }

                    if (address != 0x00)
                        _client.SendExtendedMetersResponse(command,
                                                        _EGMSettings.GetAddress(),
                                                        gameNumber,
                                                        meters);
                }

            });

            // Send Ticket Validation Data -- lp70
            _client.VirtualEGMSendTicketValidationData += new SASClient.VirtualEGMSendTicketValidationDataHandler((address, e) =>
            {
                // Chequeo que la current redemption esté en proceso // I check that the current redemption is in process
                if (current_ticket_redemption != null)
                {
                    // Si puedo transicionar a Received70 // If I can transition to Received70
                    if (current_ticket_redemption.Transition(TicketRedemptionStatus.Received70)) // Transiciono a Received70 // Transition to Received70
                    {
                        // Detengo el timer que envía exceptions 67 // I stop the timer that sends exceptions 67
                        Timer67.Stop();
                        if (address != 0x00)
                            // Respondo por el client // I respond for the client
                            _client.SendTicketValidationData(_EGMSettings.GetAddress(),
                                                            current_ticket_redemption.machineStatus,
                                                            current_ticket_redemption.amount,
                                                            current_ticket_redemption.parsingCode,
                                                            current_ticket_redemption.validationData);
                        // Transiciono a WaitingAcceptance // I transition to WaitingAcceptance
                        current_ticket_redemption.Transition(TicketRedemptionStatus.WaitingAcceptance);
                    }
                }
            });

            // Redeem ticket from VirtualEGM (Waiting four parameters) -- lp71
            _client.VirtualEGMRedeemTicket4 += new SASClient.VirtualEGMRedeemTicket4Handler((address, transferCode, transferAmount, parsingCode, validationData, ee) =>
            {
                // Chequeo que la current redemption esté en proceso // I check that the current redemption is in process // I check that the current redemption is in process
                if (current_ticket_redemption != null)
                {
                    // Si la transferCode es 0xFF // If the transferCode is 0xFF // If the transferCode is 0xFF
                    if (transferCode == 0xFF)
                    {
                        // Es la última parte de la redemption, donde se consulta el estado de la redemption // It is the last part of the redemption, where the status of the redemption is consulted

                        // Si la current redemption la puedo transicionar a AckByHost // If the current redemption can be transitioned to AckByHost
                        if (current_ticket_redemption.Transition(TicketRedemptionStatus.AckByHost)) // Transiciono a AckByHost // I transition to AckByHost
                        {
                            // Detengo el timer que envía exceptions 68 // I stop the timer that sends exceptions 68
                            Timer68.Stop();
                            if (address != 0x00)
                                // Mando a través del client toda la data de la redemption // I send through the client all the data of the redemption
                                RedeemTicket(current_ticket_redemption);
                            // Reseteo la redemption // I reset the redemption
                            current_ticket_redemption = null;
                        }
                    }
                    // Si la transferCode es cualquiera que no sea 0xFF // If transferCode is anything other than 0xFF
                    else
                    {

                        int amountInt = 0;

                        try
                        {
                            amountInt = int.Parse(BitConverter.ToString(transferAmount).Replace("-", ""));
                        }
                        catch
                        {

                        }
                        // Si la amount y la validationData coinciden con el current ticket redemption.. // If the amount and validationData match the current ticket redemption ..
                        if (current_ticket_redemption.validationData.SequenceEqual(validationData))
                        {
                            current_ticket_redemption.amountBytes = transferAmount;
                            if (address != 0x00)
                            {
                                // Mando a través del client la respuesta con Pending (0x40) // I send through the client the response with Pending (0x40)
                                _client.RedeemTicket(_EGMSettings.GetAddress(),
                                                0x40,
                                                transferAmount,
                                                new byte[] { parsingCode },
                                                validationData);
                                // Lanzo al controller el event LP71 con todos los datos que me vienen del client // I throw the LP71 event to the controller with all the data that comes to me from the client
                                LP71(transferCode, amountInt, parsingCode, validationData, ee);

                            }
                            // Si transicioné a Stacking // If I transitioned to Stacking
                            if (current_ticket_redemption.Transition(TicketRedemptionStatus.Stacking))
                            {

                            }
                        }
                        // Si no coinciden, es probable que tengan el poolId y el restricted expiration // If they do not match, it is likely that they have the poolId and the restricted expiration 
                        else
                        {
                            try
                            {
                                // Descarto los últimos 6 elementos // I discard the last 6 elements
                                byte[] validationDataDroppedLast6Elements = validationData.Take(validationData.Length - 6).ToArray();
                                // Vuelvo a comparar, si la amount y la validation data coinciden con el current ticket redemption.. // I compare again, if the amount and validation data match the current ticket redemption ..
                                if (current_ticket_redemption.validationData.SequenceEqual(validationDataDroppedLast6Elements))
                                {
                                    // Tomo los últimos 6 elementos. (Restricted Expiration + PoolId) // I take the last 6 elements. (Restricted Expiration + PoolId)
                                    byte[] Last6Elements = validationData.Skip(validationData.Length - 6).ToArray();
                                    current_ticket_redemption.restrictedexpiration = new byte[] { Last6Elements[0], Last6Elements[1], Last6Elements[2], Last6Elements[3] };
                                    current_ticket_redemption.poolId = new byte[] { Last6Elements[4], Last6Elements[5] };
                                    current_ticket_redemption.amountBytes = transferAmount;
                                    if (address != 0x00)
                                    {
                                        // Mando a través del client la respuesta con Pending (0x40) // I send through the client the response with Pending (0x40)
                                        _client.RedeemTicket(_EGMSettings.GetAddress(),
                                                0x40,
                                                transferAmount,
                                                new byte[] { parsingCode },
                                                validationDataDroppedLast6Elements);
                                        // Lanzo al controller el event LP71 con todos los datos que me vienen del client // I throw the LP71 event to the controller with all the data that comes to me from the client
                                        LP71(transferCode, amountInt, parsingCode, validationDataDroppedLast6Elements, ee);
                                    }
                                    // Si transicioné a Stacking // If I transitioned to Stacking
                                    if (current_ticket_redemption.Transition(TicketRedemptionStatus.Stacking))
                                    {

                                    }
                                }
                                else //Retomo la validationData con todos los bytes // I resume the validationData with all the bytes
                                {
                                    current_ticket_redemption.amountBytes = transferAmount;
                                    if (address != 0x00)
                                    {
                                        // Mando a través del client la respuesta con Pending (0x40) // I send through the client the response with Pending (0x40)
                                        _client.RedeemTicket(_EGMSettings.GetAddress(),
                                                0x40,
                                                transferAmount,
                                                new byte[] { parsingCode },
                                                validationData);
                                        // Lanzo al controller el event LP71 con todos los datos que me vienen del client // I throw the LP71 event to the controller with all the data that comes to me from the client
                                        LP71(transferCode, amountInt, parsingCode, validationData, ee); 

                                    }
                                    // Si transicioné a Stacking // If I transitioned to Stacking
                                    if (current_ticket_redemption.Transition(TicketRedemptionStatus.Stacking))
                                    {

                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                // Si no hay ninguna redemption en proceso // If there is no redemption in process
                else
                { 
                    // Mando a través del client la respuesta con "Not compatible with current redemption cycle " (0xC0) // I send through the client the response with "Not compatible with current redemption cycle" (0xC0)
                    _client.RedeemTicket(_EGMSettings.GetAddress(),
                                        0xC0,
                                        new byte[] { },
                                        new byte[] { },
                                        new byte[] { });
                }


            });

            // Redeem ticket from VirtualEGM (Waiting one parameter: TransferCode) -- lp71
            _client.VirtualEGMRedeemTicket1 += new SASClient.VirtualEGMRedeemTicket1Handler((address, transferCode, ee) =>
            {
                // Chequeo que la current redemption esté en proceso // I check that the current redemption is in process
                if (current_ticket_redemption != null)
                {
                    // Si la transferCode es 0xFF // If the transferCode is 0xFF
                    if (transferCode == 0xFF)
                    {
                        // Es la última parte de la redemption, donde se consulta el estado de la redemption // It is the last part of the redemption, where the status of the redemption is consulted

                        // Si la current redemption la puedo transicionar a AckByHost
                        if (current_ticket_redemption.Transition(TicketRedemptionStatus.AckByHost)) // Transiciono a AckByHost
                        {
                            // Detengo el timer que envía exceptions 68 // I stop the timer that sends exceptions 68
                            Timer68.Stop();
                            if (address != 0x00)
                                // Mando a través del client toda la data de la redemption // I send through the client all the data of the redemption
                                RedeemTicket(current_ticket_redemption);
                            current_ticket_redemption = null;
                        }
                    }
                }
                // Si no hay ninguna redemption en proceso // If there is not any redemption in process      
                else
                {
                    // Mando a través del client la respuesta con "Not compatible with current redemption cycle " (0xC0) // I send through the client the response with "Not compatible with current redemption cycle" (0xC0)
                    _client.RedeemTicket(_EGMSettings.GetAddress(),
                                        0xC0,
                                        new byte[] { },
                                        new byte[] { },
                                        new byte[] { });
                }

            });

            // --lp72
            _client.VirtualEGMLP72 += new SASClient.VirtualEGMLP72Handler((address, transferCode, transactionIndex, transferType, cashableAmount, restrictedAmount, nonRestrictedAmount, transferFlags, assetNumber, registrationKey, transactionID, expiration, poolID, receiptData, lockTimeout, ee) =>
            {

                if (address != 0x00)
                {
                    AFTOperation op = new AFTOperation();
                    bool relayToEGM = false;
                    op.InternalStatus = GetTransferStatusCode(ref relayToEGM, transferCode, transactionIndex, transferType, cashableAmount, restrictedAmount, nonRestrictedAmount, transferFlags, assetNumber, registrationKey, transactionID, expiration, poolID, receiptData, lockTimeout);


                    op.Amount = cashableAmount;
                    op.expiration = expiration;
                    op.poolID = poolID;
                    op.ReceiptStatus = 0xFF;
                    op.RestrictedAmount = restrictedAmount;
                    op.NonRestrictedAmount = nonRestrictedAmount;
                    op.TransactionID = transactionID;
                    op.transferCode = transferCode;
                    op.transferFlags = transferFlags;
                    op.TransferType = transferType;
                    if (op.InternalStatus != 0xC0 && op.InternalStatus != 0xC1)
                    {
                        _EGMStatus.aftcollection.currentoperation = op;
                    }
                    _client.Send72Response(_EGMSettings.GetAddress(),
                                0x00,
                                op.InternalStatus,
                                op.ReceiptStatus,
                                transferType,
                                cashableAmount,
                                restrictedAmount,
                                nonRestrictedAmount,
                                transferFlags,
                                assetNumber,
                                transactionID,
                                op.InternalStatus == 0x40 ? null : op.TransactionDate, // transaction Date
                                expiration,
                                poolID,
                                op.InternalStatus == 0x40 ? new byte[] { } : new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },  // cumulative cashable amount
                                op.InternalStatus == 0x40 ? new byte[] { } : new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },  // cumulative restricted amount
                                op.InternalStatus == 0x40 ? new byte[] { } : new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // cumulative non restricted amount
                    if (relayToEGM)
                    {
                        // MANDAR EVENTO AL CONTROLLER // SEND EVENT TO CONTROLLER
                        try { LaunchLog(new string[] { "EGM <-", "<- SMIB" }, $"AFT Transfer initiated. Amount: {BitConverter.ToString(cashableAmount)}", ee); } catch { };
                        try { LP72(transferType, cashableAmount, restrictedAmount, nonRestrictedAmount, transactionID, registrationKey, expiration, ee); } catch { }
                    }
                }
                SaveEGMStatus();

            });

            // --lp72
            _client.VirtualEGMLP72Int += new SASClient.VirtualEGMLP72IntHandler((address, transferCode, transactionIndex, ee) =>
            {

                if (address != 0x00)
                {
                    if (transferCode == 0xFF)
                    {
                        AFTOperation op = null;
                        if (transactionIndex == 0x00)
                        {
                            op = _EGMStatus.aftcollection.currentoperation;
                            if (op != null)
                            {
                                if (op.AckStatus == AFTOperationStatus.OpFinished)
                                {
                                    _EGMStatus.aftcollection.UpdateAckStatusForCurrentOperation(AFTOperationStatus.OpAcknowledged);
                                    SaveEGMStatus();
                                    Timer69.Stop();
                                    try { LP72Interrogation(ee); } catch { }
                                }
                            }
                        }
                        else if (transactionIndex >= 01 && transactionIndex < 100)
                            op = _EGMStatus.aftcollection.GetOperationForIndex(transactionIndex);
                        if (op != null)
                        {
                            _client.Send72Response(_EGMSettings.GetAddress(),
                                    op.InternalStatus == 0x40 ? (byte)0x00 : op.Position,
                                    op.InternalStatus,
                                    op.ReceiptStatus,
                                    op.TransferType,
                                    op.Amount,
                                    op.RestrictedAmount,
                                    op.NonRestrictedAmount,
                                    op.transferFlags,
                                    _EGMSettings.assetId,
                                    op.TransactionID,
                                    op.InternalStatus == 0x40 ? null : op.TransactionDate, // transaction Date
                                    op.expiration,
                                    op.poolID,
                                    new byte[] { },  // cumulative cashable amount
                                    new byte[] { },  // cumulative restricted amount
                                    new byte[] { }); // cumulative non restricted amount
                        }
                    }


                    // MANDAR EVENTO AL CONTROLLER // SEND EVENT TO CONTROLLER
                }

            });

            // Register Gaming Machine -- lp73
            _client.VirtualEGMRegisterGamingMachine += new SASClient.VirtualEGMRegisterGamingMachine73Handler((address, RegistrationCode, e) =>
            {
                byte[] singleton = { RegistrationCode };
                if (RegistrationCode == 0xFF)
                {
                    _client.SendRegistrationResponse(_EGMSettings.GetAddress(),
                                           0x73,
                                           _EGMStatus.regStatus,
                                           _EGMSettings.assetId,
                                           _EGMStatus.regKey,
                                           _EGMStatus.regPOSId);
                }
                else if (RegistrationCode == 0x80)
                {
                    CancelRegistration();
                }
                if (RegistrationCode == 0x80)
                    LP73(RegistrationCode, new byte[] { }, new byte[] { }, e);
                //Console.WriteLine($"Registration Code is {BitConverter.ToString(singleton)}");
            }
             );

            // Register Gaming Machine -- lp73
            _client.VirtualEGMRegisterGamingMachine1 += new SASClient.VirtualEGMRegisterGamingMachine73_1Handler((address,
                                                                                                                  RegistrationCode,
                                                                                                                  AssetNumber,
                                                                                                                  RegistrationKey,
                                                                                                                  POSID,
                                                                                                                  e) =>
            {
                // Based on Registration Code
                switch (RegistrationCode)
                {
                    case 0x80:  //////// Cancel Registration ////////// // Cancel Registration
                        {
                            CancelRegistration();
                            break;
                        }
                    case 0x00: //////// Initialize Registration ////////// 
                        {
                            // If asset number doesn't match with current asset number or all bytes are zero, Cancel registration
                            if (!_EGMSettings.assetId.SequenceEqual(AssetNumber) || AssetNumber.All(b => b == 0x00))
                            {
                                CancelRegistration();
                                break;
                            }
                            try { LaunchLog(new string[] { "<- SMIB" }, $"SMIB initialized registration with key {BitConverter.ToString(RegistrationKey)}", e); } catch { };
                            // If Registration key length not equals to 20, Cancel registration
                            if (RegistrationKey.Length != 20)
                            {
                                CancelRegistration();
                                break;
                            }
                            // If POSID length not equals to 4, Cancel registration
                            if (POSID.Length != 4)
                            {
                                CancelRegistration();
                                break;
                            }
                            // Registration status will be 00
                            _EGMStatus.setStatus(0x00);

                            // Registration Response
                            if (address != 0x00)
                                _client.SendRegistrationResponse(_EGMSettings.GetAddress(),
                                                0x73,
                                                _EGMStatus.regStatus,
                                                AssetNumber,
                                                RegistrationKey,
                                                POSID);
                            break;
                        }
                    case 0x01: ///// Confirm Registration /////////
                        {
                            // If regStatus is 80, Registration canceled
                            if (_EGMStatus.regStatus == 0x80)
                            {
                                CancelRegistration();
                            } // if regStatus is 00 (Registration initialized)
                            else if (_EGMStatus.regStatus == 0x00)
                            {
                                // regStatus will be 01
                                _EGMStatus.setStatus(0x01);

                                // We set the registration Key
                                try { LaunchLog(new string[] { "<- SMIB" }, $"SMIB Updated the registration key to {BitConverter.ToString(RegistrationKey)}", e); } catch { };
                                _EGMStatus.setregistrationKey(RegistrationKey);

                                // We set the registration postId
                                _EGMStatus.setregistrationPOSId(POSID);

                                // Registration Response
                                if (address != 0x00)
                                    _client.SendRegistrationResponse(_EGMSettings.GetAddress(),
                                                    0x73,
                                                    _EGMStatus.regStatus,
                                                    AssetNumber,
                                                    RegistrationKey,
                                                    POSID);
                            }
                            // If regStatus is 01 (Registration confirmed)
                            else if (_EGMStatus.regStatus == 0x01)
                            {
                                // If asset number doesn't match with current asset number or all bytes are zero, Cancel registration
                                if (!_EGMSettings.assetId.SequenceEqual(AssetNumber) || AssetNumber.All(b => b == 0x00))
                                {
                                    CancelRegistration();
                                    break;
                                }
                                // Current registration key is not equals to Registration key parameter, Cancel registration
                                if (!_EGMStatus.regKey.SequenceEqual(RegistrationKey))
                                {
                                    CancelRegistration();
                                    break;
                                }
                                // Current registration pos ID is not equals to Pos ID parameter, Cancel registration
                                if (!_EGMStatus.regPOSId.SequenceEqual(POSID))
                                {
                                    CancelRegistration();
                                    break;
                                }
                                // El estado será 01 // Status will be 01
                                _EGMStatus.setStatus(0x01);
                                // Seteaamos el registrationKey en Status // Seteamos el POSId en registrationKey
                                _EGMStatus.setregistrationKey(RegistrationKey);
                                // Seteamos el POSId en registrationKey // Seteamos el POSId en registrationKey
                                _EGMStatus.setregistrationPOSId(POSID);

                                // Respuesta a la registración // Registration Response
                                if (address != 0x00)
                                    _client.SendRegistrationResponse(_EGMSettings.GetAddress(),
                                                    0x73,
                                                    _EGMStatus.regStatus,
                                                    AssetNumber,
                                                    RegistrationKey,
                                                    POSID);
                            }
                            break;
                        }
                }

                SaveEGMStatus();
                // Relay LP73
                LP73(RegistrationCode, RegistrationKey, POSID, e);
            });


            // AFT Lock and Status Request -- lp74
            _client.VirtualEGM74 += new SASClient.VirtualEGM74Handler((address, lockCode, transferCondition, lockTimeout, e) =>
            {
                GameAccounting gameAccounting = null;
                gameAccounting = _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 });
                bool startlock = (GetByteWithPos(transferCondition, 1) == (byte)2 || GetByteWithPos(transferCondition, 0) == (byte)1);
                /* restricted expiration */
                DateTime Expiration = BCDToTime(_EGMStatus.restrictedExpiration, 10);
                if (_EGMStatus.restrictedExpiration != null)
                {
                    if (_EGMStatus.restrictedExpiration[0] == 0x00)
                    {
                        int days = int.Parse(BitConverter.ToString(_EGMStatus.restrictedExpiration).Replace("-", ""));
                        Expiration = BCDToTime(null, days);
                    }
                }
                /****/
                if (address != 0x00)
                    _client.Send74Response(address,
                                           _EGMSettings.assetId, // Asset Number
                                           lockCode == 0x00 ? (startlock ? (byte)0x00 : (byte)0xFF) : _EGMStatus.gameLockStatus, // Lock Status
                                           _EGMStatus.availableTransfers, // Available Transfers
                                           _EGMStatus.hostCashoutStatus, // Host Cashout Status,
                                           _EGMStatus.aftStatus, // AFTStatus,
                                           _EGMSettings.gmMaxBufferIndex, // Max Buffer Index 
                                           _EGMStatus.currentCashableAmount, // Current Cashable Amount
                                           _EGMStatus.currentRestrictedAmount, // Current Restricted Amount
                                           _EGMStatus.currentNonRestrictedAmount, // Current Non Restricted Amount
                                           _EGMSettings.gmTransferLimit, // Gaming Machine Transfer Limit
                                           Expiration, // Expiration
                                           _EGMSettings.restrictedPoolID); // Restricted Pool ID
                if (startlock)
                {
                    LP74(lockCode, transferCondition, lockTimeout, e);
                }
            });


            // Extended Validation Status from Virtual EGM (waiting 5 parameters:) -- lp7B
            _client.VirtualEGMExtendedValidationStatus += new SASClient.VirtualEGMExtendedValidationStatusHandler((address, controlMask, statusBitControlStates, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration, ee) =>
            {
                // Màscara de control // Control Mask
                _EGMSettings.controlMask = controlMask;
                // Los estados. // The states
                // ASUMO: Por cada bit de estado, si la máscara de control me habilita a actualizar el valor // I assume: For each state bit, if the control mask enables me to update the value
                // Habilito o deshabilito la función a travès de lo que me venga como parámetro en statusBitControlStates // I enable or disable the function through what comes to me as a parameter in statusBitControlStates
                //_EGMSettings.statusBitControlStates = UpdateValidationStatusBits(controlMask,statusBitControlStates, _EGMSettings.statusBitControlStates);

                if (address != 0x00)
                {
                    _client.SendExtendedValidationStatusGamingMachine(_EGMSettings.GetAddress(), _EGMSettings.assetId, _EGMSettings.statusBitControlStates, _EGMSettings.cashableTicketAndReceiptExpiration, _EGMSettings.restrictedTicketDefaultExpiration);
                }
                SaveEGMSettings();
            });

            // Receive Extended Ticket Data from Host -- lp7C
            _client.VirtualEGMSetExtendedTicketData += new SASClient.VirtualEGMSetExtendedTicketDataHandler((address, data_elements, e) =>
            {
                // Para cada par en este array, de acuerdo al primer elemento del par, // For each pair in this array, according to the first element of the pair,
                // Asigno a ciertas properties de la EGMSettings // I assign to certain properties of the EGMSettings
                // Luego lanzo el evento lp7C con ese par al Controller // Then I launch the lp7C event with that pair to the Controller
                foreach (Tuple<byte, string> t in data_elements)
                {
                    if (t.Item1 == 0x00)
                        _EGMSettings.LocationName = t.Item2;
                    if (t.Item1 == 0x01)
                        _EGMSettings.LocationAddress1 = t.Item2;
                    if (t.Item1 == 0x02)
                        _EGMSettings.LocationAddress2 = t.Item2;
                    if (t.Item1 == 0x10)
                        _EGMSettings.RestrictedTicketTitle = t.Item2;
                    if (t.Item1 == 0x20)
                        _EGMSettings.DebitTicketTitle = t.Item2;
                    try
                    {
                        LP7C(t.Item1, t.Item2, e);
                    }
                    catch
                    {

                    }
                }
                // Guardo los cambios persistiendo // I save the changes persisting
                SaveEGMSettings();
                if (address != 0x00)
                    _client.SetExtendedTicketDataResponse(_EGMSettings.GetAddress(),
                                                        0x01);
            });

            // Response al long poll 7D
            _client.VirtualEGMSetTicketData7D += new SASClient.VirtualEGMSetTicketData7DHandler((address,
                                                                                                 hostID,
                                                                                                 expiration,
                                                                                                 locationLengthByte,
                                                                                                 location,
                                                                                                 address1lengthByte,
                                                                                                 address1,
                                                                                                 address2lengthByte,
                                                                                                 address2, ee) =>
            {
                if (address != 0x00)
                    _client.SetTicketDataResponse(_EGMSettings.GetAddress(),
                                                0x01);
                Encoding encoding = Encoding.Default;
                _EGMSettings.LocationName = encoding.GetString(location);
                _EGMSettings.LocationAddress1 = encoding.GetString(address1);
                _EGMSettings.LocationAddress2 = encoding.GetString(address2);
                _EGMSettings.expirationDays = (int)expiration;
                LP7D(hostID, expiration, location, address1, address2, ee);

            });

            // Response al long poll 7D with length = 0
            _client.VirtualEGMSetTicketData7DWith00 += new SASClient.VirtualEGMSetTicketData7DWith00Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SetTicketDataResponse(_EGMSettings.GetAddress(),
                                                  0x00);
            });

            // Response al longpoll 7E
            _client.VirtualEGM7E += new SASClient.VirtualEGM7EHandler((address, ee) =>
            {

                try
                {
                    // 7E, Current DateTime
                    // T: EGMTime
                    // N: Now
                    // L: Last Polled Time
                    // El now es el cálculo de T + (N - L) // The now is the calculation of T + (N - L)
                    // Básicamente el tiempo absoluto transcurrido hasta ahora, desde el tiempo T  // Basically the absolute time elapsed until now, from time T
                    DateTime now = UnixTimeStampToDateTime(0);
                    if (_EGMStatus.EGMTime > 0)
                        now = UnixTimeStampToDateTime(_EGMStatus.EGMTime + (DateTimeOffset.Now.ToUnixTimeSeconds() - _EGMStatus.EGMLastPolledTime));
                    if (address != 0x00)
                        _client.SendCurrentDateTimeGamingMachineLP7E(_EGMSettings.GetAddress(),
                                                                    now.Month,
                                                                    now.Year,
                                                                    now.Day,
                                                                    now.Hour,
                                                                    now.Minute,
                                                                    now.Second);
                }
                catch (Exception ex)
                {
                    _EGMInfo.Error = ex.Message;
                    SaveEGMInfo();

                }


            });

            // Receive Date and Time Command --lp7F
            _client.VirtualEGMReceiveDateAndTime += new SASClient.VirtualEGMReceiveDateAndTimeHandler((address, date, time, ee) =>
            {
                if (address != 0x00) _client.SendACK();
                // Manda al controller el event LP7F con todos los datos que me vienen del client // Send the LP7F event to the controller with all the data that comes to me from the client
                LP7F(date, time, ee);

            });

            // Single Level Progressive -- lp80 
            _client.VirtualEGMLP80 += new SASClient.VirtualEGMLP80Handler((broadcast, group, level, amount, ee) =>
            {
                // si el broadcast no está activado // if the broadcast is not activated
                if (!broadcast)
                {
                    // Envío el ACK // I send the ACK
                    _client.SendACK();
                }
                // Manda al controller el event LP80 con todos los datos que me viene del client // Send the LP80 event to the controller with all the data that comes to me from the client
                LP80(broadcast, group, level, amount, ee);
            });

            // General Long Poll event (81) // General Long Poll event (81)
            _client.GeneralLongPoll += new SASClient.GeneralLongPollHandler(ee =>
            {
                // Si hay un evento prioritario en la priority queue // If there is a priority event in the priority queue
                if (_EGMStatus.priorityeventqueue.Count() > 0)
                {
                    EGMException exp = null;
                    // Desencolo el evento y lo envío a través del client // I dequeue the event and send it through the client
                    if (_EGMStatus.priorityeventqueue.TryDequeue(out exp))
                    {
                        _client.SendException(_EGMSettings.GetAddress(), exp.exception, exp.data);
                    }
                }
                // Si hay un evento en la queue de eventos // If there is an event in the event queue
                else if (_EGMStatus.eventqueue.Count() > 0)
                {
                    EGMException exp = null;
                    // Desencolo el evento y lo envío a través del client // I dequeue the event and send it through the client
                    if (_EGMStatus.eventqueue.TryDequeue(out exp))
                    {
                        _client.SendException(_EGMSettings.GetAddress(), exp.exception, exp.data);
                    }
                }
                // Si las dos colas están vacías // If the two queues are empty
                else
                {
                    // Envío el evento 00 // I send the event 00
                    _client.SendException(_EGMSettings.GetAddress(), 0x00, new byte[] { });
                }
                // Persisto en EGM Status // I persist in EGM Status
                SaveEGMStatus();
            });

            // Response al long poll 83 // Response to long poll 83
            _client.VirtualEGMLP83 += new SASClient.VirtualEGMLP83Handler((address, gameNumber, ee) =>
            {
                if (address != 0x00)
                    _client.SendCumulativeProgressiveWinsAmounts(_EGMSettings.GetAddress(),
                                                                gameNumber,
                                                                new byte[] { 0x00, 0x00, 0x00, 0x00 });
            });

            // Response al long poll 84 
            _client.VirtualEGMLP84 += new SASClient.VirtualEGMLP84Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendProgressiveWinAmount(_EGMSettings.GetAddress(),
                                                    0x00,
                                                    0x00,
                                                    0);
            });

            // Response al long poll 85
            _client.VirtualEGMLP85 += new SASClient.VirtualEGMLP85Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendSASProgressiveWinAmount(_EGMSettings.GetAddress(),
                                                        0x00,
                                                        0x00,
                                                        0);
            });

            // Multiple Level Progessive -- lp86
            _client.VirtualEGMLP86 += new SASClient.VirtualEGMLP86Handler((address, broadcast, group, amountsByLevel, ee) =>
            {
                // si el broadcast no está activado // if the broadcast is not activated
                if (!broadcast)
                {
                    // Mando ACK // I send ACK
                    if (address != 0x00)
                        // Respondo por el client // I respond for the client
                        _client.SendACK();
                }
                // Manda al controller el event LP86 con todos los datos que me viene del client // Send the LP86 event to the controller with all the data that comes to me from the client
                LP86(broadcast, group, amountsByLevel, ee);
            });


            // Response al long poll 87 // Response to long poll 87
            _client.VirtualEGMLP87 += new SASClient.VirtualEGMLP87Handler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendMultipleSASProgressiveWinAmounts(_EGMSettings.GetAddress(),
                                                                0x00,
                                                                new List<Tuple<byte, int>>());
            });


            // Response al long poll 8A 
            _client.VirtualEGMLP8A += new SASClient.VirtualEGMLP8AHandler((address, bonusAmount, taxStatus, ee) =>
            {
                if (address != 0x00) _client.SendACK();
                // Lanzo al client el event LP8A con los parámetros bonusAmount y taxStatus // I launch the LP8A event to the client with the bonusAmount and taxStatus parameters
                LP8A(bonusAmount, taxStatus, ee);
            });


            // Reset handpay gaming machine Command --lp94
            _client.VirtualEGMResetHandpay += new SASClient.VirtualEGMResetHandpayHandler((address, ee) =>
            {
                if (address != 0x00)
                    // Respondo por el client // I respond for the client
                    _client.SendResetHandpayGamingMachineResponse(_EGMSettings.GetAddress(),
                                                                0x00);
                // Manda al controller el event LP94 con todos los datos que me vienen del client // Send the LP94 event to the controller with all the data that comes to me from the client
                LP94(ee);

            });


            // Response al long poll 9A
            _client.VirtualEGMLP9A += new SASClient.VirtualEGMLP9AHandler((address, gameNumber, ee) =>
            {
                GameAccounting gameAccounting = null;
                gameAccounting = HandleGetGameAccounting("9A", gameNumber);
                if (address != 0x00)
                    _client.Response9A(_EGMSettings.GetAddress(),
                                       gameNumber,
                                       _EGMAccounting.GetValueOfMeter("BonusingDeductible", gameAccounting),
                                       _EGMAccounting.GetValueOfMeter("BonusingNoDeductible", gameAccounting),
                                       _EGMAccounting.GetValueOfMeter("BonusingWagerMatch", gameAccounting));
            });


            // Send Enabled Features -- lpA0
            _client.VirtualEGMA0 += new SASClient.VirtualEGMA0Handler((address, gameNumber, e) =>
            {
                if (address != 0x00)
                    _client.SendEnabledFeatures(_EGMSettings.GetAddress(),
                                                gameNumber,
                                                _EGMSettings.features1,
                                                _EGMSettings.features2,
                                                _EGMSettings.features3);
            });

            // Response al longpoll A4
            _client.VirtualEGMA4 += new SASClient.VirtualEGMA4Handler((address, gameNumber, ee) =>
            {
                if (address != 0x00)
                    _client.SendCashoutLimitGamingMachine(_EGMSettings.GetAddress(),
                                                         gameNumber,
                                                         _EGMSettings.GetGameSettings(gameNumber).CashoutLimit);
            });

            // Response al longpoll A8
            _client.VirtualEGMA8 += new SASClient.VirtualEGMA8Handler((address, resetMethod, ee) =>
            {
                if (address != 0x00)
                    _client.EnableJackpotHandpayResetMethod(_EGMSettings.GetAddress(), 0x02);
                // Lanzo al client el event LPA8 con el parámetro resetMethod // I launch the LPA8 event to the client with the resetMethod parameter
                LPA8(resetMethod, ee);
            });

            // Response al long poll B1
            _client.VirtualEGMSendCurrentPlayerDenomination += new SASClient.VirtualEGMSendCurrentPlayerDenominationHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendCurrentPlayerDenomination(_EGMSettings.GetAddress(),
                                                        _EGMStatus.CurrentPlayerDenomination);
            });

            // Response al long poll B2
            _client.VirtualEGMSendEnabledPlayerDenominations += new SASClient.VirtualEGMSendEnabledPlayerDenominationsHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendEnabledPlayerDenominations(_EGMSettings.GetAddress(),
                                                        _EGMStatus.NumberOfDenominations,
                                                        _EGMStatus.PlayerDenominations);
            });

            // Response al long poll B3
            _client.VirtualEGMSendTokenDenomination += new SASClient.VirtualEGMSendTokenDenominationHandler((address, ee) =>
            {
                if (address != 0x00)
                    _client.SendTokenDenomination(_EGMSettings.GetAddress(),
                                                _EGMStatus.TokenDenomination);
            });
            // Response to long poll B4
            _client.VirtualEGMSendWagerCategoryInformation += new SASClient.VirtualEGMSendWagerCategoryInformationHandler((address, gameNumber, wagerCategory, ee) => 
            {
                bool found = false;
                if (address != 0x00)
                {
                    GameInfo gameInfo = _EGMInfo.GetGameInfo(gameNumber);
                    if (gameInfo.wagerCategoriesList.Count() > 0)
                    {
                        WagerCategory c = gameInfo.wagerCategoriesList.Where(wc => wc.category.SequenceEqual(wagerCategory)).FirstOrDefault();
                        if (c != null)
                        {
                            _client.SendWagerCategoryInformation(address, gameNumber, c.category, c.paybackPercentage, c.coinInMeterValue);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        WagerCategory nc = new WagerCategory();
                       _client.SendWagerCategoryInformation(address, gameNumber, nc.category, nc.paybackPercentage, nc.coinInMeterValue);
                    }
                }
            });
            // Extended Game Information from VirtualEGM -- lpB5
            _client.VirtualEGMSendExtendedGameInformation += new SASClient.VirtualEGMSendExtendedGameInformationHandler((address, gameNumber, ee) =>
            {
                // Obtengo, a través del gameNumber, el game accounting, game settings y game info de la persistencia // I get, through the gameNumber, the game accounting, game settings and game info of the persistence
                GameAccounting gameAccounting = HandleGetGameAccounting("B5", gameNumber);
                GameSettings gameSettings = _EGMSettings.GetGameSettings(gameNumber);
                GameInfo gameInfo = _EGMInfo.GetGameInfo(gameNumber);
                // Si los 3 elementos existen // If the 3 elements exist
                if (gameInfo != null && gameSettings != null && gameAccounting != null)
                {
                    byte GameNameLength = 0x00;
                    if (gameInfo.gameName != null)
                        GameNameLength = Convert.ToByte(gameInfo.gameName.Length);

                    byte PaytableNameLength = 0x00;
                    if (gameInfo.paytableName != null)
                        PaytableNameLength = Convert.ToByte(gameInfo.paytableName.Length);
                    // Armo toda la data y la envío al client // I put together all the data and send it to the client
                    if (address != 0x00)
                        _client.SendExtendedGameNInformationResponse(_EGMSettings.GetAddress(),
                                                                     gameNumber,
                                                                     gameSettings.maxBet,
                                                                     gameSettings.progressiveGroup,
                                                                     gameSettings.progressiveLevels == null ? new byte[] { 0x00, 0x00, 0x00, 0x00 } : gameSettings.progressiveLevels,
                                                                     GameNameLength,
                                                                     gameInfo.gameName,
                                                                     PaytableNameLength,
                                                                     gameInfo.paytableName,
                                                                     gameInfo.wagerCategories == null ? new byte[] { 0x00, 0x00 } : gameInfo.wagerCategories);

                }
            });

            // Getting adress of Virtual EGM
            _client.VirtualEGMGetAddress += new SASClient.VirtualEGMGetAddressHandler(e => { return _EGMSettings.GetAddress(); });

            // Getting bool EnabledRealTime of Virtual EGM
            _client.VirtualEGMGetEnabledRealTime += new SASClient.VirtualEGMGetEnabledRealTimeHandler(e => { return _EGMSettings.GetEnabledRealTime(); });


            // Apply Real Time. Se activa cuando llega un long poll con misma address
            _client.ApplyRealTime += new SASClient.ApplyRealTimeHandler((buffer,
                                                                         ee) =>
            {
                // Si hay un evento prioritario en la priority queue // If there is a priority event in the priority queue
                if (_EGMStatus.priorityeventqueue.Count() > 0)
                {
                    EGMException exp = null;
                    // Desencolo el evento y lo envío a través del client // I dequeue the event and send it through the client
                    if (_EGMStatus.priorityeventqueue.TryDequeue(out exp))
                    {
                        _client.SendException(_EGMSettings.GetAddress(), exp.exception, exp.data);
                        // Persisto en EGM Status // Persist in EGM Status
                        SaveEGMStatus();
                    }
                }
                // Si hay un evento en la queue de eventos // If there is an event in the event queue
                else if (_EGMStatus.eventqueue.Count() > 0)
                {
                    EGMException exp = null;
                    // Desencolo el evento y lo envío a través del client // I dequeue the event and send it through the client
                    if (_EGMStatus.eventqueue.TryDequeue(out exp))
                    {
                        _client.SendException(_EGMSettings.GetAddress(), exp.exception, exp.data);
                        // Persisto en EGM Status // Persist in EGM Status
                        SaveEGMStatus();

                    }
                }
                // Si las dos colas están vacías // if the two queues are empty
                else{
                    // Analiso el long poll (analyze the long poll)
                    _client.AnalyzeLongPoll(buffer);
                }
            });
        }
        #endregion

        /// <summary>
        /// Dado un array (o lista) de array de bytes, la idea es "aplanar" la lista en un sólo array que concatene cada elemento // Given an array (or list) of byte arrays, the idea is to "flatten" the list into a single array that concatenates each element
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
        #endregion



        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /**********************************************     PUBLIC METHODS  *********************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region PUBLIC METHODS 

        #region ExceptionsAndPersistence

        /// <summary>
        /// EXCPT 3D/3E
        /// ******* PART OF THE VALIDATION PROCESS *******
        /// PrintingAndSave4DData, throws the exception 3D or 3E
        /// Method that allows simulate the ticket printing, sending an exception 3D if validation can transition to Printing state. Its parameters are saved on properties of same name in validation object.
        /// Starts a timer that sends the exception 3D/3E periodically if smib or host doesn't reaction
        /// </summary>
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
        public void PrintingAndSave4DData(byte validationType,
                                        byte indexNumber,
                                        byte[] date,
                                        byte[] time,
                                        byte[] validationNumber,
                                        byte[] amount,
                                        byte[] ticketNumber,
                                        byte validationSystemId,
                                        byte[] expiration,
                                        byte[] poolId,
                                        byte exception)
        {
            // Si aún no instancié el current ticket validation, lo hago ahora // If I have not yet instantiated the current ticket validation, I do it now
            InitCurrentTicketValidation(amount, null);

            // Transiciono a Printing, generando la excepción // I transition to Printing, generating the exception
            if (current_ticket_validation.Transition(TicketValidationStatus.Printing))
            {
                current_ticket_validation.validationType = validationType;
                current_ticket_validation.indexNumber = indexNumber;
                current_ticket_validation.date = date;
                current_ticket_validation.time = time;
                current_ticket_validation.validationNumber = validationNumber;
                current_ticket_validation.amount = uint.Parse(BitConverter.ToString(amount).Replace("-", "")); ;
                current_ticket_validation.ticketNumber = ticketNumber;
                current_ticket_validation.validationSystemID = validationSystemId;
                current_ticket_validation.expiration = expiration;
                current_ticket_validation.poolId = poolId;

                EnqueueException(exception, new byte[] { });
                if (exception == 0x3D)
                    Timer3D.Start();
                else if (exception == 0x3E)
                    Timer3E.Start();
            }
            SaveEGMSettings();
        }

        /// <summary>
        /// EXCPT 51
        /// ****** PART OF HANDPAY PROCESS ******
        /// HandpayPending, throws an exception 51
        /// Returns true if it could initiate a handpay
        /// </summary>
        /// <returns>true if could initiate a handpay. false if there is a handpay in process </returns>
        public bool HandpayPending()
        {
            // Si no hay un handpay en proceso // If there is no handpay in process
            if (!HandpayInProcess)
            {
                // Seteo el booleano de handpay en proceso en true // I set the boolean of handpay in process in true
                HandpayInProcess = true;
                // Encolo la exception 51 // I enqueue the exception 51 
                EnqueueException(0x51, new byte[] { });
                // Si hay un 51 enviado y confirmado // If there is a 51 sent and confirmed
                if (Current51IssuedAndAcknowledged)
                {
                    // Paso a false este booleano // I set this boolean to false
                    Current51IssuedAndAcknowledged = false;
                }
                // Paro el timer del 51 // I stop the timer of the 51
                try { Timer51.Stop(); } catch { }
                // Lo vuelvo a arrancar // I start it again
                try { Timer51.Start(); } catch { }
                // Encolo la respuesta del 1B // I enqueue the response of the 1B
                Enqueue1BResponse();
                // Guardo todo // I save everything
                SaveEGMStatus();
                return true;
            }
            return false;
        }

        /// <summary>
        /// EXCPT 51
        /// ****** PART OF HANDPAY PROCESS ******
        /// HandpayPending, throws an exception 51
        /// Returns true if it could initiate a handpay
        /// </summary>
        /// <param name="progressiveGroup"></param>
        /// <param name="level"></param>
        /// <param name="amount"></param>
        /// <param name="partialPay"></param>
        /// <param name="resetID"></param>
        /// <returns>true if could initiate a handpay. false if there is a handpay in process </returns>
        public bool HandpayPending(byte progressiveGroup,
                                            byte level,
                                            byte[] amount,
                                            byte[] partialPay,
                                            byte resetID)
        {
            // Si no hay un handpay en proceso // If there is no handpay in process
            if (!HandpayInProcess)
            {
                // Seteo el booleano de handpay en proceso en true // I set the boolean of handpay in process in true
                HandpayInProcess = true;
                // Encolo la exception 51 // I enqueue the exception 51
                EnqueueException(0x51, new byte[] { });
                // Si hay un 51 enviado y confirmado // If there is a 51 sent and confirmed
                if (Current51IssuedAndAcknowledged)
                {
                    // Paso a false este booleano // I set this boolean to false
                    Current51IssuedAndAcknowledged = false;
                }
                // Paro el timer del 51 // I stop the timer of the 51
                try { Timer51.Stop(); } catch { }
                // Lo vuelvo a arrancar // I start it again
                try { Timer51.Start(); } catch { }
                // Encolo la respuesta del 1B // I enqueue the response of the 1B
                Enqueue1BResponse(progressiveGroup,
                                         level,
                                        amount,
                                        partialPay,
                                        resetID);
                // Guardo todo // I save everything
                SaveEGMStatus();
                return true;
            }
            return false;
        }


        /// <summary>
        /// EXCPT 52
        /// ****** PART OF HANDPAY PROCESS ******
        /// Obtains the last handpay of  the queue, transitioning to HandpayReset state of the SM. If transitioned correctly and the handpay is acknoledged, throws the exception 52 and removes it from tthe queue
        /// Determines if there is a handpay in process
        /// </summary>
        /// <returns>true if could reset the handpay, false other wise</returns>
        public bool HandpayReset()
        {
            try
            {
                // Obtengo el handpay último de la cola // I get the last handpay of the queue
                Handpay hp = _EGMStatus.Handpayqueue.Last();
                // Si transicioné bien a HandpayReset // If I transitioned correctly to HandpayReset
                if (hp.Transition(HandpaySMStatus.HandpayReset))
                {
                    // Si el handpay está confirmado con ACK // If the handpay is confirmed with ACK
                    if (hp.LP1BAcknowledged == true)
                    {
                        // Envío el 52 y lo remuevo de la cola // I send the 52 and remove it from the queue
                        ApplyToDequeue(hp1 => { EnqueueException(0x52, new byte[] { }); });
                    }
                    // Sentencio que no hay algún handpay en proceso // I set that there is no handpay in process
                    HandpayInProcess = false;
                    // Guardo todo // I save everything
                    SaveEGMStatus();
                    return true;
                }
            }
            catch
            {
            }
            return false;

        }


        /// <summary>
        /// EXCPT 57
        /// ********** PART OF VALIDATION PROCESS *********
        /// USED BY CASHOUT AND PRINTINGANDSAVE4DData
        /// InitCurrentTicketValidation, throws the exception 57 if validation type is System
        ///  Initiating the current ticket validation, based on amount and cashoutType
        ///  If cashout type is system, throws the exception 57 and the status of validation is Sending57
        ///  Otherwise, the type will be Enhanced and vaildation status will be ResponseToLP58 (For skip states)
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="cashoutType"></param>
        public void InitCurrentTicketValidation(byte[] amount, byte? cashoutType)
        {
            if (current_ticket_validation == null)
            {
                current_ticket_validation = new TicketValidation();
                current_ticket_validation.LastTransitionTS = DateTime.Now;
                current_ticket_validation.amount = uint.Parse(BitConverter.ToString(amount).Replace("-", ""));
                if (cashoutType != null)
                    current_ticket_validation.cashoutType = cashoutType.Value;
                // Si se está en System Validation // If it is in System Validation
                if (_EGMSettings.ValidationType == EGMValidationType.System)
                {
                    // El estado inicial es Sending57, y se envía al client / sastest la excepción 57 // The initial state is Sending57, and the exception 57 is sent to the client / sastest
                    current_ticket_validation.status = TicketValidationStatus.Sending57;
                    EnqueueException(0x57, new byte[] { });
                }
                // Si se está en Enhanced, el estado estará en ResponseToLP58, habilitando la transición a Printing3D // If it is in Enhanced, the state will be in ResponseToLP58, enabling the transition to Printing3D
                else if (_EGMSettings.ValidationType == EGMValidationType.Enhanced)
                {
                    current_ticket_validation.status = TicketValidationStatus.ResponseToLP58;
                }
            }
        }
        /// <summary>
        /// ******* PART OF VALIDATION PROCESS *******
        /// Simulates a cashout, initializating una TicketValidation
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="cashoutType"></param>
        public void Cashout(byte[] amount, byte cashoutType)
        {
            // Si aún no instancié el current ticket validation, lo hago ahora // If I have not yet instantiated the current ticket validation, I do it now
            InitCurrentTicketValidation(amount, cashoutType);
            SaveEGMSettings();
        }


        /// <summary>
        /// EXCPT 67
        /// ******* PART OF REDEMPTION PROCESS *********
        /// TicketHasBeenInserted, throws the excepción 67
        /// Generates a redemption based on the given parameters and enqueues the exception 67
        /// Its state machine is on the state TicketInserted. Launchs a timer that sends the exception 67 if smib or host doesn't reaction
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="machineStatus"></param>
        /// <param name="code"></param>
        /// <param name="validationData"></param>
        public void TicketHasBeenInserted(int amount, byte machineStatus, byte code, byte[] validationData)
        {
            if (current_ticket_redemption == null)
            {
                current_ticket_redemption = new TicketRedemption();
                current_ticket_redemption.LastTransitionTS = DateTime.Now;
                current_ticket_redemption.amount = amount;
                current_ticket_redemption.parsingCode = code;
                current_ticket_redemption.machineStatus = machineStatus;
                current_ticket_redemption.validationData = validationData;
                current_ticket_redemption.status = TicketRedemptionStatus.TicketInserted;
                EnqueueException(0x67, new byte[] { });
                Timer67.Start();
            }

        }

        /// <summary>
        /// EXCPT 68
        /// ******* PART OF REDEMPTION PROCESS ********
        /// RedemptionCompleted, throws the exception 68
        /// Method that completes a redemption in process. The state machine has to transition to Sending68 state, throwing an exception 68. Updates the status of redemption with the parameter value
        /// </summary>
        /// <param name="status"></param>
        public void RedemptionCompleted(byte status)
        {
            if (current_ticket_redemption.Transition(TicketRedemptionStatus.Sending68))
            {
                if (status == 0x00)
                    _EGMAccounting.Credits += (uint)current_ticket_redemption.amount;
                current_ticket_redemption.machineStatus = status;
                EnqueueException(0x68, new byte[] { });
                Timer68.Start();
            }

        }

        /// <summary>
        /// *** EXCPT 69 ***
        /// *** PART OF THE AFT PROCESS ***
        /// Método que permite completar una transfer en proceso
        /// Method that completes a transfer in progress
        /// </summary>
        /// <param name="transferStatus">The transfer status for this transfer (please refer protocol documentation)</param>
        public void AFTTransferCompleted(byte transferStatus)
        {
            // Check that aftcollection of EGMStatus is not null
            if (_EGMStatus.aftcollection != null)
            {
                AFTOperation lastop = _EGMStatus.aftcollection.currentoperation;
                if (lastop != null)
                {
                    if (lastop.AckStatus == AFTOperationStatus.OpCreated)
                    {
                        _EGMStatus.aftcollection.UpdateStatusForCurrentOperation(transferStatus);
                        _EGMStatus.aftcollection.UpdateAckStatusForCurrentOperation(AFTOperationStatus.OpFinished);
                        SaveEGMStatus();
                        EnqueueException(0x69, new byte[] { });
                        Timer69.Start();
                    }
                }
            }

        }

        #endregion

        #region Long Polls

        /// <summary>
        /// Send LP21 response to SASClient, to write to Smib port
        /// </summary>
        /// <param name="romSignature"></param>
        public void ROMSignatureVerificationResponse(byte[] romSignature)
        {
            _client.ROMSignatureVerificationResponse(_EGMSettings.GetAddress(),
                                                    romSignature);
        }

        /// <summary>
        /// LP4D 
        /// ******* PART OF VALIDATION PROCESS *******
        /// Send the information of Enhanced Validation to SMIB
        /// </summary>
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
        public void SendEnhancedValidationInformation(byte validationType,
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
            _client.SendEnhancedValidationInformation(_EGMSettings.GetAddress(),
                                                      validationType, // validation type
                                                      indexNumber, // indexnumber
                                                      date, // date
                                                      time, // time
                                                      validationNumber, // validation number
                                                      amount, // amount
                                                      ticketNumber, // ticket number
                                                      validationSystemId, // validation system id
                                                      expiration, // expiration
                                                      poolId); // pool id
        }

        /// <summary>
        /// LP71
        /// ******* PART OF REDEMPTION PROCESS *******
        /// RedeemTicket, send a long poll 71 response
        /// Takes a redemption and sends the long poll 71 with the information of the redemption
        /// </summary>
        /// <param name="ticket"></param>
        public void RedeemTicket(TicketRedemption ticket)
        {
            if (ticket != null)
            {
                _client.RedeemTicket(_EGMSettings.GetAddress(),
                                    ticket.machineStatus,
                                    ticket.amountBytes,
                                    new byte[] { ticket.parsingCode },
                                    ticket.validationData);
            }
        }



        #endregion

        #region OnlyPersistence

        /// <summary>
        /// ******* PART OF REDEMPTION PROCESS ********
        /// Set null the redemption in process. This allows reset the redemption 
        /// </summary>
        public void ResetCurrentTicketRedemption()
        {
            current_ticket_redemption = null;
        }

        /// <summary>
        /// ******* PART OF VALIDATION PROCESS ********
        /// Set null the validation in process. This allows reset the validation
        /// </summary>
        public void ResetCurrentTicketValidation()
        {
            current_ticket_validation = null;
        }

        /// <summary>
        /// *** EGM Accounting ***  *** METERS ***
        /// Ram Clear. Reset Meters
        /// </summary>
        public void RAMClear()
        {
            _EGMAccounting.ResetMeters();
        }

        /// <summary>
        /// ** EGM Accounting **  *** METERS ***
        /// Updating of meters version 1, when the code is a byte from the meter code list of protocol. These values are saved in EGMAccounting
        /// </summary>
        /// <param name="code"></param>
        /// <param name="gameNumber"></param>
        /// <param name="value"></param>
        public void UpdateMeter(byte code, byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, code, value);
        }

        /// <summary>
        /// *** EGM Accounting **  *** METERS ***
        /// Updating of meters version 2, when the code is a string, whose value or meter isn't in the meter code list of protocol. These values are saved in EGMAccounting
        /// </summary>
        /// <param name="meter_string"></param>
        /// <param name="gameNumber"></param>
        /// <param name="value"></param>
        public void UpdateMeter(string meter_string, byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, meter_string, value);
        }

        /// <summary>
        /// ** EGM Accounting *** *** METERS ***
        /// Allow persist in the EGM Accounting once all meters are updated
        /// </summary>
        public void AllMetersUpdated()
        {
            SaveEGMAccounting();
        }

        /// <summary>
        /// ** EGM Info **
        /// Updates the NumberOfGamesImplemented field of EGMInfo
        /// </summary>
        /// <param name="numberOfGames"></param>
        public void SetNumberOfGames(byte[] numberOfGames)
        {
            _EGMInfo.NumberOfGamesImplemented = int.Parse(BitConverter.ToString(numberOfGames).Replace("-", ""));
            SaveEGMInfo();
        }


        /// <summary>
        /// **** EGM Info ****
        /// Set the versionID in EGM Info
        /// </summary>
        /// <param name="versionID"></param>
        public void SetVersionID(byte[] versionID)
        {
            _EGMInfo.SASVersion = versionID;
            SaveEGMInfo();
        }

        /// <summary>   
        /// *** EGM Info ***
        ///  Sets the serial number of the gaming machine in EGM Info
        /// </summary>
        /// <param name="gmSerialNumber"></param>
        public void SetGameMachineSerialNumber(byte[] gmSerialNumber)
        {
            _EGMInfo.GMSerialNumber = gmSerialNumber;
            SaveEGMInfo();
        }

        /// <summary>
        /// ** EGM Status **
        /// Updates the CurrentGameNumber field of EGM Status
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SetCurrentGameNumber(byte[] gameNumber)
        {
            _EGMStatus.CurrentGameNumber = gameNumber;
            SaveEGMStatus();

        }

        /// <summary>
        /// ** EGM Status ** 
        /// Set the date and time of EGM Status in unix format
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        public void SetDateAndTime(int day, int month, int year, int hour, int minute, int second)
        {
            _EGMStatus.EGMLastPolledTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            _EGMStatus.EGMTime = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero).ToUnixTimeSeconds();
            SaveEGMStatus();
        }

        /// <summary>
        ///** EGM Status ** 
        /// Sets the current player denomination, in EGM Status
        /// </summary>
        /// <param name="currentDenomination"></param>
        public void SetCurrentPlayerDenomination(byte currentDenomination)
        {
            _EGMStatus.CurrentPlayerDenomination = currentDenomination;
            SaveEGMStatus();

        }


        /// <summary>
        ///  ** EGM Status **
        /// Updates the number of denominations and player denominations in EGM Status
        /// </summary>
        /// <param name="NumberOfDenominations">Número de denominaciones</param>
        /// <param name="PlayerDenominations">Denominaciones del jugador</param>
        public void SetDenominations(byte NumberOfDenominations, byte[] PlayerDenominations)
        {
            _EGMStatus.NumberOfDenominations = NumberOfDenominations;
            _EGMStatus.PlayerDenominations = PlayerDenominations;
            SaveEGMStatus();
        }

        /// <summary>
        /// ** EGM Status ** 
        /// Updates the Token Denomination of EGMStatus
        /// </summary>
        /// <param name="TokenDenomination"></param>
        public void SetTokenDenomination(byte TokenDenomination)
        {
            _EGMStatus.TokenDenomination = TokenDenomination;
            SaveEGMStatus();
        }

        /// <summary>
        ///  *** EGM Status ***
        ///  Updates the last accepted bill information in EGM Status
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="denominationCode"></param>
        /// <param name="billMeter"></param>
        public void SetLastAcceptedBillInformation(byte countryCode,
                                                   byte denominationCode,
                                                   byte[] billMeter)
        {
            _EGMStatus.LastBillInformation = join(new byte[] { countryCode, denominationCode },
                                                  billMeter);
            SaveEGMStatus();

        }

        /// <summary>
        /// *** EGM Status ***
        /// Enqueues an exception with its respective data (in case of realtime). Classifies the exception if it is priority or not and based on this classification, it puts in the priority queue or exception queue
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="exceptionData"></param>
        public void EnqueueException(byte exception, byte[] exceptionData)
        {
            bool IsPriority = false;
            switch (exception)
            {
                case 0x57:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x67:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x68:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x3F:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x6A:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x6B:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x6F:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x56:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x3D:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x3E:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x69:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x6C:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x6D:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x51:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x52:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x8F:
                    {
                        IsPriority = true;
                        break;
                    }
                case 0x70:
                    {
                        IsPriority = true;
                        break;
                    }
            }
            if (!IsPriority)
            {
                EGMException exp = new EGMException();
                exp.exception = exception;
                exp.data = exceptionData;
                _EGMStatus.eventqueue.Enqueue(exp);
                SaveEGMStatus();

            }
            else
            {
                // Check if exception exists in priority queue
                bool exception_exists_in_priority_queue = _EGMStatus.priorityeventqueue.Where(b => b.exception == exception).Count() > 0;
                // If exception doesn't exist in priority queue
                if (!exception_exists_in_priority_queue)
                {
                    // Instantiates a new exception
                    EGMException exp = new EGMException();
                    exp.exception = exception;
                    exp.data = exceptionData;
                    // Add it to priority event queue
                    _EGMStatus.priorityeventqueue.Enqueue(exp);
                    SaveEGMStatus();
                }
            }
        }

        /// <summary>
        /// *** EGM Status *** *** AFT ***
        /// Method that allows to reset the current transaction 
        public void ResetCurrentTransfer()
        {
            if (_EGMStatus.aftcollection.aftoperations.Count() > 0)
            {
                _EGMStatus.aftcollection.currentoperation = _EGMStatus.aftcollection.aftoperations.Where(op => op.TransactionDate == _EGMStatus.aftcollection.aftoperations.Max(t => t.TransactionDate)).FirstOrDefault();
            }
        }
        /// <summary>
        /// *** EGM Status *** *** AFT ***
        /// Methods that allows to add a transaction to the AFT Collection
        /// </summary>
        public void AddTransaction(byte status, byte? receiptstatus, byte[] transactionId, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte? transferFlags, byte? transferType, byte[] expiration, byte[] poolId, byte? position, DateTime? transactionDate, bool IsCurrentTransferInterrogationResponse)
        {
            if (IsCurrentTransferInterrogationResponse)
            {
                if (_EGMStatus?.aftcollection?.currentoperation?.Position == position)
                {
                    _EGMStatus.aftcollection.currentoperation.Amount = cashableAmount;
                    _EGMStatus.aftcollection.currentoperation.InternalStatus = status;
                    _EGMStatus.aftcollection.currentoperation.expiration = expiration;
                    _EGMStatus.aftcollection.currentoperation.poolID = poolId;
                    _EGMStatus.aftcollection.currentoperation.RestrictedAmount = restrictedAmount;
                    _EGMStatus.aftcollection.currentoperation.NonRestrictedAmount = nonRestrictedAmount;
                    _EGMStatus.aftcollection.currentoperation.TransactionID = transactionId;
                    _EGMStatus.aftcollection.currentoperation.transferFlags = transferFlags == null ? (byte)0x00 :  transferFlags.Value;
                    _EGMStatus.aftcollection.currentoperation.TransferType = transferType == null ? (byte)0x00 :  transferType.Value;
                    _EGMStatus.aftcollection.currentoperation.Position = position == null ? (byte)0x00 : position.Value;
                    _EGMStatus.aftcollection.currentoperation.TransactionDate = transactionDate == null ? (DateTime)DateTime.MinValue :  transactionDate.Value;
                    _EGMStatus.aftcollection.currentoperation.ReceiptStatus = receiptstatus == null ? (byte)0x00 :  receiptstatus.Value;

                }
                else
                {
                    if (_EGMStatus?.aftcollection?.currentoperation != null)
                    {
                        _EGMStatus.aftcollection.currentoperation.Amount = cashableAmount;
                        _EGMStatus.aftcollection.currentoperation.expiration = expiration;
                        _EGMStatus.aftcollection.currentoperation.poolID = poolId;
                        _EGMStatus.aftcollection.currentoperation.RestrictedAmount = restrictedAmount;
                        _EGMStatus.aftcollection.currentoperation.NonRestrictedAmount = nonRestrictedAmount;
                        _EGMStatus.aftcollection.currentoperation.TransactionID = transactionId;
                        _EGMStatus.aftcollection.currentoperation.transferFlags = transferFlags == null ? (byte)0x00 :  transferFlags.Value;
                        _EGMStatus.aftcollection.currentoperation.TransferType = transferType == null ? (byte)0x00 :  transferType.Value;
                        _EGMStatus.aftcollection.currentoperation.Position = position == null ? (byte)0x00 : position.Value;
                        _EGMStatus.aftcollection.currentoperation.InternalStatus = status;
                        _EGMStatus.aftcollection.currentoperation.TransactionDate = transactionDate == null ? (DateTime)DateTime.MinValue :  transactionDate.Value;
                        _EGMStatus.aftcollection.currentoperation.ReceiptStatus = receiptstatus == null ? (byte)0x00 :  receiptstatus.Value;

                    }
                    else
                    {
                          AFTOperation op = new AFTOperation();
                            op.Amount = cashableAmount;
                            op.expiration = expiration;
                            op.poolID = poolId;
                            op.RestrictedAmount = restrictedAmount;
                            op.NonRestrictedAmount = nonRestrictedAmount;
                            op.TransactionID = transactionId;
                            op.transferFlags = transferFlags == null ? (byte)0x00 :  transferFlags.Value;
                            op.TransferType = transferType == null ? (byte)0x00 :  transferType.Value;
                            op.Position = position == null ? (byte)0x00 : position.Value;
                            op.InternalStatus = status;
                            op.TransactionDate = transactionDate == null ? (DateTime)DateTime.MinValue :  transactionDate.Value;
                            op.ReceiptStatus = receiptstatus == null ? (byte)0x00 :  receiptstatus.Value;

                            _EGMStatus.aftcollection.currentoperation = op;
                    }
                  
                }
       
                                 
                SaveEGMStatus();
            }
            _EGMStatus.aftcollection.AddTransaction(status, receiptstatus, transactionId, cashableAmount, restrictedAmount, nonRestrictedAmount, transferFlags, transferType, expiration, poolId, position, transactionDate);
        }


        /// <summary>
        /// *** EGM Status *** *** AFT ***
        /// Updates the registration info in EGMStatus
        /// </summary>
        public void UpdateRegInfo(byte regStatus, byte[] registrationKey, byte[] regPosId)
        {
            bool saveegmstatus = false;
            /** registration Status **/

            if (_EGMStatus.regStatus != regStatus)
            {
                //  registration Status
                _EGMStatus.regStatus = regStatus;
                saveegmstatus = true;
            }

            /** registration Key **/

            _EGMStatus.regKey = _EGMStatus.regKey == null ? new byte[] { } : _EGMStatus.regKey;

            if (!_EGMStatus.regKey.SequenceEqual(registrationKey))
            {
                string crrntregKeySTR = BitConverter.ToString(_EGMStatus.regKey);
                string newregKeySTR = BitConverter.ToString(registrationKey);
                try { LaunchLog(new string[] { "EGM ->" }, $"Registration Key updated: FROM {(crrntregKeySTR == "" ? "Empty" : crrntregKeySTR)} TO {(newregKeySTR == "" ? "Empty" : newregKeySTR)}", new EventArgs()); } catch { }
                _EGMStatus.regKey = registrationKey;
                saveegmstatus = true;
            }

            /** registration Key **/

            _EGMStatus.regPOSId = _EGMStatus.regPOSId == null ? new byte[] { } : _EGMStatus.regPOSId;

            if (!_EGMStatus.regPOSId.SequenceEqual(regPosId))
            {
                _EGMStatus.regPOSId = regPosId;
                saveegmstatus = true;
            }

            if (saveegmstatus)
                SaveEGMStatus();
        }


        /// <summary>
        /// ** EGM Settings and EGM Info ** *** AFT ***
        /// 74 Response handler from VirtualEGMController (at MainController).
        /// When a long poll 74 arrives to Host and processed by MainController, the vegm controller calls this method  to persist its parameters to EGMSettings and EGMStatus
        /// </summary>
        /// <param name="assetNumber">Into EGMSettings</param>
        /// <param name="availableTransfers">Into EGMSettings with specific mask</param>
        /// <param name="gameLockStatus">Into EGMStatus</param>
        /// <param name="hostCashoutStatus">Into EGMStatus</param>
        /// <param name="aftStatus">Into EGMStatus with specific mask</param>
        /// <param name="restrictedExpiration">Into EGMStatus</param>
        /// <param name="gmTransferLimit">Into EGMSettings</param>
        /// <param name="gmMaxBufferIndex">Into EGMSettings</param>
        /// <param name="currentRestrictedAmount">Into EGMStatus</param>
        /// <param name="currentNonRestrictedAmount">Into EGMStatus</param>
        /// <param name="restrictedPoolID">Into EGMSettings</param>
        public void Update74ResponseInfo(byte[] assetNumber, byte availableTransfers, byte gameLockStatus, byte hostCashoutStatus, byte aftStatus, byte[] restrictedExpiration, byte[] gmTransferLimit, byte gmMaxBufferIndex, byte[] currentCashableAmount, byte[] currentRestrictedAmount, byte[] currentNonRestrictedAmount, byte[] restrictedPoolID)
        {
            bool saveegmsettings = false;
            bool saveegmstatus = false;

            /** AssetNumber **/
            if (!_EGMSettings.assetId.SequenceEqual(assetNumber))
            {
                // Asset Number
                _EGMSettings.assetId = assetNumber;
                saveegmsettings = true;
            }

            /** Available Transfers **/
            byte Modified_availableTransfers = 0x0;
            // 0 = Transfer to gaming machine OK
            // 1 = Transfer from gaming machine OK
            // 2 = Transfer to printer OK
            // 3 = Win amount pending cashout to host 
            // 4 =  Bonus award to gaming machine OK
            // 65 = TBD (leave as 0)
            // 7 = Lock After Transfer request supported
            Modified_availableTransfers = (byte)(0 | GetByteWithPos(availableTransfers, 7) | GetByteWithPos(availableTransfers, 6) | GetByteWithPos(availableTransfers, 5) | GetByteWithPos(availableTransfers, 4) | GetByteWithPos(availableTransfers, 3) | GetByteWithPos(availableTransfers, 1) | GetByteWithPos(availableTransfers, 0));
            if (_EGMStatus.availableTransfers != Modified_availableTransfers)
            {
                // Available Transfers
                _EGMStatus.availableTransfers = Modified_availableTransfers;
                saveegmstatus = true;
            }

            /** Game Lock Status **/

            if (_EGMStatus.gameLockStatus != gameLockStatus)
            {
                //  Game Lock Status
                _EGMStatus.gameLockStatus = gameLockStatus;
                saveegmstatus = true;
            }

            /** Host Cashout Status **/

            if (_EGMStatus.hostCashoutStatus != hostCashoutStatus)
            {
                // Host Cashout Status
                _EGMStatus.hostCashoutStatus = hostCashoutStatus;
                saveegmstatus = true;
            }


            /** AFT Status **/
            byte Modified_aftStatus = 0x0;
            // 0 = Printer available for transaction receipts
            // 1 = Transfer to host of less than full available amount allowed
            // 2 = Custom ticket data supported 
            // 3 = AFT registered
            // 4 = In-house transfers enabled 
            // 5 = Bonus transfers enabled
            // 6 = Debit transfers enabled
            // 7 = Any AFT enabled 
            Modified_aftStatus = (byte)(0 | GetByteWithPos(aftStatus, 7) | GetByteWithPos(aftStatus, 4) | GetByteWithPos(aftStatus, 3));
            if (_EGMStatus.aftStatus != Modified_aftStatus)
            {
                // AFT Status
                _EGMStatus.aftStatus = Modified_aftStatus;
                saveegmstatus = true;
            }

            /** Restricted Expiration **/

            if (!_EGMStatus.restrictedExpiration.SequenceEqual(restrictedExpiration))
            {
                // Restricted Expiration
                _EGMStatus.restrictedExpiration = restrictedExpiration;
                saveegmstatus = true;
            }

            /** Game Machine Transfer Limit **/
            if (!_EGMSettings.gmTransferLimit.SequenceEqual(gmTransferLimit))
            {
                // Game Machine Transfer Limit
                _EGMSettings.gmTransferLimit = gmTransferLimit;
                LaunchLog(new string[] { "EGM ->"}, $"_EGMSettings.gmTransferLimit {BitConverter.ToString(_EGMSettings.gmTransferLimit)}", new EventArgs());
                saveegmsettings = true;
            }

            /** Game Machine Buffer Index **/
            if (_EGMSettings.gmMaxBufferIndex != gmMaxBufferIndex)
            {
                // Game Machine Buffer Index
                _EGMSettings.gmMaxBufferIndex = gmMaxBufferIndex;
                _EGMStatus.aftcollection.maxBufferSize = gmMaxBufferIndex;
                saveegmsettings = true;
            }

            /** Current Cashable Amount **/
            if (!_EGMStatus.currentCashableAmount.SequenceEqual(currentCashableAmount))
            {
                // Cashable Amount
                _EGMStatus.currentCashableAmount = currentCashableAmount;
                saveegmstatus = true;
            }

            /** Current Restricted Amount **/

            if (!_EGMStatus.currentRestrictedAmount.SequenceEqual(currentRestrictedAmount))
            {
                // Restricted Expiration
                _EGMStatus.currentRestrictedAmount = currentRestrictedAmount;
                saveegmstatus = true;
            }

            /** Current Non Restricted Amount **/

            if (!_EGMStatus.currentNonRestrictedAmount.SequenceEqual(currentNonRestrictedAmount))
            {
                // Restricted Expiration
                _EGMStatus.currentNonRestrictedAmount = currentNonRestrictedAmount;
                saveegmstatus = true;
            }

            /** Restricted Pool ID **/
            if (!_EGMSettings.restrictedPoolID.SequenceEqual(restrictedPoolID))
            {
                _EGMSettings.restrictedPoolID = restrictedPoolID;
                saveegmsettings = true;
            }



            if (saveegmsettings)
                SaveEGMSettings();

            if (saveegmstatus)
                SaveEGMStatus();
        }

        /// <summary>
        /// **** EGM Info and EGM Settings ****
        /// Updates all game information in EGM Info and EGM Settings
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="additionalId"></param>
        /// <param name="denomination"></param>
        /// <param name="maxBet"></param>
        /// <param name="progressiveGroup"></param>
        /// <param name="gameOptions"></param>
        /// <param name="paytableId"></param>
        /// <param name="basePercentage"></param>
        public void UpdateGamingInfo_GameID(string gameId,
                                            string additionalId,
                                            byte denomination,
                                            byte maxBet,
                                            byte progressiveGroup,
                                            byte[] gameOptions,
                                            string paytableId,
                                            string basePercentage)
        {
            _EGMInfo.GameID = gameId;
            _EGMInfo.AdditionalID = additionalId;
            _EGMInfo.BasePercentage = basePercentage;
            _EGMInfo.Denomination = denomination;
            _EGMSettings.GameOptions = gameOptions;
            _EGMSettings.MaxBet = maxBet;
            _EGMInfo.PayTableID = paytableId;
            _EGMSettings.ProgressiveGroup = progressiveGroup;
            SaveEGMInfo();
            SaveEGMSettings();
        }

        /// <summary>
        /// ** EGM Info and EGM Settings **
        /// Updates all game information at game level in EGM Info and EGM Settings
        /// </summary>
        public void UpdateGamingNInfo(byte[] gameNumber,
                                      byte[] maxBet,
                                      byte progressiveGroup,
                                      byte[] progressiveLevels,
                                      byte gameNameLength,
                                      byte[] gameName,
                                      byte paytableLength,
                                      byte[] paytableName,
                                      byte[] wagerCategories)
        {
            GameInfo gameInfo = _EGMInfo.GetGameInfo(gameNumber);
            GameSettings gameSettings = _EGMSettings.GetGameSettings(gameNumber);

            gameInfo.gameName = System.Text.Encoding.ASCII.GetString(gameName);
            gameInfo.paytableName = System.Text.Encoding.ASCII.GetString(paytableName);
            gameInfo.wagerCategories = wagerCategories;

            SaveEGMInfo();

            gameSettings.maxBet = maxBet;
            gameSettings.progressiveGroup = progressiveGroup;
            gameSettings.progressiveLevels = progressiveLevels;

            SaveEGMSettings();
        }


        /// <summary>
        /// ** EGM Settings **
        /// Updates the EnabledGameNumbers field of EGM Settings
        /// </summary>
        /// <param name="EnabledGames"></param>
        public void SetEnabledGameNumbers(List<byte[]> EnabledGames)
        {
            _EGMSettings.EnabledGameNumbers = EnabledGames;
            SaveEGMSettings();

        }

        /// <summary>
        /// ** EGM Settings **
        /// Updates the cashout limit on EGM Settings, at game settings provided by parameter
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="cashoutLimit"></param>
        public void UpdateCashoutLimit(byte[] gameNumber, byte[] cashoutLimit)
        {
            _EGMSettings.GetGameSettings(gameNumber).CashoutLimit = cashoutLimit;
            SaveEGMSettings();
        }

        /// <summary>
        /// ** EGM Settings **
        /// Allows set the realtime on client. Updates this property in EGM Settings
        /// </summary>
        /// <param name="enable_disable"></param>
        public void SetEnabledRealTime(byte enable_disable)
        {
            if (enable_disable == 0x00)
                _EGMSettings.EnabledRealTime = false;
            else if (enable_disable == 0x01)
                _EGMSettings.EnabledRealTime = true;
            _client.UpdateEnabledRealTime(_EGMSettings.EnabledRealTime);
            SaveEGMSettings();

        }


        /// <summary>
        /// *** EGM Settings *** *** VALIDATION ***
        /// Set the extended validation status on EGM Settings
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="denominationCode"></param>
        /// <param name="billMeter"></param>
        /// <param name="restrictedTicketDefaultExpiration"></param>
        public void SetExtendedValidationStatus(byte[] assetNumber,
                                                byte[] statusBits,
                                                byte[] cashableTicketAndReceiptExpiration,
                                                byte[] restrictedTicketDefaultExpiration)
        {
            // Asset Number
            _EGMSettings.assetId = assetNumber;
            // Los estados. // The States
            // ASUMO: Por cada bit de estado, si la máscara de control me habilita a actualizar el valor // I assume: For each state bit, if the control mask allows me to update the value
            // Habilito o deshabilito la función a travès de lo que me venga como parámetro en statusBitControlStates // I enable or disable the function through what comes to me as a parameter in statusBitControlStates
            _EGMSettings.statusBitControlStates = statusBits;
            // Expiración del ticket y el receipt // Expiration of the ticket and the receipt
            _EGMSettings.cashableTicketAndReceiptExpiration = cashableTicketAndReceiptExpiration;
            // Expiración del default ticket // Expiration of the default ticket
            _EGMSettings.restrictedTicketDefaultExpiration = restrictedTicketDefaultExpiration;

            SaveEGMSettings();

        }

        /// <summary>
        /// ** EGM Settings ** ** VALIDATION **
        /// Sets the validation type of EGM Settings based on validation type passed as parameter
        /// </summary>
        /// <param name="validationType"></param>
        public void SetValidationType(int validationType)
        {
            if (validationType == EGMValidationType.System.GetHashCode())
                _EGMSettings.ValidationType = EGMValidationType.System;
            if (validationType == EGMValidationType.Enhanced.GetHashCode())
                _EGMSettings.ValidationType = EGMValidationType.Enhanced;
            SaveEGMSettings();
        }

        /// <summary>
        /// *** EGM Settings *** *** VALIDATION ***
        /// Método que setea true o false a la propiedad ValidationExtensions de la EGM Settings si está habilitado el mismo o no en la EGM respectivamente // Method that sets true or false to the ValidationExtensions property of the EGM Settings if it is enabled or not in the EGM respectively
        /// </summary>
        /// <param name="b"></param>
        public void SetValidationExtensions(bool b)
        {
            _EGMSettings.ValidationExtensions = b;
            SaveEGMSettings();
        }

        /// <summary>
        /// *** EGM Settings *** *** VALIDATION ***
        /// Setting of features of VirtualEGM. Forced setting for some bits in zero, The rest come from EGM
        /// Setting of features of PhysicalEGM. For a better understanding, we take the following order: 01234567
        /// </summary>
        /// <param name="features1"></param>
        /// <param name="features2"></param>
        /// <param name="features3"></param>
        public void SetFeatures(byte features1, byte features2, byte features3)
        {
            // 00000000 | 00000001 | 00000010 | 00000100 | 00001000 | 10000000 = 10001111
            // 0 = Jackpot multiplier
            // 1 = AFT bonus awards
            // 2 = Legacy bonus awards
            // 3 = Tournament
            // 4 = Validation Extensions
            // 65 = Validation Style
            // 7 = Ticket Redemption
            _EGMSettings.features1 = (byte)(0 | GetByteWithPos(features1, 7) | GetByteWithPos(features1, 6) | GetByteWithPos(features1, 5) | GetByteWithPos(features1, 4) | GetByteWithPos(features1, 2) | GetByteWithPos(features1, 0));
            // 00000000 | 00100000 | 01000000 | 10000000 = 11100000
            // 10 = Meter model flag
            // 2 = Tickets to total drop and total cancelled credits
            // 3 = Extended meters
            // 4 = Component Authentication
            // 5 = Reserved
            // 6 = Advanced Funds Transfer
            // 7 = Multi-denom extensions
            _EGMSettings.features2 = (byte)(0 | GetByteWithPos(features2, 6) | GetByteWithPos(features2, 2) | GetByteWithPos(features2, 1) | GetByteWithPos(features2, 0));
            // 00000000
            // 0 = Maximum polling rate
            // 1 = Multiple SAS progressive win reporting
            // 2-7 = Reserved
            _EGMSettings.features3 = 0x0;

            SaveEGMSettings();
        }

        #endregion

        #region Control


        /// <summary>
        /// Sends its self address with the parity bit in 1, i.e, a single chirp
        /// </summary>
        public void Chirp()
        {
            _client.Chirp();
        }


        /// <summary>
        /// *** AFT ***
        /// Get the Registration Key of AFT
        /// </summary>
        /// <returns></returns>
        public byte[] GetRegistrationKey()
        {
            byte[] regKey = _EGMStatus.regKey == null ? new byte[] { } : _EGMStatus.regKey;
            return regKey;
        }

        /// <summary>
        /// *** PART OF THE AFT PROCESS ***
        /// Set transfer in progress if parameter is true, or transfer finished or stopped, if parameter is false
        /// It is used by VirtualEGMController
        /// </summary>
        public void SetTransferInProgress(bool truth)
        {
            TransferInProgress = truth;
            if (truth == false)
            {
                Timer69.Stop();
            }
        }

        /// <summary>
        /// ***** PART OF HANDPAY PROCESS ******
        /// Returns a boolean that determines if there is a handpay in process
        /// Devuelve un booleano que determina que hay un handpay en proceso
        /// </summary>
        /// <returns>true if there is a handpay in process, false othersie.</returns>
        public bool AHandpayInProcess()
        {
            return HandpayInProcess;
        }

        /// <summary>
        /// Instantiates all main structures in this VirtualEGM, like EGM Accounting, EGM Settings, EGM Status and EGMInfo, that are persisted. Also, starts the client in the serial port passed as argument
        /// </summary>
        /// <param name="port">Client port</param>
        public void StartVirtualEGM(string port)
        {
            LaunchLog(new string[] { }, $"Reading VEGM Files", new EventArgs());
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();

            // La EGMAccounting, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMAccounting, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMAccounting = xmlfile_serializer.Deserialize<EGMAccounting>("EGMAccounting.xml");
                _EGMAccounting.SetGames(false);
            }
            catch
            {
                _EGMAccounting = new EGMAccounting();
                _EGMAccounting.SetGames(false);
                xmlfile_serializer.SaveXml<EGMAccounting>(_EGMAccounting, "EGMAccounting.xml");
            }

            // La EGMSettings, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMSettings, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMSettings = xmlfile_serializer.Deserialize<EGMSettings>("EGMSettings.xml");
            }
            catch
            {
                _EGMSettings = new EGMSettings();
                xmlfile_serializer.SaveXml<EGMSettings>(_EGMSettings, "EGMSettings.xml");
            }

            // La EGMStatus, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMStatus, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMStatus = xmlfile_serializer.Deserialize<EGMStatus>("EGMStatus.xml");
                _EGMStatus.Handpayqueue = _EGMStatus.ToConcurrentQueue(_EGMStatus.HandpayqueueList);
                _EGMStatus.eventqueue = _EGMStatus.ToConcurrentQueue(_EGMStatus.eventqueueList);
                _EGMStatus.priorityeventqueue = _EGMStatus.ToConcurrentQueue(_EGMStatus.priorityeventqueueList);
                // AFTOperation lastop = _EGMStatus.aftcollection.GetLastOperation();
                // if (lastop != null)
                // {
                //     if (lastop.Acknowledged == false && lastop.InternalStatus == 0x00)
                //     {
                //         Timer69.Start();
                //     }
                // }
                HandpayInProcess = (_EGMStatus.Handpayqueue.Where(hp_ => hp_.status == HandpaySMStatus.HandpayOcurred).Count() > 0);
                if (!AllAcknoledged())
                {
                    try { Timer51.Start(); } catch { }

                }
            }
            catch
            {
                _EGMStatus = new EGMStatus();
                xmlfile_serializer.SaveXml<EGMStatus>(_EGMStatus, "EGMStatus.xml");
            }
            // La EGMInfo, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMInfo, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMInfo = xmlfile_serializer.Deserialize<EGMInfo>("EGMInfo.xml");
            }
            catch
            {
                _EGMInfo = new EGMInfo();
                xmlfile_serializer.SaveXml<EGMInfo>(_EGMInfo, "EGMInfo.xml");
            }
            LaunchLog(new string[] { }, $"Starting Client", new EventArgs());
            _client.Start(port);
        }


        /// <summary>
        /// Stops the Virtual EGM by stopping the continous reading of client
        /// </summary>
        public void StopVirtualEGM()
        {
            _client.Stop();
        }

        /// <summary>
        /// Disables the Virtual EGM. But the reading from Smib still works
        /// </summary>
        public void DisableVirtualEGM()
        {
            _client.DisableClient();
        }

        /// <summary>
        /// Enables la VirtualEGM.
        /// </summary>
        public void EnableVirtualEGM()
        {
            _client.EnableClient();
        }

        /// <summary>
        /// VirtualEGM Constructur
        /// </summary>
        /// <param name="print">This boolean indicates when print logs at sending or receaving a long polls</param>
        public VirtualEGM(bool print)
        {
            // Timer51: Tiempo máximo para lanzar la excepción 51 // Maximum time to launch the 51 exception
            Timer51 = new System.Timers.Timer(15000);
            Timer51.Elapsed += SendException51;

            // Timer3D: Tiempo máximo para lanzar la excepción 3D // Maximum time to launch the 3D exception
            Timer3D = new System.Timers.Timer(15000);
            Timer3D.Elapsed += SendException3D;

            // Timer3E: Tiempo máximo para lanzar la excepción 3E // Maximum time to launch the 3E exception
            Timer3E = new System.Timers.Timer(15000);
            Timer3E.Elapsed += SendException3E;

            // Timer67: Tiempo máximo para lanzar la excepción 67 // Maximum time to launch the 67 exception
            Timer67 = new System.Timers.Timer(10000);
            Timer67.Elapsed += SendException67;

            // Timer68: Tiempo máximo para lanzar la excepción 68 // Maximum time to launch the 68 exception
            Timer68 = new System.Timers.Timer(15000);
            Timer68.Elapsed += SendException68;

            // Timer69: Tiempo máximo para lanzar la excepción 69 // Maximum time to launch the 69 exception
            Timer69 = new System.Timers.Timer(15000);
            Timer69.Elapsed += SendException69;

            _client = new SASClient(print);

            RegisterMethods();

        }


        #endregion


        #endregion


        // Persisto los cambios no guardados en los xml // Persist the changes not saved in the xml

        private void SaveEGMSettings()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMSettings>(_EGMSettings, "EGMSettings.xml");
        }

        private void SaveEGMStatus()
        {
            _EGMStatus.HandpayqueueList = _EGMStatus.Handpayqueue.ToList();
            _EGMStatus.eventqueueList = _EGMStatus.eventqueue.ToList();
            _EGMStatus.priorityeventqueueList = _EGMStatus.priorityeventqueue.ToList();

            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMStatus>(_EGMStatus, "EGMStatus.xml");
        }

        private void SaveEGMAccounting()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMAccounting>(_EGMAccounting, "EGMAccounting.xml");

        }

        private void SaveEGMInfo()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMInfo>(_EGMInfo, "EGMInfo.xml");

        }


    }
}
