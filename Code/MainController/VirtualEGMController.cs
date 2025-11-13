using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace MainController
{

    /// <summary>
    /// Interactúa con la VirtualEGM, perteneciente al módulo BitbossInterface. El propósito de esta clase es ser mediador entre la VirtualEGM y el MainController, como por ejemeplo
    /// capturar eventos de la VirtualEGM y avisarle al MainController, escuchar al MainController y redirigir pedidos a la VirtualEGM. Vale aclarar que la VirtualEGM es la que contiene al client
    /// el cual tiene conexión con la SMIB y/o SASTest
    ///
    /// It interacts with the VirtualEGM, belonging to the BitbossInterface module. The purpose of this class is to mediate between the VirtualEGM and the MainController, such as 
    /// capturing events from the VirtualEGM and alerting the MainController, listen to the MainController and redirect requests to the VirtualEGM. It is worth noting that the VirtualEGM is the one that contains the client
    /// which has a connection to the SMIB and/or SASTest
    /// </summary>
    public class VirtualEGMBehaviourController
    {

        #region ELEMENTOS DEL CONTROLADOR O HANDLER DE LA PHYSICALEGM
        /// <summary>
        ///  A VirtualEGM persisting all data and communicates with Client
        /// </summary>
        private VirtualEGM virtualEGM;
        /// <summary>
        /// Un módulo que gestiona la persistencia de trazabilidad de envíos y recepciones
        /// A module that manages the traceability persistence of shipments and receipts.
        /// </summary>
        public LogTracer logTracer;
        /// <summary>
        /// The serial port for virtualEGM client
        /// </summary>
        private string s_port = "";

        #endregion

        /*****************************************************************************/
        /*****************************************************************************/
        /*****************************************************************************/
        /**************************VIRTUALEGM EVENTS (ORDER BY CODE)*****************/
        /*****************************************************************************/
        /*****************************************************************************/
        /*****************************************************************************/

        #region VIRTUAL EGM EVENTS


        #region "Long polls requests for interface"

        public event VirtualEGMLP01Handler VirtualLP01;
        public delegate void VirtualEGMLP01Handler(EventArgs e);

        public event VirtualEGMLP02Handler VirtualLP02;
        public delegate void VirtualEGMLP02Handler(EventArgs e);

        public event VirtualEGMLP03Handler VirtualLP03;
        public delegate void VirtualEGMLP03Handler(EventArgs e);

        public event VirtualEGMLP04Handler VirtualLP04;
        public delegate void VirtualEGMLP04Handler(EventArgs e);

        public event VirtualEGMLP06Handler VirtualLP06;
        public delegate void VirtualEGMLP06Handler(EventArgs e);

        public event VirtualEGMLP07Handler VirtualLP07;
        public delegate void VirtualEGMLP07Handler(EventArgs e);

        public event VirtualEGMLP08Handler VirtualLP08;
        public delegate void VirtualEGMLP08Handler(byte[] billDenominations, byte billAcceptorFlag, EventArgs e);

        public event VirtualEGMLP0EHandler VirtualLP0E;
        public delegate void VirtualEGMLP0EHandler(byte enable_disable, EventArgs e);

        public event VirtualEGMLP2EHandler VirtualLP2E;
        public delegate void VirtualEGMLP2EHandler(byte[] bufferAmount, EventArgs e);

        public event VirtualEGMLP4CHandler VirtualLP4C;
        public delegate void VirtualEGMLP4CHandler(byte[] machineID, byte[] sequenceNumber, EventArgs e);

        public event VirtualEGMLP21Handler VirtualLP21;
        public delegate void VirtualEGMLP21Handler(byte[] seedValue, EventArgs e);

        public event VirtualEGMLP4DEndValidationHandler VirtualLP4DEndValidation;
        public delegate void VirtualEGMLP4DEndValidationHandler(EventArgs e);

        public event VirtualEGMLP58Handler VirtualLP58;
        public delegate void VirtualEGMLP58Handler(byte validationSystemId, byte[] validationNumber, EventArgs e);

        public event VirtualEGMLP71Handler VirtualLP71;
        public delegate void VirtualEGMLP71Handler(byte transferCode, int amount, byte parsingCode, byte[] validationData, EventArgs e);

        public event VirtualEGMLP72Handler VirtualLP72;
        public delegate void VirtualEGMLP72Handler(byte transferCode, byte[] cashableamount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte[] transactionID, byte[] registrationKey, byte[] expiration, EventArgs e);

        public event VirtualEGMLP72InterrogationHandler VirtualLP72Interrogation;
        public delegate void VirtualEGMLP72InterrogationHandler(EventArgs e);

        public event VirtualEGMLP73Handler VirtualLP73;
        public delegate void VirtualEGMLP73Handler(byte registrationCode, byte[] registrationKey, byte[] posId, EventArgs e);

        public event VirtualEGMLP74Handler VirtualLP74;
        public delegate void VirtualEGMLP74Handler(byte lockCode, byte lockCondition, byte[] lockTimeout, EventArgs e);

        public event VirtualEGMLP7CHandler VirtualLP7C;
        public delegate void VirtualEGMLP7CHandler(byte code, string data, EventArgs e);

        public event VirtualEGMLP7DHandler VirtualLP7D;
        public delegate void VirtualEGMLP7DHandler(byte[] HostId,
                                                   byte expiration,
                                                   byte[] location,
                                                   byte[] address1,
                                                   byte[] address2, EventArgs e);

        public event VirtualEGMLP7FHandler VirtualLP7F;
        public delegate void VirtualEGMLP7FHandler(byte[] date, byte[] time, EventArgs e);


        public event VirtualEGMLP80Handler VirtualLP80;
        public delegate void VirtualEGMLP80Handler(bool broadcast, byte group, byte level, byte[] amount, EventArgs e);


        public event VirtualEGMLP86Handler VirtualLP86;
        public delegate void VirtualEGMLP86Handler(bool broadcast, byte group, List<Tuple<byte, byte[]>> amountsAndLevels, EventArgs e);

        public event VirtualEGMLP8AHandler VirtualLP8A;
        public delegate void VirtualEGMLP8AHandler(byte[] bonusAmount, byte taxStatus, EventArgs e);

        public event VirtualEGMLP94Handler VirtualLP94;
        public delegate void VirtualEGMLP94Handler(EventArgs e);

        public event VirtualEGMLPA8Handler VirtualLPA8;
        public delegate void VirtualEGMLPA8Handler(byte resetMethod, EventArgs e);

        public event VirtualEGMCommunicationDownEventHandler VirtualEGMCommunicationDownEvent;
        public delegate void VirtualEGMCommunicationDownEventHandler(bool truth, EventArgs e);

        public event LaunchLogHandler LaunchLog; 
        public delegate void LaunchLogHandler(string[] tags, string message, EventArgs e);

        #endregion


        #endregion


        /*****************************************************************************/
        /*****************************************************************************/
        /*****************************************************************************/
        /******************************    INIT      *********************************/
        /*****************************************************************************/
        /*****************************************************************************/
        /*****************************************************************************/

        #region INIT
        /// <summary>
        /// Registración de métodos y/o eventos
        /// Registration of methods and/or events: Register the events of VirtualEGM events into local handlers
        /// It is called at initialize_host method
        /// </summary>
        void RegisterMethods()
        {
            // Suscribimos los distintos eventos o funciones 
            // We subscribe to the different events or functions 
            // Suscribimos los distintos eventos o funciones 
            // We subscribe to the different events or functions 
            virtualEGM.CommandSent += new VirtualEGM.CommandSentHandler(cmdSent);
            virtualEGM.CommandReceived += new VirtualEGM.CommandReceivedHandler(cmdReceived);
            virtualEGM.SmibLinkDown += new VirtualEGM.SmibLinkDownHandler((b, e) => VirtualEGMCommunicationDownEvent(b, e));
            virtualEGM.LP01 += new VirtualEGM.LP01Handler(e => VirtualLP01(e));
            virtualEGM.LP02 += new VirtualEGM.LP02Handler(e => VirtualLP02(e));
            virtualEGM.LP03 += new VirtualEGM.LP03Handler(e => VirtualLP03(e));
            virtualEGM.LP04 += new VirtualEGM.LP04Handler(e => VirtualLP04(e));
            virtualEGM.LP06 += new VirtualEGM.LP06Handler(e => VirtualLP06(e));
            virtualEGM.LP07 += new VirtualEGM.LP07Handler(e => VirtualLP07(e));
            virtualEGM.LP08 += new VirtualEGM.LP08Handler((bd, baf, e) => VirtualLP08(bd, baf, e));
            virtualEGM.LP0E += new VirtualEGM.LP0EHandler((ed, e) => VirtualLP0E(ed, e));
            virtualEGM.LP2E += new VirtualEGM.LP2EHandler((ba, e) => VirtualLP2E(ba, e));
            virtualEGM.LP4C += new VirtualEGM.LP4CHandler((mid, seqn, e) => VirtualLP4C(mid, seqn, e));
            virtualEGM.LP21 += new VirtualEGM.LP21Handler((s, e) => VirtualLP21(s, e));
            virtualEGM.LP4D += new VirtualEGM.LP4DHandler(SendValidationTicket);
            VirtualEGM.LP4DResponseSuccessful += new VirtualEGM.LP4DResponseSuccessfulHandler(e => VirtualLP4DEndValidation(e));
            virtualEGM.LP58 += new VirtualEGM.LP58Handler((vsi, vn, e) => VirtualLP58(vsi, vn, e));
            virtualEGM.LP71 += new VirtualEGM.LP71Handler((tc, a, pc, vd, e) => VirtualLP71(tc, a, pc, vd, e));
            virtualEGM.LP7C += new VirtualEGM.LP7CHandler((c, d, e) => VirtualLP7C(c, d, e));
            virtualEGM.LP7D += new VirtualEGM.LP7DHandler((hi, exp, l, ad1, ad2, e) => VirtualLP7D(hi, exp, l, ad1, ad2, e));
            virtualEGM.LP7F += new VirtualEGM.LP7FHandler((d, t, e) => VirtualLP7F(d, t, e));
            virtualEGM.LP80 += new VirtualEGM.LP80Handler((b, g, l, a, e) => VirtualLP80(b, g, l, a, e));
            virtualEGM.LP86 += new VirtualEGM.LP86Handler((b, g, aandl, e) => VirtualLP86(b, g, aandl, e));
            virtualEGM.LP8A += new VirtualEGM.LP8AHandler((ba, ts, e) => VirtualLP8A(ba, ts, e));
            virtualEGM.LP94 += new VirtualEGM.LP94Handler(e => VirtualLP94(e));
            virtualEGM.LPA8 += new VirtualEGM.LPA8Handler((r, e) => VirtualLPA8(r, e));
            virtualEGM.LP72 += new VirtualEGM.LP72Handler((tc, ca, ra, nra, tid, rk, exp, e) => VirtualLP72(tc, ca, ra, nra, tid, rk, exp, e));
            virtualEGM.LP72Interrogation += new VirtualEGM.LP72InterrogationHandler(e => VirtualLP72Interrogation(e));
            virtualEGM.LP73 += new VirtualEGM.LP73Handler((rc, rk, pid, e) => VirtualLP73(rc, rk, pid, e));
            virtualEGM.LP74 += new VirtualEGM.LP74Handler((lc, lcond, lt, e) => VirtualLP74(lc, lcond, lt, e));
            virtualEGM.LaunchLog += new VirtualEGM.LaunchLogHandler((t, m, e) => LaunchLog(t,m,e));
        }

        /// <summary>
        /// Función que devuelve el Trace config, el modo en el que el controller deja logs
        /// Function that returns the Trace config, the way in which the controller leaves logs.
        /// </summary>
        /// <returns></returns>
        private string getTraceConfig()
        {
            // Crear la configuration desde la appsettings.json
            // Create the configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            // Inicializo el logtracer, con la propiedad SASTraceMode de la configuración
            // I initialize the logtracer, with the SASTraceMode property of the configuration
            string conf = configuration["SASTraceMode"];
            if (conf == "elastic-search")
            {
                conf = "client-elastic-search";
            }
            else if (conf == "api")
            {
                conf = "client-api";
            }
            return conf;
        }



        /// <summary>
        ///  Inicializamos el handler Live Trace
        ///  We initialize the Live Trace handler
        /// </summary>
        public void CreateLiveTraceController()
        {
            logTracer = new LogTracer(getTraceConfig());
            logTracer.Logger.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainController.VirtualEGMBehaviourController"/> class.
        /// </summary>
        /// <param name="port">Port.</param>
        public VirtualEGMBehaviourController(string port)
        {
            // Inicializo la VirtualEGM , suscribiendo los siguientes eventos a las distintas subrutinas de Program.
            // Es decir, VirtualEGM interfacea los siguientes longpolls
            //
            // I initialize the VirtualEGM , subscribing the following events to the different subroutines of Program.
            // That is, VirtualEGM interfaces the following longpolls
            virtualEGM = new VirtualEGM(false);


            s_port = port;

            RegisterMethods();



        }


        #endregion


        /***************************************************************************************/
        /***************************************************************************************/
        /***************************************************************************************/
        /******************************    VIRTUALEGM EVENT HANDLERS      **********************/
        /***************************************************************************************/
        /***************************************************************************************/
        /***************************************************************************************/

        #region EVENT HANDLERS
        /// <summary>
        ///  Un comando fué enviado a la EGM. La data consta del comando, si cumple con crc y si se envía como retry
        ///  A command was sent to the EGM. The data consists of the command, whether it complies with crc and whether it is sent as a retry.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="crc"></param>
        /// <param name="isRetry"></param>
        /// <param name="e"></param>
        void cmdSent(string cmd, bool crc, bool isRetry, EventArgs e) {
            if (logTracer == null || logTracer.Logger == null)
                return;
            var split = cmd.Split("-");
            if (split.Length > 0 && cmd != "00" && cmd != "01") {
                if (split.Length >= 3
                    && split[1] == "FF" && split[2] == "00") {
                    return;
                }
                logTracer.Logger.AddTrace(cmd, "Sent", crc, isRetry);
            }
        }


        /// <summary>
        /// Un comando fué recibido a la EGM
        /// A command was received at the EGM
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="crc"></param>
        /// <param name="e"></param>
        void cmdReceived(string cmd, bool crc, EventArgs e)
        {
            if (logTracer == null || logTracer.Logger == null)
                return;
            if (cmd.Split("-").Length > 0 && cmd != "80" && cmd != "81" && cmd != "82") {
                // Console.WriteLine(cmd);
                logTracer.Logger.AddTrace(cmd, "Received", crc, false);
            }
        }

        /// <summary>
        /// Send Validation Ticket
        /// It is used when a long poll 4D arrives to Client, processed and raised by VirtualEGM
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="e"></param>
        public void SendValidationTicket(byte functionCode, EventArgs e)
        {
            Validation v = InterfacedValidation.Instance().GetValidation(functionCode);
            if (v != null)
            {
                virtualEGM.SendEnhancedValidationInformation(v.validationType, // validation type
                                                             v.indexNumber, // indexnumber
                                                             v.date, // date
                                                             v.time, // time
                                                             v.validationNumber, // validation number
                                                             uint.Parse(BitConverter.ToString(v.amount).Replace("-", "")), // amount
                                                             v.ticketNumber, // ticket number
                                                             v.validationSystemId, // validation system id
                                                             v.expiration, // expiration
                                                             v.poolId); // pool id                  
            }
            else
            {
                virtualEGM.SendEnhancedValidationInformation(0x00, // validation type
                                                             0x00, // indexnumber
                                                             new byte[] { 0x00, 0x00, 0x00, 0x00 }, // date
                                                             new byte[] { 0x00, 0x00, 0x00 }, // time
                                                             new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, // validation number
                                                             0, // amount
                                                             new byte[] { 0x00, 0x00 }, // ticket number
                                                             0x00, // validation system id
                                                             new byte[] { 0x00, 0x00, 0x00, 0x00 }, // expiration
                                                             new byte[] { 0x00, 0x00 }); // pool id
            }
        }

        #endregion

        /**************************************************************************************/
        /**************************************************************************************/
        /**********************************    QUERIES    *************************************/
        /**************************************************************************************/
        /**************************************************************************************/

        #region QUERIES

        /// <summary>
        /// Routine or event that notifies the VirtualEGM that the aft transfer was completed.
        /// Sending ticket transfer complete (69)
        /// It is used by MainController, when a long poll 72 interrogation response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        public void AFTTransferCompleted(byte transferStatus)
        {
            virtualEGM.AFTTransferCompleted(transferStatus);
        }

        public void UpdateRegInfo(byte regStatus, byte[] registrationKey, byte[] regPosId)
        {
            virtualEGM.UpdateRegInfo(regStatus, registrationKey, regPosId);
        }

        public byte[] GetRegistrationKey()
        {
            return virtualEGM.GetRegistrationKey();
        }

        /// <summary>
        /// Enable Transfers to VirtualEGM
        /// It is used by MainController, mainly  when AFTCurrentTransaction finished
        public void EnableTransfers()
        {
            virtualEGM.TransfersDisabled = false;
        }

        /// <summary>
        /// Disable Transfers to VirtualEGM
        /// It is used by MainController, mainly  when AFTCurrentTransaction started
        public void DisableTransfers()
        {
            virtualEGM.TransfersDisabled = true;
        }

        /// <summary>
        /// Make vegm block transfers while there is a SMIB Transfer in progress
        public void TransferInProgress(bool truth)
        {
            virtualEGM.SetTransferInProgress(truth);
        }

        /// <summary>
        /// Reset the AFT Current transfer to the last success transfer
        public void ResetCurrentTransfer()
        {
            virtualEGM.ResetCurrentTransfer();
        }

        /// <summary>
        /// AddAFTTransaction
        public void AddFinishedAFTTransaction(byte status, byte? receiptstatus, byte[] transactionId, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte? transferFlags, byte? transferType, byte[] expiration, byte[] poolId, byte? position, DateTime? transactionDate, bool IsCurrentTransferInterrogationResponse)
        {
            virtualEGM.AddTransaction(status, receiptstatus, transactionId, cashableAmount, restrictedAmount, nonRestrictedAmount, transferFlags, transferType, expiration, poolId, position, transactionDate, IsCurrentTransferInterrogationResponse);
        }

        #region VirtualEGMControl

        /// <summary>
        /// Devuelve un booleano que determina que hay un handpay en proceso
        /// Returns a boolean that determines that there is a handpay in process.
        /// It is used by MainController checking this for stop Client and stop Host
        /// </summary>
        /// <returns>true si hay un handay en proceso. false caso contrario. // true si hay un handay en proceso. false caso contrario. </returns>
        public bool AHandpayInProcess()
        {
            return virtualEGM.AHandpayInProcess();
        }


        /// <summary>
        /// Arranca todo el controlador de la VirtualEGM
        /// Starts the entire VirtualEGM driver
        /// It is used by MainController when initializes its functionality and the Restart method
        /// </summary>
        public void StartVirtualEGM()
        {
            // With s_port property, initializes the VirtualEGM model with the client
            virtualEGM.StartVirtualEGM(s_port);
        }

        /// <summary>
        /// Stop VirtualEGM
        /// It is used by MainController, mainly in the Restart method
        /// </summary>
        public void StopVirtualEGM()
        {
            virtualEGM.StopVirtualEGM();
        }

        /// <summary>
        /// Enable Client
        /// It is used by MainController, mainly in the Restart method, and when the physical egm link is restored
        /// </summary>
        public void EnableVirtualEGM()
        {
            virtualEGM.EnableVirtualEGM();
        }

        /// <summary>
        /// Stop VirtualEGM
        /// It is used by MainController, when the conditions checked in TryStopClient meets for disable client
        /// </summary>
        public void DisableVirtualEGM()
        {
            virtualEGM.DisableVirtualEGM();
        }


        /// <summary>
        /// Reset Current Ticket Redemption
        /// It is used by MainController when the Redemption State Machine is stucked in a non-terminal state
        /// </summary>
        public void ResetCurrentTicketRedemption()
        {
            virtualEGM.ResetCurrentTicketRedemption();
        }



        /// <summary>
        /// Reset Current Ticket Validation
        /// It is used by MainController when the Validation State Machine is stucked in a non-terminal state
        /// </summary>
        public void ResetCurrentTicketValidation()
        {
            virtualEGM.ResetCurrentTicketValidation();
        }

        #endregion

        #region ExceptionsRaising

        /// <summary>
        /// Rutina que toma una exception y los datos correspondientes a esa exception y la encola en la VirtualEGM para enviarla a través del client
        /// Routine that takes an exception and the data corresponding to that exception and enqueues it in the VirtualEGM to send it through the client.
        /// It is used by MainController, at PhysicalEGM handler of generic exception event arriving to Host
        /// </summary>
        /// <param name="exception">The exception code</param>
        /// <param name="exceptionData">The exception data parameter. Generally, it is included in realtime mode</param>
        public void EnqueueException(byte exception, byte[] exceptionData)
        {
            virtualEGM.EnqueueException(exception, exceptionData);
        }

        /// <summary>
        /// Rutina o evento que le avisa a la VirtualEGM que se está imprimiendo. Además, se le pasa los datos para general el long poll 4D
        /// Routine or event that notifies the VirtualEGM that it is printing. In addition, data is passed to it to generate the 4D long poll.
        /// It throws the exception 3D or 3E, depending of the parameter (3D) (3E)
        /// </summary>
        /// <param name="validationType">4D parameter. Please refer to protocol documentation</param>
        /// <param name="indexNumber">4D parameter. Please refer to protocol documentation</param>
        /// <param name="date">4D parameter. Please refer to protocol documentation</param>
        /// <param name="time">4D parameter. Please refer to protocol documentation</param>
        /// <param name="validationNumber">4D parameter. Please refer to protocol documentation</param>
        /// <param name="amount">4D parameter. Please refer to protocol documentation</param>
        /// <param name="ticketNumber">4D parameter. Please refer to protocol documentation</param>
        /// <param name="validationSystemId">4D parameter. Please refer to protocol documentation</param>
        /// <param name="expiration">exception code, it can be 3D or 3E</param>
        /// <param name="poolId">4D parameter. Please refer to protocol documentation</param>
        public void PrintingAndSave4DData(byte validationType,byte indexNumber, byte[] date, byte[] time, byte[] validationNumber, byte[] amount, byte[] ticketNumber, byte validationSystemId, byte[] expiration, byte[] poolId, byte exception)
        {
            virtualEGM.PrintingAndSave4DData(validationType,
                                             indexNumber,
                                             date,
                                             time,
                                             validationNumber,
                                             amount,
                                             ticketNumber,
                                             validationSystemId,
                                             expiration,
                                             poolId,
                                             exception);
        }

        ///// <summary>
        ///// Envío de una exception handpay (51)
        ///// Sending a handpay exception (51)
        ///// </summary>
        //public void HandPay()
        //{
        //    virtualEGM.HandpayPending();
        //}

        /// <summary>
        ///  Envío de una exception handpay (51), con data adicional del handpay
        ///  Sending of a handpay exception (51), with additional handpay data
        ///  It is used by MainController, when a long poll 1B response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// </summary>
        /// <param name="progressiveGroup">1B response info. Please refer to protocol documentation</param>
        /// <param name="level">1B response info. Please refer to protocol documentation</param>
        /// <param name="amount">1B response info. Please refer to protocol documentation</param>
        /// <param name="partialPay">1B response info. Please refer to protocol documentation</param>
        /// <param name="resetID">1B response info. Please refer to protocol documentation</param>
        public void HandPay(byte progressiveGroup,byte level, byte[] amount, byte[] partialPay, byte resetID)
        {
            virtualEGM.HandpayPending(progressiveGroup,
                                       level,
                                       amount,
                                       partialPay,
                                       resetID);
        }


        /// <summary>
        /// Envío de la exception de reset de un handpay (52)
        /// Sending a handpay reset exception (52)
        /// It is used by MainController relaying the physical exception 52 to Smib
        /// </summary>
        public void HandPayReset()
        {
            virtualEGM.HandpayReset();
        }

        /// <summary>
        ///  Rutina o evento que le avisa a la VirtualEGM que se hizo un cashout
        ///  Routine or event that notifies the VirtualEGM that a cashout has been made
        ///  It throws the exception 57 if validation type of VirtualEGM is on System. (57)
        ///  It is used by MainController, when a long poll 57 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// </summary>
        /// <param name="amount">57 response info. Please refer to protocol documentation</param>
        /// <param name="cashoutType">57 response info. Please refer to protocol documentation</param>
        public void Cashout(byte[] amount, byte cashoutType)
        {
            virtualEGM.Cashout(amount, cashoutType);
        }


        /// <summary>
        /// Rutina o evento que le avisa a la VirtualEGM que un ticket se insertó
        /// Routine or event that notifies VirtualEGM that a ticket was inserted.
        /// Sending a ticket inserted exception (67)
        /// It is used by MainController, when a long poll 70 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// </summary>
        /// <param name="amount">70 response info. Please refer to protocol documentation</param>
        /// <param name="parsingCode">70 response info. Please refer to protocol documentation</param>
        /// <param name="validationData">70 response info. Please refer to protocol documentation</param>
        public void TicketHasBeenInserted(int amount, byte parsingCode, byte[] validationData)
        {
            virtualEGM.TicketHasBeenInserted(amount, 0x00, parsingCode, validationData);
        }
        
        /// <summary>
        /// Rutina o evento que le avisa a la VirtualEGM que se completó la redemption
        /// Routine or event that notifies the VirtualEGM that the redemption was completed.
        /// Sending ticket transfer complete (68)
        /// It is used by MainController, when a long poll 71 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// </summary>
        /// <param name="status"></param>
        public void RedemptionCompleted(byte status)
        {
            virtualEGM.RedemptionCompleted(status);
        }


        #endregion

        #region PersistenceUpdate

        /// <summary>
        /// Se actualiza el ValidationType
        /// ValidationType is updated
        /// It is used by MainController, when a long poll A0 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} file 
        /// </summary>
        /// <param name="type">A0 response info. Please refer to protocol documentation</param>
        public void SetValidationType(int type)
        {
            virtualEGM.SetValidationType(type);
        }

        ///// <summary>
        /////  Se actualiza la validation extension
        /////  Validation extension updated
        ///// It is used by MainController, when a long poll A0 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        ///// Updates the {{ EGMSettings }} file 
        ///// </summary>
        ///// <param name="b"></param>
        //public void SetValidationExtensions(bool b)
        //{
        //    virtualEGM.SetValidationExtensions(b);
        //}


        /// <summary>
        /// Se actualiza o setea las features de la VirtualEGM
        /// VirtualEGM features are updated or updated
        /// It is used by MainController, when a long poll A0 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} file 
        /// </summary>
        /// <param name="feat1">A0 response info. Please refer to protocol documentation</param>
        /// <param name="feat2">A0 response info. Please refer to protocol documentation</param>
        /// <param name="feat3">A0 response info. Please refer to protocol documentation</param>
        public void SetFeatures(byte feat1, byte feat2, byte feat3)
        {
            virtualEGM.SetFeatures(feat1, feat2, feat3);
        }



        /// <summary>
        /// Rutina que actualiza el límite cashout
        /// Actualización rutinaria del límite de cobro
        ///
        /// Routine updating the cashout limit
        /// Routine update of the collection limit
        /// It is used by MainController, when a long poll A4 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} file 
        /// </summary>
        /// <param name="gameNumber">A4 response info. Please refer to protocol documentation</param>
        /// <param name="cashoutLimit">A4 response info. Please refer to protocol documentation</param>
        public void UpdateCashoutLimit(byte[] gameNumber, byte[] cashoutLimit)
        {
            virtualEGM.UpdateCashoutLimit(gameNumber, cashoutLimit);
        }

        /// <summary>
        /// Actualización de la info del game, por Id
        /// Game info update, by Id
        /// It is used by MainController, when a long poll 1F response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} and {{ EGMInfo }} files
        /// </summary>
        /// <param name="gameId">1F response info. Please refert to protocol documenation</param>
        /// <param name="additionalId">1F response info. Please refert to protocol documenation</param>
        /// <param name="denomination">1F response info. Please refert to protocol documenation</param>
        /// <param name="maxBet">1F response info. Please refert to protocol documenation</param>
        /// <param name="progressiveGroup">1F response info. Please refert to protocol documenation</param>
        /// <param name="gameOptions">1F response info. Please refert to protocol documenation</param>
        /// <param name="paytableId">1F response info. Please refert to protocol documenation</param>
        /// <param name="basePercentage">1F response info. Please refert to protocol documenation</param>
        public void UpdateGamingInfo_GameID(string gameId, string additionalId, byte denomination, byte maxBet, byte progressiveGroup, byte[] gameOptions, string paytableId, string basePercentage)
        {
            virtualEGM.UpdateGamingInfo_GameID(gameId,
                                               additionalId,
                                               denomination,
                                               maxBet,
                                               progressiveGroup,
                                               gameOptions,
                                               paytableId,
                                               basePercentage);
        }



        /// <summary>
        /// Setear los game numbers habilitados
        /// Set the enabled game numbers
        /// It is used by MainController, when a long poll 56 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} file
        /// </summary>
        /// <param name="EnabledGames">56 response info. Please refer to protocol documentation</param>
        public void SetEnabledGameNumbers(List<byte[]> EnabledGames)
        {
            virtualEGM.SetEnabledGameNumbers(EnabledGames);
        }


        /// <summary>
        ///  Enable/Disable RealTime (01 enable, 00 disable)
        ///  It is used by MainController, when a real time event change occurs in Host, raised by SASResponseHandler and processed by PhysicalEGMController
        ///  Updates the {{ EGMSettings }} file
        /// </summary>
        /// <param name="enable_disable">The boolean of real time enabled</param>
        public void EnableDisableRealTimeEvent(byte enable_disable)
        {
            virtualEGM.SetEnabledRealTime(enable_disable);
        }

        /// <summary>
        /// Update N Gaming Info
        /// It is used by MainController, when a long poll B5 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} and {{ EGMInfo }} files
        /// </summary>
        /// <param name="gameNumber">B5 response info. Please refer to protocol documentation</param>
        /// <param name="maxBet">B5 response info. Please refer to protocol documentation</param>
        /// <param name="progressiveGroup">B5 response info. Please refer to protocol documentation</param>
        /// <param name="progressiveLevels">B5 response info. Please refer to protocol documentation</param>
        /// <param name="gameNameLength">B5 response info. Please refer to protocol documentation</param>
        /// <param name="gameName">B5 response info. Please refer to protocol documentation</param>
        /// <param name="paytableLength">B5 response info. Please refer to protocol documentation</param>
        /// <param name="paytableName">B5 response info. Please refer to protocol documentation</param>
        /// <param name="wagerCategories">B5 response info. Please refer to protocol documentation</param>
        public void UpdateGameNInfo(byte[] gameNumber, byte[] maxBet, byte progressiveGroup, byte[] progressiveLevels, byte gameNameLength, byte[] gameName, byte paytableLength, byte[] paytableName, byte[] wagerCategories)
        {
            // Guardamos los datos en la physicalEGM
            // Save the data in the physicalEGM
            virtualEGM.UpdateGamingNInfo(gameNumber, maxBet, progressiveGroup, progressiveLevels, gameNameLength, gameName, paytableLength, paytableName, wagerCategories);
        }

        /// <summary>
        /// Seteo de la extension validation status
        /// Setting the extension validation status
        /// It is used by MainController, when a long poll 7B response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMSettings }} file
        /// </summary>
        /// <param name="assetNumber">7B response info. Please refer to protocol documentation</param>
        /// <param name="statusBits">7B response info. Please refer to protocol documentation</param>
        /// <param name="cashableTicketAndReceiptExpiration">7B response info. Please refer to protocol documentation</param>
        /// <param name="restrictedTicketDefaultExpiration">7B response info. Please refer to protocol documentation</param>
        public void UpdateExtendedValidationStatus(byte[] assetNumber, byte[] statusBits, byte[] cashableTicketAndReceiptExpiration, byte[] restrictedTicketDefaultExpiration)
        {
            virtualEGM.SetExtendedValidationStatus(assetNumber, statusBits, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration);
        }


        /// <summary>
        /// Update Meters for VirtualEGM
        /// It is used by MainController, when a meter value request response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMAccounting }} file
        /// </summary>
        /// <param name="code">Meter code</param>
        /// <param name="gameNumber">Game number which meter belongs to</param>
        /// <param name="value">Meter value</param>
        public void UpdateMeters(byte code, byte[] gameNumber, int value)
        {
            virtualEGM.UpdateMeter(code, gameNumber, value);
        }

        /// <summary>
        /// Update Meters for VirtualEGM for a code string
        /// It is used by MainController, when a meter value request response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMAccounting }} file
        /// </summary>
        /// <param name="meter_string">Meter string code</param>
        /// <param name="game_number">Game number which meter belongs to</param>
        /// <param name="value">Meter value</param>
        public void UpdateMeters(string meter_string, byte[] game_number, int value)
        {
            byte[] gameNumber = new byte[] { 0x00, 0x00 };
            if (game_number != null)
            {
                gameNumber = game_number;
            }
            virtualEGM.UpdateMeter(meter_string, gameNumber, value);
        }

        //// All meters updated
        ///// <summary>
        ///// 
        ///// </summary>
        //public void AllMetersUpdated()
        //{
        //    virtualEGM.AllMetersUpdated();
        //}


        //// RAM Clear
        //public void RAMClear()
        //{
        //    virtualEGM.RAMClear();
        //}


        /// <summary>
        /// Set Version ID
        /// It is used by MainController, when a long poll 54 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMInfo }} file
        /// </summary>
        /// <param name="versionID">54 response info. Please refer to protocol documentation</param>
        public void SetVersionID(byte[] versionID)
        {
            virtualEGM.SetVersionID(versionID);
        }


        /// <summary>
        /// Set GameMachine Serial Number
        /// It is used by MainController, when a long poll 54 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMInfo }} file
        /// </summary>
        /// <param name="gmSerialNumber">54 response info. Please refer to protocol documentation</param>
        public void SetGameMachineSerialNumber(byte[] gmSerialNumber)
        {
            virtualEGM.SetGameMachineSerialNumber(gmSerialNumber);
        }




        /// <summary>
        ///  Rutina o evento que le avisa a la VirtualEGM que se actualizó el número de juegos implementados
        ///  Routine or event that notifies the VirtualEGM that the number of implemented games has been updated.
        /// It is used by MainController, when a long poll 51 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ Info }} file
        /// </summary>
        /// <param name="numberOfGames">51 response info. Please refer to protocol documentation</param>
        public void SetNumberOfGamesImplemented(byte[] numberOfGames)
        {
            virtualEGM.SetNumberOfGames(numberOfGames);
        }


        /// <summary>
        /// Seteo de la información del último billete aceptado
        /// Setting of the information of the last accepted ticket
        /// It is used by MainController, when a long poll 48 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMStatus }} file
        /// </summary>
        /// <param name="countryCode">48 response info. Please refer to protocol documentation</param>
        /// <param name="denominationCode">48 response info. Please refer to protocol documentation</param>
        /// <param name="billMeter">48 response info. Please refer to protocol documentation</param>
        public void UpdateLastAcceptedBillInformation(byte countryCode, byte denominationCode, byte[] billMeter)
        {
            virtualEGM.SetLastAcceptedBillInformation(countryCode,
                                                       denominationCode,
                                                       billMeter);
        }

        /// <summary>
        /// Process LP21 response event from MainController
        /// </summary>
        /// <param name="romSignature">21 response info. Please refer to protocol documentation</param>
        public void ROMSignatureVerificationResponse(byte[] romSignature)
        {
            virtualEGM.ROMSignatureVerificationResponse(romSignature);
        }

        /// <summary>
        /// Setear fecha y hora en la Virtual EGM
        /// It is used by MainController, when a long poll 7E response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMStatus }} file
        /// </summary>
        /// <param name="month">7E response info</param>
        /// <param name="day">7E response info</param>
        /// <param name="year">7E response info</param>
        /// <param name="hour">7E response info</param>
        /// <param name="minute">7E response info</param>
        /// <param name="second">7E response info</param>
        public void SetDateAndTime(int month, int day, int year, int hour, int minute, int second)
        {
            virtualEGM.SetDateAndTime(day, month, year, hour, minute, second);
        }

        /// <summary>
        /// Setear la current game number en la Virtual EGM
        /// Setting the current game number in the Virtual EGM
        /// It is used by MainController, when a long poll 55 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMStatus }} file
        /// </summary>
        /// <param name="gameNumber">55 response info</param>
        public void SetCurrentGameNumber(byte[] gameNumber)
        {
            virtualEGM.SetCurrentGameNumber(gameNumber);
        }



        /// <summary>
        /// Setear la current denominación de jugador
        /// Set the current player name
        /// It is used by MainController, when a long poll B1 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMStatus }} file
        /// </summary>
        /// <param name="currentDenomination">B1 response info</param>
        public void SetCurrentPlayerDenomination(byte currentDenomination)
        {
            virtualEGM.SetCurrentPlayerDenomination(currentDenomination);
        }

        /// <summary>
        /// Se actualiza o setea las denominations en la PhysicalEGM
        /// The denominations in the PhysicalEGM are updated or set.
        /// It is used by MainController, when a long poll B2 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMStatus }} file
        /// </summary>
        /// <param name="NumberOfDenominations">B2 response info</param>
        /// <param name="PlayerDenominations">B2 response info</param>
        public void SetDenominations(byte NumberOfDenominations, byte[] PlayerDenominations)
        {
            virtualEGM.SetDenominations(NumberOfDenominations, PlayerDenominations);
        }

        public void Update74ResponseInfo(byte[] assetNumber, byte availableTransfers, byte gameLockStatus, byte hostCashoutStatus, byte aftStatus, byte[] restrictedExpiration, byte[] gmTransferLimit, byte gmMaxBufferIndex, byte[] currentCashableAmount, byte[] currentRestrictedAmount, byte[] currentNonRestrictedAmount, byte[] restrictedPoolID)
        {
            virtualEGM.Update74ResponseInfo(assetNumber, availableTransfers, gameLockStatus, hostCashoutStatus, aftStatus, restrictedExpiration, gmTransferLimit, gmMaxBufferIndex, currentCashableAmount, currentRestrictedAmount, currentNonRestrictedAmount, restrictedPoolID);
        }




        /// <summary>
        ///  Se actualiza o setea el token denomination en la PhysicalEGM
        ///  The denomination token is updated or set in the PhysicalEGM.
        /// It is used by MainController, when a long poll B3 response arrives to Host, processed by SASResponseHandler and PhysicalEGMController
        /// Updates the {{ EGMStatus }} file
        /// </summary>
        /// <param name="TokenDenomination"></param>
        public void SetTokenDenomination(byte TokenDenomination)
        {
            virtualEGM.SetTokenDenomination(TokenDenomination);
        }


        #endregion

        #endregion




    }
}
