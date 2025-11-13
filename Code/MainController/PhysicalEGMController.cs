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
using Recipes;


namespace MainController
{
    /// <summary>
    /// Interactúa con la PhysicalEGM, perteneciente al módulo BitbossInterface. El propósito de esta clase es ser mediador entre la VirtualEGM y el MainController, como por ejemeplo
    /// capturar eventos de la PhysicalEGM y avisarle al MainController, escuchar al MainController y redirigir pedidos a la PhysicalEGM. Vale aclarar que a diferencia de la VirtualEGM,
    /// esta clase contiene al host, por lo que tiene más interacción con este último enviando pedidos que tengan que redirigirse a la EGM, mientras que con la PhysicalEGM, se persiste la data real de la EGM
    /// 
    /// It interacts with the PhysicalEGM, belonging to the BitbossInterface module. The purpose of this class is to mediate between the VirtualEGM and the MainController, such as capture
    /// events from the PhysicalEGM and notify the MainController, listen to the MainController and redirect requests to the PhysicalEGM. It is worth mentioning that unlike VirtualEGM,
    /// this class contains the host, so it has more interaction with the latter by sending orders that have to be redirected to the EGM, while with the PhysicalEGM, the actual EGM data is persisted.
    /// </summary>
    public class PhysicalEGMBehaviourController
    {
        #region ELEMENTOS DEL CONTROLADOR O HANDLER DE LA PHYSICALEGM
        public List<byte> InitialRoutineLPsWithNoResponse;
        /// <summary>
        /// Bandera que permite forzar una registración desde la PhysicalEGM
        /// Flag that allow force a registration from PhysicalEGM
        /// </summary>
        public bool ForceDummyRegistrationOnStartup;
        /// <summary>
        /// Una recipe de rutina inicial
        /// An initial routine recipe
        /// </summary>
        public Recipe initialroutine;
        /// <summary>
        /// Una recipe de registration
        /// A registration recipe
        /// </summary>
        public Recipe registrationRecipe;
        /// <summary>
        ///  Una PhysicalEGM que persiste todos los datos 
        ///  A PhysicalEGM persisting all data 
        /// </summary>
        public PhysicalEGM physicalEGM;
        /// <summary>
        /// Un host
        /// A host
        /// </summary>
        public SASHost host;
        /// <summary>
        /// max buffer index to interrogate
        /// </summary>
        public byte maxBufferIndexForInterrogate; 
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool FlagTransferToGamingMachine = false;
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool FlagTransferFromGamingMachine = false;
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool FlagTransferToPrinter = false;
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool FlagWinAmountPendingCashoutToHost = false;
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool FlagBonusAwardToGamingMachine = false;
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool FlagLockAfterTransferRequestSupported = false;
        /// <summary>
        /// <summary>
        /// AFT Flag
        /// </summary>
        public bool AFTCurrentTransferInterrogated = false;
        /// <summary>
        /// Un contador de pedidos 6F
        /// A 6F order counter
        /// </summary>
        public int count6f = 0;
        /// <summary>
        ///  Bandera de que el timer de pedido de meters está corriendo
        ///  Flag that the meter order timer is running
        /// </summary>
        public bool TimerMetersInitiated = false;
        /// <summary>
        /// Bandera de que el timer que corre cada 3 segundos está corriendo
        /// Flag that the timer running every 3 seconds is running.
        /// </summary>
        private static bool Timer3SecondsInitiated = false;
        /// <summary>
        /// Bandera de que el timer que corre cada 30 segundos está corriendo
        /// Flag that the timer running every 30 seconds is running.
        /// </summary>
        public bool Timer30SecondsInitiated = false;
        /// <summary>
        ///  Bandera de que el timer que corre cada 300 segundos está corriendo
        ///  Flag that the timer running every 300 seconds is running.
        /// </summary>
        public bool Timer300SecondsInitiated = false;
        /// <summary>
        /// Cantidad de reintentos para largar el timer de meters
        /// Number of retries to start the meters timer
        /// </summary>
        public int TimerMetersRetries = 0;
        /// <summary>
        /// Cantidad de reintentos para largar el timer de 3 segundos
        /// Number of retries to start 3-second timer
        /// </summary>
        private static int Timer3SecondsRetries = 0;
        /// <summary>
        /// The current GMSerialNumber
        /// </summary>
        private byte[] GMCurrentSerialNumber = new byte[] {};
        /// <summary>
        /// Cantidad de reintentos para largar el timer de 30 segundos
        /// Number of retries to start 30-second timer
        /// </summary>
        public int Timer30SecondsRetries = 0;
        /// <summary>
        ///  Cantidad de reintentos para largar el timer de 300 segundos
        ///  Number of retries to start 300-second timer
        /// </summary>
        public int Timer300SecondsRetries = 0;
        /// <summary>
        /// Un módulo que gestiona la persistencia de transacciones
        /// A module that manages transaction persistence
        /// </summary>
        public TransactionsController transactionsController;
        /// <summary>
        /// Un módulo que gestiona la persistencia de trazabilidad de envíos y recepciones
        /// A module that manages the traceability persistence of shipments and receipts.
        /// </summary>
        public LogTracer logTracer;
        /// <summary>
        // The timestamp of last EGM response
        /// </summary>
        public DateTime? LastEGMResponseReceivedAt = null;
        /// <summary>
        /// Function
        /// Returns the LastEGMResponseReceivedAt attribute if it is not null. Otherwise, returns the last egm response timestamp persisted at PhysicalEGM Status file (EGMStatus)
        /// Reference: MainController.LinksHealth()
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastEGMResponseReceivedAt()
        {
            if (LastEGMResponseReceivedAt != null)
                return LastEGMResponseReceivedAt.Value;
            else 
                return physicalEGM.GetLastEGMResponseTS();
        }
        #endregion


        /*****************************************************************************/
        /*****************************************************************************/
        /*****************************************************************************/
        /**************************PHYSICALEGM EVENTS (ORDER BY CODE)*****************/
        /*****************************************************************************/
        /*****************************************************************************/
        /*****************************************************************************/

        #region PHYSICAL EGM EVENTS

        #region "Meter Updating"

        public event PhysicalMeterUpdatedHandler MeterUpdated;
        public delegate void PhysicalMeterUpdatedHandler(byte code, 
                                                         byte[] gameNumber, 
                                                         int value,
                                                         EventArgs e);

        public event PhysicalStrMeterUpdatedHandler StrMeterUpdated;
        public delegate void PhysicalStrMeterUpdatedHandler(string code,
                                                            byte[] gameNumber,
                                                            int value,
                                                            EventArgs e);
        #endregion

        #region "Exceptions"
        public event PhysicalExp3DHandler PhysicalExp3D;
        public delegate void PhysicalExp3DHandler(EventArgs e);

        public event PhysicalExp3EHandler PhysicalExp3E;
        public delegate void PhysicalExp3EHandler(EventArgs e);

        public event PhysicalExp51Handler PhysicalExp51;
        public delegate void PhysicalExp51Handler(EventArgs e);

        public event PhysicalExp52Handler PhysicalExp52;
        public delegate void PhysicalExp52Handler(EventArgs e);

        public event PhysicalExp57Handler PhysicalExp57;
        public delegate void PhysicalExp57Handler(EventArgs e);

        public event PhysicalExp67Handler PhysicalExp67;
        public delegate void PhysicalExp67Handler(EventArgs e);

        public event PhysicalExp68Handler PhysicalExp68;
        public delegate void PhysicalExp68Handler(EventArgs e);

        public event PhysicalExp69Handler PhysicalExp69;
        public delegate void PhysicalExp69Handler(EventArgs e);

        public event PhysicalExp3FHandler PhysicalExp3F;
        public delegate void PhysicalExp3FHandler(EventArgs e);

        public event PhysicalExp8CHandler PhysicalExp8C;
        public delegate void PhysicalExp8CHandler(EventArgs e);

        public event PhysicalExpDefaultHandler PhysicalExpDefault;
        public delegate void PhysicalExpDefaultHandler(byte exception, byte[] exceptionData, EventArgs e);

        #endregion

        #region "Long polls responses"

        public event PhysicalLP1BResponseHandler PhysicalLP1BResponse;
        public delegate void PhysicalLP1BResponseHandler(byte progressiveGroup,
                                                        byte level,
                                                        byte[] amount,
                                                        byte[] partialPay,
                                                        byte resetID,
                                                        EventArgs e);

        public event PhysicalLP1FResponseHandler PhysicalLP1FResponse;
        public delegate void PhysicalLP1FResponseHandler(SASResponseHandler.GamingInfo gi,
                                                        EventArgs e);
        public event PhysicalLP21ResponseHandler PhysicalLP21Response;
        public delegate void PhysicalLP21ResponseHandler(byte[] romSignature,
                                                         EventArgs e);

        public event PhysicalLP48ResponseHandler PhysicalLP48Response;
        public delegate void PhysicalLP48ResponseHandler(byte countryCode,
                                                         byte denominationCode,
                                                         byte[] billMeter,
                                                         EventArgs e);

        public event PhysicalLP4DResponseHandler PhysicalLP4DResponse;
        public delegate void PhysicalLP4DResponseHandler(byte validationType,
                                                        byte indexNumber,
                                                        byte[] date,
                                                        byte[] time,
                                                        byte[] validationNumber,
                                                        byte[] amount,
                                                        byte[] ticketNumber,
                                                        byte validationSystemId,
                                                        byte[] expiration,
                                                        byte[] poolId,
                                                        EventArgs e);


        public event PhysicalLP51ResponseHandler PhysicalLP51Response;
        public delegate void PhysicalLP51ResponseHandler(byte[] numberOfGames, EventArgs e);

        public event PhysicalLP54ResponseHandler PhysicalLP54Response;
        public delegate void PhysicalLP54ResponseHandler(byte[] VersionID, byte[] GMSerialNumber, EventArgs e);

        public event PhysicalLP55ResponseHandler PhysicalLP55Response;
        public delegate void PhysicalLP55ResponseHandler(byte[] gameNumber,
                                                        EventArgs e);

        public event PhysicalLP56ResponseHandler PhysicalLP56Response;
        public delegate void PhysicalLP56ResponseHandler(List<byte[]> EnabledGames,
                                                        EventArgs e);

        public event PhysicalLP57ResponseHandler PhysicalLP57Response;
        public delegate void PhysicalLP57ResponseHandler(SASResponseHandler.PendingCashoutInformation info,
                                                        EventArgs e);

        public event PhysicalLP58ResponseHandler PhysicalLP58Response;
        public delegate void PhysicalLP58ResponseHandler(byte status, EventArgs e);

        public event PhysicalLP70ResponseHandler PhysicalLP70Response;
        public delegate void PhysicalLP70ResponseHandler(SASResponseHandler.Response70Parameters data,
                                                        EventArgs e);

        public event PhysicalLP71_SuccesfulResponseHandler PhysicalLP71_SuccesfulResponse;
        public delegate void PhysicalLP71_SuccesfulResponseHandler(SASResponseHandler.RedeemTicket ticket,
                                                                   EventArgs e);

        public event PhysicalLP71_40ResponseHandler PhysicalLP71_40Response;
        public delegate void PhysicalLP71_40ResponseHandler(SASResponseHandler.RedeemTicket ticket,
                                                            EventArgs e);

        public event PhysicalLP71_FailedResponseHandler PhysicalLP71_FailedResponse;
        public delegate void PhysicalLP71_FailedResponseHandler(SASResponseHandler.RedeemTicket ticket,
                                                                EventArgs e);

        public event PhysicalLP72_Handler PhysicalLP72Response;
        public delegate void PhysicalLP72_Handler( byte status,
                                                    byte? receiptstatus,
                                                    byte[] transactionId,
                                                    byte[] cashableAmount,
                                                    byte[] restrictedAmount,
                                                    byte[] nonRestrictedAmount,
                                                    byte? transferFlags,
                                                    byte? transferType,
                                                    byte[] expiration,
                                                    byte[] poolId,
                                                    byte? position,
                                                    DateTime? transactionDate,
                                                    bool IsCurrentTransferInterrogationResponse,
                                                    EventArgs e);

        public event PhysicalLP73ResponseHandler PhysicalLP73Response;
        public delegate void PhysicalLP73ResponseHandler(byte regStatus,
                                                         byte[] assetNumber,
                                                         byte[] registrationKey,
                                                         byte[] regPOSId,
                                                         EventArgs e);
        public event PhysicalLP74ResponseHandler PhysicalLP74Response;
        public delegate void PhysicalLP74ResponseHandler(byte[] assetNumber,
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

        public event PhysicalLP7EResponseHandler PhysicalLP7EResponse;
        public delegate void PhysicalLP7EResponseHandler(int month, int day, int year, int hour, int minute, int second, EventArgs e);

        public event PhysicalLP7BResponseHandler PhysicalLP7BResponse;
        public delegate void PhysicalLP7BResponseHandler(byte[] assetNumber,
                                                         byte[] statusBits,
                                                         byte[] cashableTicketAndReceiptExpiration,
                                                         byte[] restrictedTicketDefaultExpiration,
                                                         EventArgs e);

        public event PhysicalLPA0ResponseHandler PhysicalLPA0Response;
        public delegate void PhysicalLPA0ResponseHandler(byte[] gameNumber,
                                                        byte features1,
                                                        byte features2,
                                                        byte features3,
                                                        EventArgs e);


        public event PhysicalLPA4ResponseHandler PhysicalLPA4Response;
        public delegate void PhysicalLPA4ResponseHandler(byte[] gameNumber,
                                                        byte[] cashoutLimit,
                                                        EventArgs e);

        public event PhysicalLPB1ResponseHandler PhysicalLPB1Response;
        public delegate void PhysicalLPB1ResponseHandler(byte currentPlayerDenomination,
                                                        EventArgs e);

        public event PhysicalLPB2ResponseHandler PhysicalLPB2Response;
        public delegate void PhysicalLPB2ResponseHandler(byte NumberOfDenominations,
                                                         byte[] PlayerDenominations,
                                                         EventArgs e);

        public event PhysicalLPB3ResponseHandler PhysicalLPB3Response;
        public delegate void PhysicalLPB3ResponseHandler(byte TokenDenomination,
                                                         EventArgs e);


        public event PhysicalLPB5ResponseHandler PhysicalLPB5Response;
        public delegate void PhysicalLPB5ResponseHandler(byte[] gameNumber,
                                                         byte[] maxBet,
                                                         byte progressiveGroup,
                                                         byte[] progressiveLevels,
                                                         byte gameNameLength,
                                                         byte[] gameName,
                                                         byte paytableLength,
                                                         byte[] paytableName,
                                                         byte[] wagerCategories,
                                                         EventArgs e);

        public event PhysicalAFTTransactionCompletedHandler PhysicalAFTTransactionCompleted;
        public delegate void PhysicalAFTTransactionCompletedHandler(EventArgs e);


        public event PhysicalEGMRealTimeEventHandler PhysicalEGMRealTimeEvent;
        public delegate void PhysicalEGMRealTimeEventHandler(byte rt, EventArgs e);


        public event PhysicalEGMCommunicationEventHandler PhysicalEGMCommunicationEvent;
        public delegate void PhysicalEGMCommunicationEventHandler(bool communication, EventArgs e);

        public event LaunchLogHandler LaunchLog; 
        public delegate void LaunchLogHandler(string[] tags, string message, EventArgs e);

        #endregion

        // This method is called when the initial routine is launched and finished.
        public void LaunchInitialRoutineFinished()
        {
            // Invoking the event 'InitialRoutineFinished' with an empty EventArgs object.
            InitialRoutineFinished(new EventArgs());
        }

        // Event that is triggered when the initial routine is finished.
        public event InitialRoutineFinishedHandler InitialRoutineFinished;

        // Delegate for the 'InitialRoutineFinished' event that takes an EventArgs object.
        public delegate void InitialRoutineFinishedHandler(EventArgs e);

        // End of the code block.


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
        /// Registration of methods and/or events: Register the events of Host and the SASResponseHandlers events into local handlers
        /// It is called at initialize_host method
        /// </summary>
        void RegisterMethods()
        {
            // Suscribimos los distintos eventos o funciones 
            // We subscribe to the different events or functions 
            host.CommandSent += new SASHost.CommandSentHandler(cmdSent);
            host.CommandReceived += new SASHost.CommandReceivedHandler(cmdReceived);
            host.DataReceived += new SASHost.ShowMessageHandler(dataReceived);
            host.CommunicationEvent += new SASHost.CommunicationEventHandler(communicationEvent);
            SASResponseHandler.Singleton.MeterUpdated += new SASResponseHandler.MeterUpdatedHandler(meterUpdated);
            SASResponseHandler.Singleton.TransactionReceived += new SASResponseHandler.TransactionReceivedHandler(transactionReceived);
            SASResponseHandler.Singleton.TransferReady69 += new SASResponseHandler.TransferReady69Handler(transferCompleted69);
            SASResponseHandler.Singleton.HandpayPending51 += new SASResponseHandler.HandpayPending51Handler(handpayPending51);
            SASResponseHandler.Singleton.AnyBillInserted += new SASResponseHandler.AnyBillInsertedHandler(anyBillInserted);
            SASResponseHandler.Singleton.HandpayReset52 += new SASResponseHandler.HandpayReset52Handler(handpayReset52);
            SASResponseHandler.Singleton.OperatorChangedOptions3C += new SASResponseHandler.OperatorChangedOptions3CHandler(operatorChangedOptions3C);
            SASResponseHandler.Singleton.Spin7F += new SASResponseHandler.Spin7FHandler(spin7F);
            SASResponseHandler.Singleton.E6FCompleted += new SASResponseHandler.E6FCompletedHandler(e6FCompleted);
            SASResponseHandler.Singleton.InfoUpdated += new SASResponseHandler.InfoUpdatedHandler(infoUpdated);
            SASResponseHandler.Singleton.ValidationNeeded57 += new SASResponseHandler.ValidationNeeded57Handler(SystemValidationNeeded57);
            SASResponseHandler.Singleton.PendingCashoutSent += new SASResponseHandler.PendingCashoutInformationSentHandler(PendingCashoutSent);
            SASResponseHandler.Singleton.CashoutTicketHasBeenPrinted3D += new SASResponseHandler.CashoutTicketHasBeenPrinted3DHandler(CashoutTicketHasBeenPrinted3D);
            SASResponseHandler.Singleton.HandpayValidated3E += new SASResponseHandler.HandpayValidated3EHandler(HandpayValidated3E);
            SASResponseHandler.Singleton.TicketInserted67 += new SASResponseHandler.TicketInserted67Handler(TicketInserted67);
            SASResponseHandler.Singleton.TicketTransferCompleted68 += new SASResponseHandler.TicketTransferCompleted68Handler(TicketTransferCompleted68);
            SASResponseHandler.Singleton.ValidationIDNotConfigured3F += new SASResponseHandler.ValidationIDNotConfigured3FHandler(ValidationIDNotConfigured3F);
            SASResponseHandler.Singleton.TicketValidationReceived += new SASResponseHandler.TicketValidationReceivedHandler(TicketValidationReceived);
            SASResponseHandler.Singleton.RedeemTicketReceived += new SASResponseHandler.RedeemTicketReceivedHandler(RedeemTicketReceived);
            SASResponseHandler.Singleton.ValidationNumberReceived += new SASResponseHandler.ValidationNumberReceivedHandler(ValidationNumberReceived);
            SASResponseHandler.Singleton.HandpayInformationReceived += new SASResponseHandler.HandpayInformationReceivedHandler(HandpayInformationReceived);
            SASResponseHandler.Singleton.SendEnhancedValidationInformationResponse += new SASResponseHandler.SendEnhancedValidationInformationResponseHandler(SendEnhancedValidationInformationResponse);
            SASResponseHandler.Singleton.SendEnabledFeaturesResponse += new SASResponseHandler.SendEnabledFeaturesResponseHandler(SendEnabledFeaturesResponse);
            SASResponseHandler.Singleton.DefaultException += new SASResponseHandler.DefaultExceptionHandler(DefaultException);
            SASResponseHandler.Singleton.SendNumberOfGamesImplemented += new SASResponseHandler.SendNumberOfGamesImplementedHandler(SendNumberOfGamesImplemented);
            SASResponseHandler.Singleton.CashoutLimitReceived += new SASResponseHandler.CashoutLimitReceivedHandler(CashoutLimitReceived);
            SASResponseHandler.Singleton.SendDateTimeGamingMachineResponse += new SASResponseHandler.SendDateTimeGamingMachineResponseHandler(SendDateTimeGamingMachineResponse);
            SASResponseHandler.Singleton.SendSelectedGameNumber += new SASResponseHandler.SendSelectedGameNumberHandler(SendSelectedGameNumber);
            SASResponseHandler.Singleton.SendEnabledGamesNumbers += new SASResponseHandler.SendEnabledGamesNumbersHandler(SendEnabledGamesNumbers);
            SASResponseHandler.Singleton.SendCurrentPlayerDenomination += new SASResponseHandler.SendCurrentPlayerDenominationHandler(SendCurrentPlayerDenomination);
            SASResponseHandler.Singleton.SendEnabledPlayerDenominations += new SASResponseHandler.SendEnabledPlayerDenominationsHandler(SendEnabledPlayerDenominations);
            SASResponseHandler.Singleton.SendTokenDenomination += new SASResponseHandler.SendTokenDenominationHandler(SendTokenDenomination);
            SASResponseHandler.Singleton.RealTimeEvent += new SASResponseHandler.RealTimeEventHandler(RealTimeEvent);
            SASResponseHandler.Singleton.SelectedGame8C += new SASResponseHandler.SelectedGame8CHandler(SelectedGame8C);
            SASResponseHandler.Singleton.SendSASVersionIDAndGamingMachineSerialNumber += new SASResponseHandler.SendSASVersionIDAndGamingMachineSerialNumberHandler(SendSASVersionIDAndGamingMachineSerialNumber);
            SASResponseHandler.Singleton.SendWagerCategoryInformationResponse += new SASResponseHandler.SendWagerCategoryInformationHandler(SendWagerCategoryInformationResponse);
            SASResponseHandler.Singleton.SendGameNExtendedInformationResponse += new SASResponseHandler.SendGameNExtendedInformationResponseHandler(SendGameNExtendedInformationResponse);
            SASResponseHandler.Singleton.SendLastBillAcceptedInformationResponse += new SASResponseHandler.SendLastBillAcceptedInformationResponseHandler(SendLastBillAcceptedInformationResponse);
            SASResponseHandler.Singleton.ExtendedValidationStatusResponse += new SASResponseHandler.ExtendedValidationStatusResponseHandler(ExtendedValidationStatusResponse);
            SASResponseHandler.Singleton.AFTLockAndStatusRequestGamingMachineResponse += new SASResponseHandler.AFTLockAndStatusRequestGamingMachineResponseHandler(AFTLockAndStatusRequestGamingMachineResponse);
            SASResponseHandler.Singleton.LPReceived += new SASResponseHandler.LPReceivedHandler(LPReceived);
            SASResponseHandler.Singleton.AFTRegisterGamingMachineResponse += new SASResponseHandler.AFTRegisterGamingMachineResponseHandler(AFTRegisterGamingMachineRespoonse);
            SASResponseHandler.Singleton.ROMSignatureVerificationResponse += new SASResponseHandler.ROMSignatureVerificationResponseHandler(ROMSignatureVerificationResponse);
        }

        /// <summary>
        /// Inicialización del host
        /// Host initialization. Host instantiation, methods registration and timers configuration
        /// It is used at constructor
        /// </summary>
        /// <param name="port">The serial port name used by SASHOst </param>
        void initialize_host(string port)
        {
            // Sea host un nuevo host() // Let host be a new host()
            host = new SASHost();
            // Seteamos el puerto // We set the port
            host.SetSerialPort(port);

            RegisterMethods();

            // TimerReading: Tiempo máximo de lectura // TimerReading: Maximum reading time 
            TimerCheckMeters = new System.Timers.Timer(1000);
            TimerCheckMeters.Elapsed += TimerStartCheckMeters;
            Timer10SecondsAfter8A = new System.Timers.Timer(10000);
            Timer10SecondsAfter8A.Elapsed += TimerStart10SecondsAfter8A;
            Timer3Seconds = new System.Timers.Timer(3000);
            Timer3Seconds.Elapsed += TimerStart3Seconds;
            Timer30Seconds = new System.Timers.Timer(30000);
            Timer30Seconds.Elapsed += TimerStart30Seconds;
            Timer300Seconds = new System.Timers.Timer(300000);
            Timer300Seconds.Elapsed += TimerStart300Seconds;

        }

        /// <summary>
        /// Función que devuelve el Trace config que es el modo en el que el controller deja logs
        /// Function that returns the Trace config that is the way in which the controller leaves logs. It can be host-api or host-elastic-search
        /// It is used in the CreateLiveTraceController method to instantate the LogTracer
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
                conf = "host-elastic-search";
            }
            else if (conf == "api")
            {
                conf = "host-api";
            }
            return conf;
        }

        /// <summary>
        ///  Inicializamos el handler Live Trace
        ///  We initialize the Live Trace handler
        ///  Instantiate the LogTracer method based on trace config
        /// </summary>
        public void CreateLiveTraceController()
        {
            logTracer = new LogTracer(getTraceConfig());
            logTracer.Logger.Init();
        }

        /// <summary>
        /// Inicializamos el host y la physicalEGM del controller
        /// Initialize the host and the controller's physicalEGM. Also, we initiate the registrationREcipe and initialRoutine.
        /// All references to this constructor is in initialize_physicalEGM, when passing the serial port by argument, and if it is empty, pass the COM1 serial port by default
        /// </summary>
        /// <param name="port">The serial port name</param>
        public PhysicalEGMBehaviourController(string port)
        {
            initialroutine = new Recipe("InitialRoutine"); // Instancio una nueva recipe // I am launching a new recipe
            initialroutine.Instance.Init(this);
            InitialRoutineLPsWithNoResponse = new List<byte>();

            registrationRecipe = new Recipe("Registration"); // Instancio una nueva recipe // I am launching a new recipe 
            registrationRecipe.Instance.Init(this);
            Console.WriteLine($"[{DateTime.Now}] Reading PhysicalEGM files");
            physicalEGM = new PhysicalEGM();
            initialize_host(port);
        }

        /// <summary>
        /// La rutina inicial del controlador de la PhysicalEGM
        /// The initial routine of the PhysicalEGM controller. 
        /// It is used recursively here if it is Failed, and it is used at StartRecipesThread method
        /// </summary>
        /// <param name="retries">The max number of retries. If it reaches zero, it continues as registration</param>
        void initial_routine(int retries)
        {
            if (retries >= 0)
            {
                // Ejecutar la recipe, esperando el status
                // Execute the recipe, waiting for the status
                ActionStatus status = initialroutine.Instance.Execute();
                // Si el status es Completed
                // If the status is Completed
                if (status == ActionStatus.Completed)
                {
                    // Lanzo la registration
                    // I launch the registration
                    registration(0);
                }
                // Si el status es Failed
                // If the status is Failed
                else if (status == ActionStatus.Failed)
                {
                    Thread.Sleep(2000);
                    initial_routine(retries - 1);
                }
                if (InitialRoutineLPsWithNoResponse.Count() > 0)
                {
                    LaunchLog(new string[] {"EGM ->"}, $"*******************************************************************************************", new EventArgs());
                    LaunchLog(new string[] {"EGM ->"}, $"****************WARNING: Long Polls with no response during initial routine:****************", new EventArgs());
                    LaunchLog(new string[] {"EGM ->"}, $"********************************************{BitConverter.ToString(InitialRoutineLPsWithNoResponse.ToArray())}*********************************************", new EventArgs());
                    LaunchLog(new string[] {"EGM ->"}, $"*******************************************************************************************", new EventArgs());
                }                                
            }

        }

    
        /// <summary>
        /// La rutina de registración
        /// The registration routine
        /// It is used at initial routine once this method is finished as Failed or Completed.
        /// </summary>
        /// <param name="retries">The max number of retries. If it reaches zero, it continues as continous polling</param>
        void registration(int retries)
        {
            if (retries >= 0 && ForceDummyRegistrationOnStartup)
            {
                // Ejecutar la recipe, esperando el status
                // Execute the recipe, waiting for the status
                ActionStatus status = registrationRecipe.Instance.Execute();
                // Si el status es Completed
                // If the status is Completed
                if (status == ActionStatus.Completed)
                {
                    StartContinousPolling();
                }
                // Si el status es Failed
                // If the status is Failed
                else if (status == ActionStatus.Failed)
                {
                    StartContinousPolling();
                }
            }
            else
            {
                StartContinousPolling();
            }
        }

        /// <summary>
        /// Comienza el polleo continuo. Arranca todos los timers del controlador
        /// Start continuous polling. Start all timers of the controller
        /// It is used at registration method, once if that method is finished as Failed or Completed
        /// </summary>
        public void StartContinousPolling()
        {
            // Start TimerCheckMeters
            TimerCheckMeters.Start();
            // Start Timer3Seconds
            Timer3Seconds.Start();
            // Start Timer30Seconds
            Timer30Seconds.Start();
            // Start Timer300Seconds          
            Timer300Seconds.Start();
        }

        /// <summary>
        /// Lanza todas las recipes, empezando por la rutina inicial.
        /// Launch all recipes, starting with the initial routine.
        /// It is used by StartPhyisicalEGM, as the first execution
        /// </summary>
        public void StartRecipesThread()
        {
            // 6 segundos para descarga
            // 6 seconds to download
            Thread.Sleep(6000);
            // Lanzo la rutina inicial
            // I launch the initial routine
            Task.Run(() => initial_routine(1));

        }


        /// <summary>
        /// Arranca todo el controlador de la PhysicalEGM, comenzando por la misma, el controlador de transacciones, el host, y las recipes
        /// Starts the entire PhysicalEGM driver, starting with the PhysicalEGM, the transaction driver, the host, and the recipes.
        /// </summary>
        public void StartPhysicalEGM()
        {
            // Arrancamos la physicalEGM
            // We start the physicalEGM
            physicalEGM.StartPhysicalEGM();
            // Instanciamos un nuevo controlador de transacciones
            // We install a new transaction controller
            transactionsController = new TransactionsController();
            // Seteamos el asset number, sacando el dato de la interfacing settings
            // We set the asset number, taking the data from the interfacing settings
            host.SetAssetNumber(InterfacingSettings.Singleton().AFT_AssetNumber);
            // Comienzo de polleo
            // Comienzo de polleo
            LaunchLog(new string[] { }, $"Starting Host", new EventArgs() );
            host.StartPolling();
            // Lanzo el thread de rutina inicial, si la rutina inicial no está en ejecución
            // I launch the initial routine thread, if the initial routine is not running
            Task.Run(() => StartRecipesThread());
        }


        /// <summary>
        /// Stop PhysicalEGM, stop all timers running. Setting to false of all booleans variables and setting to zero all retries variables 
        /// </summary>
        public void StopPhysicalEGM()
        {
            TimerCheckMeters.Stop();
            Timer3Seconds.Stop();
            Timer30Seconds.Stop();
            Timer300Seconds.Stop();
            host.StopPolling();
            count6f = 0;
            TimerMetersRetries = 0; // Cantidad de reintentos para largar el timer de meters // Number of retries to start the meters timer 
            Timer3SecondsRetries = 0; // Cantidad de reintentos para largar el timer de 3 segundos // Number of retries to start 30-second timer 
            Timer30SecondsRetries = 0; // Cantidad de reintentos para largar el timer de 30 segundos // Number of retries to start 30-second timer 
            Timer300SecondsRetries = 0; // Cantidad de reintentos para largar el timer de 300 segundos // Number of retries to start 300-second timer 

        }



        /* Transactions */
        /// <summary>
        ///  Transferencia o cashout, según el código
        ///  Transfer or cashout, depending on the code
        ///  It is used on AFTTransferFunds, request from WebAPI
        /// </summary>
        /// <param name="code">The transfer mode: It can be 00 or 80</param>
        /// <param name="cashableAmount">Cashable amount/credits</param>
        /// <param name="restrictedAmount">Restricted amount/credits</param>
        /// <param name="nonrestrictedAmount">Non Restricted amount / credits</param>
        public void transfer(byte code, long cashableAmount, long restrictedAmount, long nonrestrictedAmount, byte[] transactionID, byte[] registrationKey, byte[] expiration)
        {
            host.AFTTransferFunds(code, cashableAmount, restrictedAmount, nonrestrictedAmount, transactionID, registrationKey, expiration);

        }

        #endregion



        /***************************************************************************************/
        /***************************************************************************************/
        /***************************************************************************************/
        /******************************    HOST EVENT HANDLERS      ****************************/
        /***************************************************************************************/
        /***************************************************************************************/
        /***************************************************************************************/

        #region HOST EVENT HANDLERS
        /****************************************     HOST SASRESPONSE HANDLERS         ******************************************/

        #region Host SasResponse Handlers

        #region Meters
        /// <summary>
        ///  Se actualizó un meter
        ///  Updated a meter
        ///  EventHandler executed when a meter code arrives from a meter long poll response
        ///  It is used as handler of MeterUpdated from SASResponseHandler
        /// </summary>
        /// <param name="m">The meter structure: name, code, value, gameNumber</param>
        /// <param name="e"></param>
        void meterUpdated(SASResponseHandler.Meter m, EventArgs e)
        {
            byte[] gameNumber = new byte[] { 0x00, 0x00 };
            if (m.gameNumber != null)
            {
                gameNumber = m.gameNumber;
            }
            // Chequea cuál es el meter, por caso. Hay dos alternativas, por nombre o por código
            // Check which one to put in, if any. There are two alternatives, by name or by code
            switch (m.meterName)
            {
                /* Por nombre */
                case "Total_Coin_In":
                    // Avisa a la physicalEGM para actualizar su Accounting
                    // Notify physicalEGM to update your Accounting
                    physicalEGM.UpdateTotalCoinIn(gameNumber, m.value);
                    // Avisa al MainController para que, a través de la VirtualEGM, actualize su Accounting
                    // Notifies the MainController to update its Accounting through the VirtualEGM.
                    try { MeterUpdated(0x00, gameNumber, m.value, e); } catch { }
                    break;
                case "Total_Coin_Out":
                    physicalEGM.UpdateTotalCoinOut(gameNumber, m.value);
                    try { MeterUpdated(0x01, gameNumber, m.value, e); } catch { }
                    break;
                case "Total_Drop_Meter":
                    physicalEGM.UpdateTotalDrop(gameNumber, m.value);
                    try { MeterUpdated(0x24, gameNumber, m.value, e); } catch { }
                    break;
                case "Total_Jackpot_Meter":
                    physicalEGM.UpdateTotalJackPot(gameNumber, m.value);
                    try { MeterUpdated(0x02, gameNumber, m.value, e); } catch { }
                    break;
                case "Games_Played_Meter":
                    physicalEGM.UpdateGamesPlayed(gameNumber, m.value);
                    try { MeterUpdated(0x05, gameNumber, m.value, e); } catch { }
                    break;
                case "Games_Won_Meter":
                    physicalEGM.UpdateGamesWon(gameNumber, m.value);
                    try { MeterUpdated(0x06, gameNumber, m.value, e); } catch { }
                    break;
                case "Door_Open_Meter":
                    physicalEGM.UpdateSlootDoorOpen(gameNumber, m.value);
                    try { StrMeterUpdated("SlotDoorOpen", gameNumber, m.value, e); } catch { }
                    break;
                case "Power_Reset_Meter":
                    physicalEGM.UpdatePowerReset(gameNumber, m.value);
                    try { StrMeterUpdated("PowerReset", gameNumber, m.value, e); } catch { }
                    break;
                case "Current_Credits_Meter":
                    physicalEGM.Update000C(gameNumber, m.value);
                    try { MeterUpdated(0x0C, gameNumber, m.value, e); } catch { }
                    break;
                case "Current_Restricted_Credits_Meter":
                    physicalEGM.Update001B(gameNumber, m.value);
                    try { MeterUpdated(0x1B, gameNumber, m.value, e); } catch { }
                    break;
                case "TrueCoinIn":
                    meterUpdated("TrueCoinIn", gameNumber, m.value);
                    try { StrMeterUpdated("TrueCoinIn", gameNumber, m.value, e); } catch { }
                    break;
                case "TrueCoinOut":
                    meterUpdated("TrueCoinOut", gameNumber, m.value);
                    try { StrMeterUpdated("TrueCoinOut", gameNumber, m.value, e); } catch { }
                    break;
                case "TotalBillsInDollars":
                    meterUpdated("TotalBillsInDollars", gameNumber, m.value);
                    try { StrMeterUpdated("TotalBillsInDollars", gameNumber, m.value, e); } catch { }
                    break;
                case "BonusingDeductible":
                    meterUpdated("BonusingDeductible", gameNumber, m.value);
                    try { StrMeterUpdated("BonusingDeductible", gameNumber, m.value, e); } catch { }
                    break;
                case "BonusingNoDeductible":
                    meterUpdated("BonusingNoDeductible", gameNumber, m.value);
                    try { StrMeterUpdated("BonusingNoDeductible", gameNumber, m.value, e); } catch { }
                    break;
                case "BonusingWagerMatch":
                    meterUpdated("BonusingWagerMatch", gameNumber, m.value);
                    try { StrMeterUpdated("BonusingWagerMatch", gameNumber, m.value, e); } catch { }
                    break;
                default:
                    {
                        /* Por código */
                        /* by code */
                        switch (m.meterCode)
                        {
                            case 0x00:
                                physicalEGM.UpdateTotalCoinIn(gameNumber, m.value);
                                try { MeterUpdated(0x00, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x01:
                                physicalEGM.UpdateTotalCoinOut(gameNumber, m.value);
                                try { MeterUpdated(0x01, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x02:
                                physicalEGM.UpdateTotalJackPot(gameNumber, m.value);
                                try { MeterUpdated(0x02, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x05:
                                physicalEGM.UpdateGamesPlayed(gameNumber, m.value);
                                try { MeterUpdated(0x05, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x06:
                                physicalEGM.UpdateGamesWon(gameNumber, m.value);
                                try { MeterUpdated(0x06, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x1B:
                                physicalEGM.Update001B(gameNumber, m.value);
                                try { MeterUpdated(0x1B, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x0C:
                                physicalEGM.Update000C(gameNumber, m.value);
                                try { MeterUpdated(0x0C, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x04: // Total_Cancelled_Credits
                                physicalEGM.Update0004(gameNumber, m.value);
                                try { MeterUpdated(0x04, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x15: // Total tickets in credits
                                physicalEGM.Update0015(gameNumber, m.value);
                                try { MeterUpdated(0x15, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x16: // Total tickets out credits
                                physicalEGM.Update0016(gameNumber, m.value);
                                try { MeterUpdated(0x16, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x17: // 
                                physicalEGM.Update0017(gameNumber, m.value);
                                try { MeterUpdated(0x17, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x18: // 
                                physicalEGM.Update0018(gameNumber, m.value);
                                try { MeterUpdated(0x18, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x24: // Total drops credits
                                physicalEGM.Update0024(gameNumber, m.value);
                                physicalEGM.UpdateTotalDrop(gameNumber, m.value);
                                try { MeterUpdated(0x24, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x28:  // Total cashable in credits, including non-restricted
                                physicalEGM.Update0028(gameNumber, m.value);
                                try { MeterUpdated(0x28, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x29: // Total regular cashable ticket in credits
                                physicalEGM.Update0029(gameNumber, m.value);
                                try { MeterUpdated(0x29, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x2A: // Total regular cashable ticket in credits
                                physicalEGM.Update002A(gameNumber, m.value);
                                try { MeterUpdated(0x2A, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x2B: // Total nonrestricted promotional ticket out credits
                                physicalEGM.Update002B(gameNumber, m.value);
                                try { MeterUpdated(0x2B, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x2C: // Total cashable ticket out credits, including debits 
                                physicalEGM.Update002B(gameNumber, m.value);
                                try { MeterUpdated(0x2C, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x2D: // Total restricted promotional ticket out credits
                                physicalEGM.Update002D(gameNumber, m.value);
                                try { MeterUpdated(0x2D, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x2E: // Electronic regular cashable transfers to gaming
                                       // machine, not including external bonus awards (credits)
                                physicalEGM.Update002E(gameNumber, m.value);
                                try { MeterUpdated(0x2E, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x32: // Electronic regular cashable transfers to host (credits)
                                physicalEGM.Update0032(gameNumber, m.value);
                                try { MeterUpdated(0x32, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x35: // Total regular cashable ticket in count
                                physicalEGM.Update0035(gameNumber, m.value);
                                try { MeterUpdated(0x35, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x36: // Total restricted promotional ticket out credits
                                physicalEGM.Update0036(gameNumber, m.value);
                                try { MeterUpdated(0x36, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x37:  // Total nonrestricted ticket in count
                                physicalEGM.Update0037(gameNumber, m.value);
                                try { MeterUpdated(0x37, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x38: // Total cashable out count, including debit ticket
                                physicalEGM.Update0038(gameNumber, m.value);
                                try { MeterUpdated(0x38, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x39: // Total restricted promotional ticket out count
                                physicalEGM.Update0039(gameNumber, m.value);
                                try { MeterUpdated(0x39, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x80: // Regular cashable ticket in cents
                                physicalEGM.Update0080(gameNumber, m.value);
                                try { MeterUpdated(0x80, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x81: // Regular cashable ticket in count
                                physicalEGM.Update0081(gameNumber, m.value);
                                try { MeterUpdated(0x81, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x82: // Restricted ticket in cent
                                physicalEGM.Update0082(gameNumber, m.value);
                                try { MeterUpdated(0x82, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x83: // Restricted ticket in count
                                physicalEGM.Update0083(gameNumber, m.value);
                                try { MeterUpdated(0x83, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x84: // Nonrestricted ticket in cents
                                physicalEGM.Update0084(gameNumber, m.value);
                                try { MeterUpdated(0x84, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x85: // Nonrestricted ticket in count
                                physicalEGM.Update0085(gameNumber, m.value);
                                try { MeterUpdated(0x85, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x86: // Regular cashable ticket out cents
                                physicalEGM.Update0086(gameNumber, m.value);
                                try { MeterUpdated(0x86, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x87: // Regular cashable ticket out count
                                physicalEGM.Update0087(gameNumber, m.value);
                                try { MeterUpdated(0x87, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x88: // Restricted ticket out cents
                                physicalEGM.Update0088(gameNumber, m.value);
                                try { MeterUpdated(0x88, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x89: // Restricted ticket out counts
                                physicalEGM.Update0089(gameNumber, m.value);
                                try { MeterUpdated(0x89, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x8A: // Debit ticket out cents
                                physicalEGM.Update008A(gameNumber, m.value);
                                try { MeterUpdated(0x8A, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x8B: // Debit ticket out counts
                                physicalEGM.Update008B(gameNumber, m.value);
                                try { MeterUpdated(0x8B, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x8C: // Validated cancelled credit handpay, receipt printed cents
                                physicalEGM.Update008C(gameNumber, m.value);
                                try { MeterUpdated(0x8C, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x8D: // Validated cancelled credit handpay, receipt printed counts
                                physicalEGM.Update008D(gameNumber, m.value);
                                try { MeterUpdated(0x8D, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x8E: // Validated jackpot handpay, receipt printed cents
                                physicalEGM.Update008E(gameNumber, m.value);
                                try { MeterUpdated(0x8E, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x8F: // Validated jackpot handpay, receipt printed counts
                                physicalEGM.Update008F(gameNumber, m.value);
                                try { MeterUpdated(0x8F, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x90: // Validated cancelled credit handpay, no receipt cents
                                physicalEGM.Update0090(gameNumber, m.value);
                                try { MeterUpdated(0x90, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x91: //  Validated cancelled credit handpay, no receipt counts
                                physicalEGM.Update0091(gameNumber, m.value);
                                try { MeterUpdated(0x91, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x92: // Validated jackpot handpay, no receipt cents
                                physicalEGM.Update0092(gameNumber, m.value);
                                try { MeterUpdated(0x92, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x93: // Validated jackpot handpay, no receipt counts
                                physicalEGM.Update0093(gameNumber, m.value);
                                try { MeterUpdated(0x93, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x3E: // Number of bills currently in stacker
                                physicalEGM.Update003E(gameNumber, m.value);
                                try { MeterUpdated(0x3E, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x3F: // Total value of bills currently in stacker (Credits)
                                physicalEGM.Update003F(gameNumber, m.value);
                                try { MeterUpdated(0x3F, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x40: // 
                                physicalEGM.Update0040(gameNumber, m.value);
                                try { MeterUpdated(0x40, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x41: //
                                physicalEGM.Update0041(gameNumber, m.value);
                                try { MeterUpdated(0x41, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x42: // 
                                physicalEGM.Update0042(gameNumber, m.value);
                                try { MeterUpdated(0x42, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x43: //
                                physicalEGM.Update0043(gameNumber, m.value);
                                try { MeterUpdated(0x43, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x44: //
                                physicalEGM.Update0044(gameNumber, m.value);
                                try { MeterUpdated(0x44, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x45: //
                                physicalEGM.Update0045(gameNumber, m.value);
                                try { MeterUpdated(0x45, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x46: //
                                physicalEGM.Update0046(gameNumber, m.value);
                                try { MeterUpdated(0x46, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x47: //
                                physicalEGM.Update0047(gameNumber, m.value);
                                try { MeterUpdated(0x47, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x48: //
                                physicalEGM.Update0048(gameNumber, m.value);
                                try { MeterUpdated(0x48, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x49: //
                                physicalEGM.Update0049(gameNumber, m.value);
                                try { MeterUpdated(0x49, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x50: //
                                physicalEGM.Update0050(gameNumber, m.value);
                                try { MeterUpdated(0x50, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x51: //
                                physicalEGM.Update0051(gameNumber, m.value);
                                try { MeterUpdated(0x51, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x52: //
                                physicalEGM.Update0052(gameNumber, m.value);
                                try { MeterUpdated(0x52, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x53: //
                                physicalEGM.Update0053(gameNumber, m.value);
                                try { MeterUpdated(0x53, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x54: //
                                physicalEGM.Update0054(gameNumber, m.value);
                                try { MeterUpdated(0x54, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x55: //
                                physicalEGM.Update0055(gameNumber, m.value);
                                try { MeterUpdated(0x55, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x56: //
                                physicalEGM.Update0056(gameNumber, m.value);
                                try { MeterUpdated(0x56, gameNumber, m.value, e); } catch { }
                                break;
                            case 0x57: //
                                physicalEGM.Update0057(gameNumber, m.value);
                                try { MeterUpdated(0x57, gameNumber, m.value, e); } catch { }
                                break;
                            default:
                                if (m.meterCode != null)
                                {
                                    physicalEGM.UpdateMeter(m.meterCode.Value, gameNumber, m.value);
                                    try { MeterUpdated(m.meterCode.Value, gameNumber, m.value, e); } catch { }
                                }
                                break;
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// Se actualizó un meter con código string
        /// Updated a meter with string code
        /// EventHandler executed when a meter code arrives from a specific info long poll response with no meter code
        /// It is used as handler of MeterUpdated with meter code string from SASResponseHandler
        /// </summary>
        /// <param name="meter_string">meter string code</param>
        /// <param name="game_number">game number byte array</param>
        /// <param name="value">The value of this meter</param>
        public void meterUpdated(string meter_string, byte[] game_number, int value)
        {
            // Tomo como default el gameNumber
            // I take as default the gameNumber
            byte[] gameNumber = new byte[] { 0x00, 0x00 };
            // Si el que me viene como parámetro no es nulo
            // If the one that comes to me as parameter is not null
            if (game_number != null)
            {
                // al gameNumber le asigno el parámetro  
                // to the gameNumber I assign the parameter  
                gameNumber = game_number;
            }
            // Lanzo el evento
            // I launch the event
            physicalEGM.UpdateMeter(meter_string, gameNumber, value);
        }

        #endregion

        #region Transactions
        /// <summary>
        /// Se recevió una respuesta a un interrogate ( es decir, una transacción)
        /// A response to an interrogate (i.e. a transaction) was received.
        /// It is the event handler for TransactionReceived from SASResponseHandler
        /// </summary>
        /// <param name="interrogate_response">The long poll resposne from interrogate</param>
        /// <param name="e"></param>
        void transactionReceived(byte[] interrogate_response, EventArgs e)
        {

            // Transición de Sent -> Pending, si el quinto byte (status) es pending (0x40)
            if (GetByteFromArrayIndex(interrogate_response, 4) == 0x40)
            {
                if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Pending))
                {
                    AFTCurrentTransaction.Instance().InternalStatus = GetByteFromArrayIndex(interrogate_response, 4).Value;
                    AFTCurrentTransaction.Instance().Update();
                }
            }
            // Transición de Sent -> Rejected -> Interrogated, si el quinto byte (status) es está entre 0x80 y 0x90
            else if ((GetByteFromArrayIndex(interrogate_response, 4) >= 0x80 && GetByteFromArrayIndex(interrogate_response, 4) < 0x9F)
                   || GetByteFromArrayIndex(interrogate_response, 4) == 0xC0
                   || GetByteFromArrayIndex(interrogate_response, 4) == 0xC1)
            {
                if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Rejected))
                {
                    if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Interrogated))
                    {
                        AFTInterrogate();
                    }
                }

            }


            /*
            * Generamos una nueva línea de transacción
            * We generate a new transaction line
            * 
            */
            DateTime? TransactionDate = null;
            byte? ReceiptStatus = GetByteFromArrayIndex(interrogate_response, 5);
            byte? TransferType = GetByteFromArrayIndex(interrogate_response, 6);
            byte? TransferFlags = GetByteFromArrayIndex(interrogate_response, 22);
            byte? Position = GetByteFromArrayIndex(interrogate_response, 3);
            string TransactionID = null;
            int? RestrictedAmount = null;
            byte[] transaction_arr = new byte[] {};
            byte[] _cashableAmount = new byte[] {};
            byte[] _restrictedAmount = new byte[] {};
            byte[] _nonrestrictedAmount = new byte[] {};
            byte[] expiration = new byte[] {};
            byte[] poolID = new byte[] {};
            byte? transferStatus = GetByteFromArrayIndex(interrogate_response, 4);
            TransactionLogLine t = new TransactionLogLine();
            int? length = GetByteFromArrayIndex(interrogate_response, 2); // Obtenemos el tamaño // We obtain the size
            if (length >= 39) // Si tiene al menos 39 bytes
            {
                if (Position != null) // Chequeamos que el cuarto byte no sea nulo // We check that the fourth byte is not null
                {
                    t.Position = Position.Value; // Obtenemos la posición o índice de la transacción // We obtain the position or index of the transaction 
                    // if (transaction_init_index == null) // Si es la primera vez que consultamos con un interrogate // If this is the first time we have consulted with an interrogate 
                    // {
                    //     transaction_init_index = Position; // La posición será el primer índice como referencia para ir consultado sus anteriores transacciones // The position will be the first index as a reference to consult your previous transactions.
                    //     for (byte i = transaction_init_index.Value; i > 0; i--)
                    //     {
                    //         host.AFTInit(i); // Interrogamos las anteriores transacciones //  We question the above transactions
                    //     }
                    // }
                    // Si el quinto byte es no nulo, proseguimos con seguridad
                    // If the fifth byte is non-null, we proceed with safety
                    if (transferStatus != null)
                    {
                        // Guardamos el status de transferencia
                        // Guardamos el status de transferencia
                        t.TransferStatus = "0x" + BitConverter.ToString(new byte[] { transferStatus.Value });
                        // Si el sexto y séptimo byte son no nulos, proseguimos con seguridad
                        // If the sixth and seventh bytes are non-null, we proceed with safety

                        if (ReceiptStatus != null
                        && TransferType != null)
                        {
                            // Guardamos el status de recepción y tipo de transferencia
                            // We save the status of reception and type of transfer.
                            t.ReceiptStatus = "0x" + BitConverter.ToString(new byte[] { ReceiptStatus.Value });
                            ReceiptStatus = GetByteFromArrayIndex(interrogate_response, 5).Value;
                            t.TransferType = "0x" + BitConverter.ToString(new byte[] { TransferType.Value });
                            // Guardamos los amounts
                            // We save the amounts

                             _cashableAmount = new byte[] { interrogate_response[7], interrogate_response[8], interrogate_response[9], interrogate_response[10], interrogate_response[11] };
                            _restrictedAmount = new byte[] { interrogate_response[12], interrogate_response[13], interrogate_response[14], interrogate_response[15], interrogate_response[16] };
                            _nonrestrictedAmount =  new byte[] { interrogate_response[17], interrogate_response[18], interrogate_response[19], interrogate_response[20], interrogate_response[21] };
                            t.CashableAmount = int.Parse(BitConverter.ToString(_cashableAmount).Replace("-", ""));
                            t.RestrictedAmount = int.Parse(BitConverter.ToString(_restrictedAmount).Replace("-", ""));
                            RestrictedAmount = t.RestrictedAmount;
                            // Guardamos el ID de Transaccion
                            // Save the Transaction ID

                            int idLengthIndex = 27;
                            int transaction_length = GetByteFromArrayIndex(interrogate_response, idLengthIndex).Value;
                            List<byte> transaction_list = new List<byte>();
                            for (int k = 1; k <= transaction_length; k++)
                            {
                                transaction_list.Add(GetByteFromArrayIndex(interrogate_response, idLengthIndex + k).Value);
                            }
                            transaction_arr = transaction_list.ToArray();
                            t.TransactionID = System.Text.Encoding.ASCII.GetString(transaction_arr);
                            TransactionID = t.TransactionID;
                            // Parseamos la fecha de transacción
                            // Parse the transaction date

                            byte[] _transactionDay = { interrogate_response[idLengthIndex + transaction_length + 2] };
                            byte[] _transactionMonth = { interrogate_response[idLengthIndex + transaction_length + 1] };
                            byte[] _transactionYear = { interrogate_response[idLengthIndex + transaction_length + 3], interrogate_response[idLengthIndex + transaction_length + 4] };
                            byte[] _transactionHour = { interrogate_response[idLengthIndex + transaction_length + 5] };
                            byte[] _transactionMinute = { interrogate_response[idLengthIndex + transaction_length + 6] };
                            byte[] _transactionSecond = { interrogate_response[idLengthIndex + transaction_length + 7] };

                            try
                            {
                                t.TransactionDateTime = new DateTime(int.Parse(BitConverter.ToString(_transactionYear).Replace("-", "")),
                                                                    int.Parse(BitConverter.ToString(_transactionMonth).Replace("-", "")),
                                                                    int.Parse(BitConverter.ToString(_transactionDay).Replace("-", "")),
                                                                    int.Parse(BitConverter.ToString(_transactionHour).Replace("-", "")),
                                                                    int.Parse(BitConverter.ToString(_transactionMinute).Replace("-", "")),
                                                                    int.Parse(BitConverter.ToString(_transactionSecond).Replace("-", "")));
                                TransactionDate = t.TransactionDateTime;
                            }
                            catch
                            {
                                t.TransactionDateTime = DateTime.MinValue;
                                TransactionDate = t.TransactionDateTime;

                            }


                            try
                            { 
                            // Save the Expiration
                            expiration = new byte[] { interrogate_response[idLengthIndex + transaction_length + 8], 
                                                      interrogate_response[idLengthIndex + transaction_length + 9], 
                                                      interrogate_response[idLengthIndex + transaction_length + 10], 
                                                      interrogate_response[idLengthIndex + transaction_length + 11]  };

                            // Save the PoolID
                            poolID = new byte[] {  interrogate_response[idLengthIndex + transaction_length + 12] ,
                                                   interrogate_response[idLengthIndex + transaction_length + 13] };
                            }
                            catch
                            {

                            }
                            // Guardamos la transacción (la persistimos)
                            // Save the transaction (persist it)

                            transactionsController.AddTransaction(t);
                        }

                    }
                }
            }

          
            try { PhysicalLP72Response(transferStatus.Value, // transferStatus
                                       ReceiptStatus, // ReceiptStatus
                                       transaction_arr, // TransactionID
                                       _cashableAmount, // Cashable Amount
                                       _restrictedAmount, // Restricted Amount
                                       _nonrestrictedAmount, // Non Restricted Amount
                                       TransferFlags, // Transfer Flags
                                       TransferType, // Transfer Type
                                       expiration, // Expiration
                                       poolID, // PoolID
                                       Position, // Position
                                       TransactionDate, // TransactionDate
                                       AFTCurrentTransferInterrogated, // Current transfer Interrogation response
                                       e); }  catch {}

            if (AFTCurrentTransferInterrogated)
                AFTCurrentTransferInterrogated = false;              

            // Transición de Interrogated -> Acknowledged
            if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Acknowledged))
            {
                AFTCurrentTransaction.Instance().InternalStatus = GetByteFromArrayIndex(interrogate_response, 4).Value;
                AFTCurrentTransaction.Instance().Position = GetByteFromArrayIndex(interrogate_response, 3).Value;
                if (TransactionDate != null)
                    AFTCurrentTransaction.Instance().TransactionDate = TransactionDate.Value;
                if (TransactionID != null)
                    AFTCurrentTransaction.Instance().TransactionID = TransactionID;
                if (ReceiptStatus != null)
                    AFTCurrentTransaction.Instance().ReceiptStatus = ReceiptStatus.Value;
                if (RestrictedAmount != null)
                    AFTCurrentTransaction.Instance().RestrictedAmount = RestrictedAmount.Value;
                // Consultamos los meters y los créditos
                // We consult the meters and credits
                host.SendSelectedMeter(new byte[] { 0x0C, 0x1B });
                host.SendSelectedMeter(new byte[] { 0x00, 0x01, 0x02, 0x04, 0x05, 0x06, 0x0C, 0x13, 0x15, 0x16 });
                host.SendSelectedMeter(new byte[] { 0x17, 0x18, 0x1B, 0x24, 0x25, 0x26, 0x8C, 0x8D, 0x8E, 0x8F });
                host.SendSelectedMeter(new byte[] { 0x90, 0x91, 0x92, 0x93, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 });
                host.SendSelectedMeter(new byte[] { 0xA8, 0xA9, 0xAA, 0xAB, 0xAE, 0xAF, 0xB0, 0xB1, 0xB8, 0xB9 });
                host.SendSelectedMeter(new byte[] { 0xBA, 0xBB, 0xBC, 0xBD });
                SendStatusRequest();
                
                if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Completed))
                {
                    PhysicalAFTTransactionCompleted(e);
                }
            }

        }

        #endregion

        #region Exceptions

        /// <summary>
        /// A 3C event was received
        /// It is the event handler for OperatorChangedOptions3C from SASResponseHandler, when an exception 3C arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void operatorChangedOptions3C(EventArgs e)
        {
            // Send long poll 54 
            host.SendSASVersionAndMachineSerialNumber();
        }

        /// <summary>
        /// Se recibió un evento 3D
        /// A 3D event was received
        /// It is the event handler for CashoutTicketHasBeenPrinted3D from SASResponseHandler, when an exception 3D arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void CashoutTicketHasBeenPrinted3D(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp3D(e);

        }

        /// <summary>
        /// Se recibió la excepción 3F
        /// 3F exception received
        /// Relay event purpose
        /// It is the event handler for ValidationIDNotConfigured3F from SASResponseHandler, when a exception 3F arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void ValidationIDNotConfigured3F(EventArgs e)
        {
            // Relay this event to a upper level
            PhysicalExp3F(e);
        }



        /// <summary>
        ///  Se recibió un evento 3E
        ///  A 3E event was received
        ///  It is the event handler for HandpayValidated3E from SASResponseHandler, when an exception 3E arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void HandpayValidated3E(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp3E(e);
        }

        /// <summary>
        /// Se insertó en la EGM un billete
        /// A bill was inserted in the EGM
        /// It is the event handler for AnyBillInserted from SASResponseHandler, when a bill inserted exception arrives to Host (0x47 to 0x50).
        /// Only relay the event to an upper level
        /// </summary>
        /// <param name="e"></param>
        void anyBillInserted(EventArgs e)
        {
            host.SendTotalDollarValueOfBillsMeters(); // Long poll 20 
            host.SendSelectedMeter(new byte[] { 0x0B, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46 }); // Long poll 2F
            host.SendSelectedMeter(new byte[] { 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50 }); // Long poll 2F
            host.SendSelectedMeter(new byte[] { 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A }); // Long poll 2F
            host.SendSelectedMeter(new byte[] { 0x5B, 0x5C, 0x5D, 0x5E, 0x5F }); // Long poll 2F
            host.SendCredits(); // Consultamos los créditos //  We consult the credits // Long poll 2F
            host.SendLastBillAcceptedInformation(); // Long poll 48

        }

        /// <summary>
        /// Se recibió un handpay pending (excepción 51).
        /// A handpay pending was received (exception 51).
        /// It is the event handler for HandpayPending51 from SASResponseHandler, when a exception 51 arrives to Host.
        /// Only relay the event to an upper level
        /// </summary>
        /// <param name="e"></param>
        void handpayPending51(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp51(e);
        }


        /// <summary>
        /// Se recibió un handpay reseet (excepción 52)
        /// A handpay reseet (exception 52) was received.
        /// It is the event handler for HandpayReset52 from SASResponseHandler, when an exception 52 arrives to Host.
        /// Only relay the event to an upper level
        /// </summary>
        /// <param name="e"></param>
        void handpayReset52(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp52(e);
            // Consultamos los meters y los créditos
            // We consult the meters and credits
            host.SendMeters(); // Enviamos el 1C, recibimos sus meters // We sent the 1C, we received your meters 
            GetMeters(new byte[] { 0x03, 0x04, 0x13, 0x15, 0x16, 0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24,
                                       0x8C, 0x8D, 0x8E, 0x8F, 0x90, 0x91, 0x92, 0x93, 0x25, 0x26}); // Obtenemos los meters que figuran a continuación // We obtain the following meters 
            host.SendCredits(); // Consultamos los créditos // We consult the credits 
        }

        /// <summary>
        /// Se recibió un evento 57
        /// One event was received 57
        /// It is the event handler for ValidationNeeded57 from SASResponseHandler, when an exception 57 arrives
        /// </summary>
        /// <param name="e"></param>
        void SystemValidationNeeded57(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp57(e);
            //host.SendPendingCashoutInformation(); // Enviamos un lp57 (Pending Cashout Information)
        }


        /// <summary>
        /// Se recibió una exception de Ticket insertado 67
        /// An exception was received for Ticket inserted 67
        /// It is the event for TicketInserted67 from SASResponseHandler, when an 67 exception arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void TicketInserted67(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp67(e);
        }



        /// <summary>
        ///  Transferencia de ticket completada 68
        ///  Ticket transfer completed 68
        ///  It is the event  handler for TicketTransferCompleted68 from SASResponseHandler, when an exception 68 arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void TicketTransferCompleted68(EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExp68(e);

        }



        /// <summary>
        /// Se recibió un evento 69
        /// An event was received 69
        /// It is the event handler for TransferReady69 from SASResponseHandler. It is the handler when an exception 69 arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void transferCompleted69(EventArgs e)
        {
            PhysicalExp69(e);
            // Transición de Pending -> Interrogated
            if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Interrogated))
            {
                AFTInterrogate(); // e interrogamos
            }
        }


        /// <summary>
        /// Se recibió la excepción 8C
        /// Exception 8C was received
        /// Relay event purpose
        /// It is the event handler for SelectedGame8C from SASResponseHandler, when an exception 8C arrives to Host
        /// </summary>
        /// <param name="e"></param>
        void SelectedGame8C(EventArgs e)
        {
            // Relay this event to a upper level
            PhysicalExp8C(e);
        }

        /// <summary>
        /// Exception default
        /// By cases on exception code, we're doing some operations, for example for exception 12 (Door closed) occurs and then query the meter 26
        /// It is the event handler for DefaultException from SASResponseHandler, when an other exception arrives to Host
        /// </summary>
        /// <param name="exception">The exception Code</param>
        /// <param name="exceptionData">The exception data, generally empty if Real Time feature es disabled</param>
        /// <param name="e"></param>
        void DefaultException(byte exception, byte[] exceptionData, EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalExpDefault(exception, exceptionData, e);
            // By cases on exception code, we're doing some operations
            switch (exception)
            {
                case 0x12: // Door Closed
                    {
                        host.SendSelectedMeter(new byte[] { 0x26 });
                        break;
                    }
                case 0x7F: // Game ended
                    {

                        break;
                    }
                default:
                    {

                        break;
                    }
            }
        }




        /// <summary>
        /// Se recibió un evento de real time
        /// A real time event was received  
        /// Relay event purpose
        /// It is the event handler for RealTimeEvent from SASResponseHandler, when a exception arrives and a real time change is occurred
        /// </summary>
        /// <param name="rt">The byte rt, 01 if realtime is enabled, 00 if realtime is disabled</param>
        /// <param name="e"></param>
        void RealTimeEvent(byte rt, EventArgs e)
        {
            // Relay this event to a upper level
            PhysicalEGMRealTimeEvent(rt, e);
        }

        #endregion

        #region Long Poll Responses


        /// <summary>
        /// Se recibió la información de handpay
        /// Information received from handpay
        /// It is the event handler for HandpayInformationReceived from SASResponseHandler, when a lp 1B response arrives to Host
        /// </summary>
        /// <param name="progressiveGroup">Data from long poll response</param>
        /// <param name="level">Data from long poll response</param>
        /// <param name="amount">Data from long poll response</param>
        /// <param name="partialPay">Data from long poll response</param>
        /// <param name="resetID">Data from long poll response</param>
        /// <param name="e"></param>
        void HandpayInformationReceived(byte progressiveGroup,
                                        byte level,
                                        byte[] amount,
                                        byte[] partialPay,
                                        byte resetID, EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalLP1BResponse(progressiveGroup, level, amount, partialPay, resetID, e);
        }




        /// <summary>
        /// Se actualizó la información de gaming
        /// Gaming information has been updated
        /// It is the event handler for InfoUpdated from SASResponseHandler, when a long poll 1F response arrives to Host
        /// </summary>
        /// <param name="gi">The GamingInfo structure. It contains several info like _gameID, _additionalID, _denomination, _maxBet _progressiveGroup _gameOptions _paytableID, _basePercentage </param>
        /// <param name="e"></param>
        void infoUpdated(SASResponseHandler.GamingInfo gi, EventArgs e)
        {
            // Guardamos los datos en la physicalEGM
            // Save the data in the physicalEGM
            /// Update info at EGMSettings file and EGMStatus file in physicalEGM level
            physicalEGM.UpdateGamingInfo_GameID(gi._gameID, gi._additionalID, gi._denomination, gi._maxBet, gi._progressiveGroup, gi._gameOptions, gi._paytableID, gi._basePercentage);
            // Relay the event to an upper level
            PhysicalLP1FResponse(gi, e);
        }



        /// <summary>
        /// Se recibió una respuesta al lp 48
        /// A response was received to lp 48
        /// Update info itself and relay event to a upper level
        /// It is the event handler for SendLastBillAcceptedInformationResponse from SASResponseHandler, when a lp 48 response arrived to Host
        /// </summary>
        /// <param name="countryCode">Data from long poll 48 response</param>
        /// <param name="denominationCode">Data from long poll 48 response</param>
        /// <param name="billMeter">Data from long poll 48 response</param>
        /// <param name="e"></param>
        void SendLastBillAcceptedInformationResponse(byte countryCode,
                                                     byte denominationCode,
                                                     byte[] billMeter,
                                                     EventArgs e)
        {
            // Update this info in PhysicalEGM persistency
            /// Update info at EGMStatus file in physicalEGM level
            physicalEGM.SetLastAcceptedBillInformation(countryCode,
                                                       denominationCode,
                                                       billMeter);

            // Mando el evento PhysicalLP48Response al nivel superior del controller (Program)
            // I send the PhysicalLP48Response event to the upper level of the controller (Program).
            PhysicalLP48Response(countryCode,
                                 denominationCode,
                                 billMeter,
                                 e);
        }

        /// <summary>
        ///  Se recibió la respuesta al lp4D
        ///  Response to lp4D was received
        ///  It is the event handler for SendEnhancedValidationInformationResponse from SASResponseHandler, when a long poll 4D response arrives to Host
        /// </summary>
        /// <param name="validationType">Data from long poll 4D response</param>
        /// <param name="indexNumber">Data from long poll 4D response</param>
        /// <param name="date">Data from long poll 4D response</param>
        /// <param name="time">Data from long poll 4D response</param>
        /// <param name="validationNumber">Data from long poll 4D response</param>
        /// <param name="amount">Data from long poll 4D response</param>
        /// <param name="ticketNumber">Data from long poll 4D response</param>
        /// <param name="validationSystemId">Data from long poll 4D response</param>
        /// <param name="expiration">Data from long poll 4D response</param>
        /// <param name="poolId">Data from long poll 4D response</param>
        /// <param name="e"></param>
        void SendEnhancedValidationInformationResponse(byte validationType,
                                                    byte indexNumber,
                                                    byte[] date,
                                                    byte[] time,
                                                    byte[] validationNumber,
                                                    byte[] amount,
                                                    byte[] ticketNumber,
                                                    byte validationSystemId,
                                                    byte[] expiration,
                                                    byte[] poolId, EventArgs e)
        {
            // Instantiates a new Validation with the respective information from long poll
            Validation v = new Validation();
            v.validationType = validationType;
            v.indexNumber = indexNumber;
            v.date = date;
            v.time = time;
            v.validationNumber = validationNumber;
            v.ticketNumber = ticketNumber;
            v.validationSystemId = validationSystemId;
            v.expiration = expiration;
            v.poolId = poolId;
            v.amount = amount;
            // Insert it to the interfaced validation collection
            InterfacedValidation.Instance().InsertValidation(v);
            // Relay the event to an upper level
            PhysicalLP4DResponse(validationType, indexNumber, date, time, validationNumber, amount, ticketNumber, validationSystemId, expiration, poolId, e);
        }



        /// <summary>
        ///  Send Number Of Games Implemented
        ///  It is the event handler for SendNumberOfGamesImplemented from SASResponseHandler, when a long poll 51 response  arrives to Host
        /// </summary>
        /// <param name="numberOfGames">The number of games in a BCD format from long poll 51 response</param>
        /// <param name="e"></param>
        void SendNumberOfGamesImplemented(byte[] numberOfGames, EventArgs e)
        {
            // Relay the event to an upper level
            LaunchLog(new string[] { "EGM ->" }, $"Number of Games Implemented : {BitConverter.ToString(numberOfGames).Replace("-","")}", new EventArgs());
            PhysicalLP51Response(numberOfGames, e);
        }

        /// <summary>
        /// Se recibió una respuesta al lp 54
        /// A response was received to lp 54
        /// Update info itself and relay event to a upper level
        /// It is the event handler for SendSASVersionIDAndGamingMachineSerialNumber from SASResponseHandler, when a long poll 54 response arrives to Host
        /// </summary>
        /// <param name="SASVersion"></param>
        /// <param name="GMSerialNumber"></param>
        /// <param name="e"></param>
        void SendSASVersionIDAndGamingMachineSerialNumber(byte[] SASVersion, byte[] GMSerialNumber, EventArgs e)
        {
            if (!GMCurrentSerialNumber.SequenceEqual(GMSerialNumber))
            {
                GMCurrentSerialNumber = GMSerialNumber;
                LaunchLog(new string[] { "EGM ->" }, $"SerialNumber Received: {BitConverter.ToString(GMSerialNumber)}", new EventArgs());
            }
            // Update to physicalEGM
            ///  Update info at EGMInfo file in physicalEGM level
            physicalEGM.SetVersionID(SASVersion);
            /// Update info at EGMInfo file in physicalEGM level
            physicalEGM.SetGameMachineSerialNumber(GMSerialNumber);
            // Relay event to a upper level (controller)
            PhysicalLP54Response(SASVersion, GMSerialNumber, e);
        }





        /// <summary>
        /// Se recibió una respuesta al pedido de información de juego seleccionado (lp55)
        /// A response was received to the request for selected game information (lp55).
        /// It is the event handler for -sendSelectedGameNumber from SASResponseHandler, when a long poll 55 response arrives to Host
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="e"></param>
        void SendSelectedGameNumber(byte[] gameNumber, EventArgs e)
        {
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            /// Update info at EGMStatus file in physicalEGM level
            physicalEGM.SetCurrentGameNumber(gameNumber);
            // Relay this event to a upper level
            PhysicalLP55Response(gameNumber, e);
        }





        /// <summary>
        /// Se recibió una respuesta al pedido de información de juegos habilitados (lp56)
        /// A response was received to the request for information on enabled games (lp56).
        /// It is the event handler for SendEnabledGamesNumbers from SASResponseHandler, when long poll 56 response arrives to Host
        /// </summary>
        /// <param name="EnabledGames">A list with all enabled games from lp56 response</param>
        /// <param name="e"></param>
        void SendEnabledGamesNumbers(List<byte[]> EnabledGames, EventArgs e)
        {
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            ///  Update info at EGMSettings file in physicalEGM level
            physicalEGM.SetEnabledGameNumbers(EnabledGames);
            // Relay this event to a upper level
            PhysicalLP56Response(EnabledGames, e);
        }



        /// <summary>
        ///  Se recibió la respuesta al Pending Cashout Information (lp57)
        ///  Response to Pending Cashout Information (lp57) was received.
        ///  It is the event handler for PendingCashoutSent form SASResponseHandler, when a long poll 57 response arrives to Host
        /// </summary>
        /// <param name="info">PendingCashoutInformation, with cashouttype and amount </param>
        /// <param name="e"></param>
        void PendingCashoutSent(SASResponseHandler.PendingCashoutInformation info, EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalLP57Response(info, e);

        }

        /// <summary>
        /// Se recibió la respuesta al pedido de ValidationNumber (lp58)
        /// Response to ValidationNumber request (lp58) was received.
        /// It is the event handler for ValidationNumberReceived from SASResponseHandler, when a long poll 58 response arrives to Host
        /// </summary>
        /// <param name="status"></param>
        /// <param name="e"></param>
        void ValidationNumberReceived(byte status, EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalLP58Response(status, e);
        }


        /// <summary>
        ///  Se recibió por completo un 6f, se terminó de procesarse.
        ///  A 6f was received in full, processing was completed.
        ///  It is the event for E6FCompleted, when a long poll 6F response arrives.
        /// Only relay the event to an upper level
        /// </summary>
        /// <param name="e"></param>
        void e6FCompleted(EventArgs e)
        {
            // resta en uno el count6f, y cuando llega a 0, transiciona a Completed la current transaction
            // subtracts count6f by one, and when it reaches 0, transitions to Completed the current transaction
            count6f = count6f > 0 ? count6f - 1 : 0;
            if (count6f == 0)
            {
                // Empty, it is not necessary
            }
        }



        /// <summary>
        /// Validación de ticket recibida.
        /// Ticket validation received.
        /// It is the event handler for TicketValidationReceived from SASResponseHandler, when a long poll 70 response arrives to Host.
        /// Only relay the event to an upper level
        /// </summary>
        /// <param name="data">The Long poll 70 response structure, formed by ticketStatus, ticketAmount, parsingCode, validationData</param>
        /// <param name="e"></param>
        void TicketValidationReceived(SASResponseHandler.Response70Parameters data, EventArgs e)
        {
            // Relay the event to an upper level
            PhysicalLP70Response(data, e);

        }

                /// <summary>
        /// Ticket Redemption Data recibido
        /// It is the event handler for RedeemTicketReceived from SASResponseHandler, when a long poll 71 response arrives to Host
        /// Relays some events to the upper level based on machineStatus
        /// </summary>
        /// <param name="ticket">The RedeemTicket structure info, like machineStatus, amount, parsingCode, and validationData</param>
        /// <param name="e"></param>
        void RedeemTicketReceived(SASResponseHandler.RedeemTicket ticket, EventArgs e)
        {
            /// Success
            if (ticket.machineStatus == 0x00 || ticket.machineStatus == 0x01 || ticket.machineStatus == 0x02)
            {
                // Relay Succesful response event to an upper level
                PhysicalLP71_SuccesfulResponse(ticket, e);
            }
            // Pending
            else if (ticket.machineStatus == 0x40)
            {
                // Relay Pending response event to an upper level
                PhysicalLP71_40Response(ticket, e);
            }
            // Failed
            else if (ticket.machineStatus >= 0x80 && ticket.machineStatus <= 0x8B)
            {
                // Relay Failed response event to an upper level
                PhysicalLP71_FailedResponse(ticket, e);
            }
        }


        /// <summary>
        /// Se recibió una respuesta al lp 74
        /// A response was received to lp 74
        /// Update info itself and relay event to a upper level
        /// It is the event handler for AFTLockAndStatusRequestGamingMachineResponse from SASResponseHandler, when a lp 74 response arrived to Host
        /// </summary>
        /// <param name="assetNumber">Data from long poll response, please refer to protocol documentation</param>
        /// <param name="gameLockStatus">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="availableTransfers">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="hostCashoutStatus">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="AFTStatus">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="maxBufferIndex">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="currentCashableAmount">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="currentRestrictedAmount">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="currentNonRestrictedAmount">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="gamingMachineTransferLimit">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="restrictedExpiration">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="restrictedPoolId">Data from long poll 74 response, please refer to protocol documentation</param>
        /// <param name="e"></param>
        void AFTLockAndStatusRequestGamingMachineResponse(byte[] assetNumber,
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
                                                          EventArgs e)
        {
            maxBufferIndexForInterrogate = maxBufferIndex;
            physicalEGM.Update74ResponseInfo(assetNumber,
                availableTransfers,
                gameLockStatus,
                hostCashoutStatus,
                AFTStatus,
                restrictedExpiration,
                gamingMachineTransferLimit,
                maxBufferIndex,
                currentCashableAmount,
                currentRestrictedAmount,
                currentNonRestrictedAmount,
                restrictedPoolId);
            // Mando el evento PhysicalLP74Response al nivel superior del controller (Program)
            // I send the PhysicalLP74Response event to the upper level of the controller (Program).
            PhysicalLP74Response(assetNumber,
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

        /// <summary>
        /// Se recibió una respuesta al lp 73
        /// A response was received to lp 73
        /// Update info itself and relay event to a upper level
        /// It is the event handler for AFTRegisterGamingMachineRespoonse from SASResponseHandler, when a lp 73 response arrived to Host
        /// </summary>
        void AFTRegisterGamingMachineRespoonse(byte regStatus, byte[] assetNumber, byte[] registrationKey, byte[] regPosID, EventArgs e)
        {
            PhysicalLP73Response(regStatus, assetNumber, registrationKey, regPosID, e);
        }

        void ROMSignatureVerificationResponse(byte[] romSignature, EventArgs e)
        {
            PhysicalLP21Response(romSignature, e);
        }

        /// <summary>
        /// Se recibió una respuesta al lp 7B
        /// A response was received to lp 7B
        /// Update info itself and relay event to a upper level
        /// It is the event handler for ExtendedValidationStatusResponse from SASResponseHandler, when a lp 7B response arrived to Host
        /// </summary>
        /// <param name="assetNumber">Data from long poll 7B response</param>
        /// <param name="statusBits">Data from long poll 7B response</param>
        /// <param name="cashableTicketAndReceiptExpiration">Data from long poll 7B response</param>
        /// <param name="restrictedTicketDefaultExpiration">Data from long poll 7B response</param>
        /// <param name="e"></param>
        void ExtendedValidationStatusResponse(byte[] assetNumber,
                                              byte[] statusBits,
                                              byte[] cashableTicketAndReceiptExpiration,
                                              byte[] restrictedTicketDefaultExpiration,
                                              EventArgs e)
        {
            // Manda los datos de la 7B en la PhysicalEGM
            // Send the data of the 7B in the PhysicalEGM
            /// Update info at EGMSettings file in physicalEGM level
            physicalEGM.SetExtendedValidationStatus(assetNumber, statusBits, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration);

            // Mando el evento PhysicalLP7BResponse al nivel superior del controller (Program)
            // I send the PhysicalLP7BResponse event to the upper level of the controller (Program).
            PhysicalLP7BResponse(assetNumber,
                                 statusBits,
                                 cashableTicketAndReceiptExpiration,
                                 restrictedTicketDefaultExpiration,
                                 e);
        }


        /// <summary>
        ///  Se recibió una respuesta al pedido de fecha y hora de la EGM
        ///  A response to the EGM date and time request was received.
        ///  Parse the arguments byte array, set the date time itself and relay event
        ///  It is the event handler for SendDateTimeGamingMachineResponse from SASResponseHandler, when a long poll 7E response arrives to Host
        /// </summary>
        /// <param name="date">date byte array in a BCD format from response</param>
        /// <param name="time">time byte array in a BCD format from response</param>
        /// <param name="e"></param>
        void SendDateTimeGamingMachineResponse(byte[] date, byte[] time, EventArgs e)
        {
            // Parseamos todos los elementos de la fecha
            // Parse all date elements
            int month = int.Parse(BitConverter.ToString(new byte[] { date[0] }).Replace("-", ""));
            int day = int.Parse(BitConverter.ToString(new byte[] { date[1] }).Replace("-", ""));
            int year = int.Parse(BitConverter.ToString(new byte[] { date[2], date[3] }).Replace("-", ""));
            int hour = int.Parse(BitConverter.ToString(new byte[] { time[0] }).Replace("-", ""));
            int minute = int.Parse(BitConverter.ToString(new byte[] { time[1] }).Replace("-", ""));
            int second = int.Parse(BitConverter.ToString(new byte[] { time[2] }).Replace("-", ""));
            //Seteo el date en physicalEGM
            //Setting the date in physicalEGM
            ///  Update info at EGMStatus file in physicalEGM level
            physicalEGM.SetDateAndTime(day, month, year, hour, minute, second);
            // Lanzamos el evento LP7E Response
            // We launched the LP7E Response event to a upper level
            PhysicalLP7EResponse(month, day, year, hour, minute, second, e);
            // Reseteamos el timer de 30 segundos
            // Reset the 30-second timer
            Timer30SecondsInitiated = false;
        }



        /// <summary>
        ///  Se recibió una exception 7F
        ///  An exception 7F was received
        ///  It is the event for Spin7F from SASResponseHandler, when a long poll 7F response arrives to Host
        ///  Sends the long poll 1C, query specific meters (07, 22, 25, 26) and send credits
        /// </summary>
        /// <param name="e"></param>
        void spin7F(EventArgs e)
        {
            host.SendMeters(); // Enviamos el 1C, recibimos sus meters // We sent the 1C, we received your meters 
            host.SendSelectedMeter(new byte[] { 0x07, 0x22, 0x25, 0x26 });
            host.SendCredits(); // Consultamos los créditos // We Check the credtis
            host.SendLP74(0xFF, 0x00, new byte[] { 0x00, 0x00 });  // Send the long poll 74
            host.SendSelectedMeter(new byte[] { 0x0C, 0x1B }); // Send the selected meters 0C and 1B
        }



        /// <summary>
        ///  Se recibió la respuesta al Send Enabled Features (lpA0)
        ///  The response to Send Enabled Features (lpA0) was received.
        ///  It is the event handler for SendEnabledFeaturesResponse of SASResponseHandler. This the event when lpA0 response arrives to Host
        /// </summary>
        /// <param name="gameNumber">Data from long poll response</param>
        /// <param name="features1">Data from long poll response</param>
        /// <param name="features2">Data from long poll response</param>
        /// <param name="features3">Data from long poll response</param>
        /// <param name="e"></param>
        void SendEnabledFeaturesResponse(byte[] gameNumber,
                                        byte features1,
                                        byte features2,
                                        byte features3,
                                        EventArgs e)
        {
            // Relay the event to upper level
            PhysicalLPA0Response(gameNumber,
                                features1,
                                features2,
                                features3,
                                e);
        }




        /// <summary>
        ///  Se recibió una respuesta al pedido de cashoutlimit
        ///  A response to cashoutlimit's request was received.
        ///  It is the event handler for CashoutLimitReceived from SASResponseHandler, when a long poll A4 response arrives to Host
        /// </summary>
        /// <param name="gameNumber">The game number which the cashout limit belongs</param>
        /// <param name="cashoutLimit">The cashout limit in a BCD format</param>
        /// <param name="e"></param>
        void CashoutLimitReceived(byte[] gameNumber, byte[] cashoutLimit, EventArgs e)
        {
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            ///  Update info at EGMSettings file in physicalEGM level
            physicalEGM.UpdateCashoutLimit(gameNumber, cashoutLimit);
            // Relay this event to a upper level
            PhysicalLPA4Response(gameNumber, cashoutLimit, e);
        }





        /// <summary>
        /// Se recibió una respuesta al pedido de la denominación del actual jugador (lpB1)
        /// A response to the request for the current player's name (lpB1) was received.
        /// Set itself all info about current player denominations
        /// It is the event handler for SendCurrentPlayerDenomination from SASResponseHandler, when a long poll B1 response arrived to Host
        /// </summary>
        /// <param name="currentPlayerDenomination">The current player denomination byte code</param>
        /// <param name="e"></param>
        void SendCurrentPlayerDenomination(byte currentPlayerDenomination, EventArgs e)
        {
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            /// Update info at EGMStatus file in physicalEGM level
            physicalEGM.SetCurrentPlayerDenomination(currentPlayerDenomination);
            // Relay this event to a upper level
            PhysicalLPB1Response(currentPlayerDenomination, e);

        }



        /// <summary>
        ///  Se recibió una respuesta al pedido de las denominaciones habilitadas
        ///  A response to the request for qualified denominations was received.
        ///  Set itself all info about denominations
        ///  It is the event handler for SendEnabledPlayerDenominations from SASResponseHandler, when a long poll B2 response arrives to Host
        /// </summary>
        /// <param name="NumberOfDenominations">Data from Longpoll B2 response</param>
        /// <param name="PlayerDenominations">Data from Longpoll B2 response</param>
        /// <param name="e"></param>
        void SendEnabledPlayerDenominations(byte NumberOfDenominations, byte[] PlayerDenominations, EventArgs e)
        {
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            /// Update info at EGMStatus file in physicalEGM 
            physicalEGM.SetDenominations(NumberOfDenominations, PlayerDenominations);
            // Relay this event to a upper level
            PhysicalLPB2Response(NumberOfDenominations, PlayerDenominations, e);
        }


        /// <summary>
        /// Se recibió una respuesta al pedido de la denominación token
        /// A response to the request for the token denomination was received.
        /// Set itself the token denomination and relay event
        /// It is the event for SendTokenDenomination from SASResponseHandler, when a long poll B3 response arrives to Host
        /// </summary>
        /// <param name="TokenDenomination"></param>
        /// <param name="e"></param>
        void SendTokenDenomination(byte TokenDenomination, EventArgs e)
        {
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            /// Update info at EGMStatus file in physicalEGM level
            physicalEGM.SetTokenDenomination(TokenDenomination);

            // Relay this event to a upper level
            PhysicalLPB3Response(TokenDenomination, e);
        }

        /// <summary>
        /// A response was received to lp B4
        /// Update info itself and relay event to a upper level
        /// It is the event handler for SendWagerCategoryInformationResponse from SASResponseHandler, when a long poll B4 response arrives to Host
        /// </summary>
        /// <param name="gameNumber">Data from long poll B4 response</param>
        /// <param name="wagerCategory">Data from long poll B4 response</param>
        /// <param name="paybackPercentage">Data from long poll B4 response</param>
        /// <param name="coinInMeterValue">Data from long poll B4 response</param>
        /// <param name="e"></param>
        void SendWagerCategoryInformationResponse(byte[] gameNumber,
                                                  byte[] wagerCategory,
                                                  byte[] paybackPercentage,
                                                  byte[] coinInMeterValue,
                                                  EventArgs e)
        {

        }

        /// <summary>
        /// Se recibió una respuesta al lp B5
        /// A response was received to lp B5
        /// Update info itself and relay event to a upper level
        /// It is the event handler for SendGameNExtendedInformationResponse from SASResponseHandler, when a long poll B5 response arrives to Host
        /// </summary>
        /// <param name="gameNumber">Data from long poll 54 response</param>
        /// <param name="maxBet">Data from long poll 54 response</param>
        /// <param name="progressiveGroup">Data from long poll 54 response</param>
        /// <param name="progressiveLevels">Data from long poll 54 response</param>
        /// <param name="gameNameLength">Data from long poll 54 response</param>
        /// <param name="gameName">Data from long poll 54 response</param>
        /// <param name="paytableLength">Data from long poll 54 response</param>
        /// <param name="paytableName">Data from long poll 54 response</param>
        /// <param name="wagerCategories">Data from long poll 54 response</param>
        /// <param name="e"></param>
        void SendGameNExtendedInformationResponse(byte[] gameNumber,
                                                  byte[] maxBet,
                                                  byte progressiveGroup,
                                                  byte[] progressiveLevels,
                                                  byte gameNameLength,
                                                  byte[] gameName,
                                                  byte paytableLength,
                                                  byte[] paytableName,
                                                  byte[] wagerCategories,
                                                  EventArgs e)
        {

            // Guardamos los datos en la physicalEGM
            // Save the data in the physicalEGM
            /// Update info at EGMInfo file and EGMSettings file in physicalEGM level
            physicalEGM.UpdateGamingNInfo(gameNumber, maxBet, progressiveGroup, progressiveLevels, gameNameLength, gameName, paytableLength, paytableName, wagerCategories);


            // Mando el evento PhysicalLPB5Response al nivel superior del controller (Program)
            // I send the PhysicalLPB5Response event to the upper level of the controller (Program).
            PhysicalLPB5Response(gameNumber,
                                 maxBet,
                                 progressiveGroup,
                                 progressiveLevels,
                                 gameNameLength,
                                 gameName,
                                 paytableLength,
                                 paytableName,
                                 wagerCategories,
                                 e);
        }



        /// <summary>
        ///  Se recibe cierto long poll
        ///  A certain long poll is received
        ///  It is the event handler for LPReceived from SASResponseHandler, when a custom long poll response arrives to Host
        ///  Based on long poll code, do some executions
        /// </summary>
        /// <param name="lpcode">The long poll code</param>
        /// <param name="e"></param>
        void LPReceived(byte lpcode, EventArgs e)
        {
            if (lpcode == 0x2F)
            {
                if (TimerMetersInitiated)
                {
                    TimerMetersInitiated = false; // Resetea la bandera
                }
            }
        }
        #endregion


















        #endregion

        #region Host Events
        //
        /// <summary>
        ///  Un comando fué enviado a la EGM. La data consta del comando, si cumple con crc y si se envía como retry
        ///  A command was sent to the EGM. The data consists of the command, whether it complies with crc and whether it is sent as a retry.
        ///  It is the event handler for CommandSent from SASHost (host), when a long poll is sent to long poll
        ///  Transition the current long poll state machine if it is the current long poll sent to EGM
        ///  Logging trace if it is active
        /// </summary>
        /// <param name="cmd">The full long poll as string (bytes separated with -)</param>
        /// <param name="crc">The crc flag</param>
        /// <param name="isRetry">The retry flag (indicates if the last commnad sent is the current cmd</param>
        /// <param name="e"></param>
        void cmdSent(string cmd, bool crc, bool isRetry, EventArgs e)
        {
            // If current long poll of the state machine is the cmd long poll,transition state to LongPollSentToEGM
            if (SendingLongPollSM.Instance().longpoll == cmd)
            {
                SendingLongPollSM.Instance().Transition(SendingLongPollSMStatus.LongPollSentToEGM);
            }
            // If logTrace is not null (trace loggin is active), add a trace with Sent type
            if (logTracer != null)
            {
                if (logTracer.Logger != null)
                    if (cmd.Split("-").Length > 1)
                        logTracer.Logger.AddTrace(cmd, "Sent", crc, isRetry);
            }
        }



        /// <summary>
        /// Un comando fué recibido a la EGM
        /// A command was received at the EGM
        /// It is the event handler for CommandReceived from SASHost (host), when a long poll is received from EGM
        /// It checks if the machine state is not in a Idle State, checks that cmd response belongs to the current sent long poll, and Transitioning to specific states
        /// Stamping the timestamp of last EGM response 
        ///  Logging trace if it is active
        /// </summary> 
        /// <param name="cmd"></param>
        /// <param name="crc"></param>
        /// <param name="e"></param>
        void cmdReceived(string cmd, bool crc, EventArgs e)
        {
            // Check that the machine state is not in a Idle State
            if (SendingLongPollSM.Instance().status != SendingLongPollSMStatus.Idle)
            {
                // Checks that cmd response belongs to the current sent long poll and transitioning to specific states
                if (IsResponse(cmd, SendingLongPollSM.Instance().longpoll))
                    SendingLongPollSM.Instance().Transition(SendingLongPollSMStatus.LongPollResponseSuccesfull);
                else
                    SendingLongPollSM.Instance().Transition(SendingLongPollSMStatus.LongPollNoResponse);
            }
            // Stamping the timestamp of last EGM response
            if (crc)
                LastEGMResponseReceivedAt = DateTime.Now;
            // Logging trace if it is active
            if (logTracer != null)
            {
                if (logTracer.Logger != null)
                    if (cmd.Split("-").Length >= 1 && cmd != "00")
                        logTracer.Logger.AddTrace(cmd, "Received", crc, false);
            }
        }

        /// <summary>
        /// En deshuso, pero se necesitaría en un futuro
        /// In disuse, but would be needed in the future.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="e"></param>
        static void dataReceived(string cmd, EventArgs e) { }

        /// <summary>
        /// Un evento de comunicación se recibió, con parámetro true si hay comunicación con la EGM, o false si no hay comunicación con la EGM
        /// A communication event was received, with parameter true if there is communication with the EGM, or false if there is no communication with the EGM.
        /// It is the event handler for CommunicationEvent from SASHost (host), when the EGM link communication changed (to down or up).
        /// </summary>
        /// <param name="communication">the communication flag, true if EGM link is on, false if EGM link is off</param>
        /// <param name="e"></param>
        void communicationEvent(bool communication, EventArgs e)

        {
            // Relay this event with flag parameter to a upper level (controller)
            PhysicalEGMCommunicationEvent(communication, e);

        }

        #endregion

        #endregion





        /**************************************************************************************/
        /**************************************************************************************/
        /**********************************    QUERIES    *************************************/
        /**************************************************************************************/
        /**************************************************************************************/

        #region QUERIES
        // Acá brindamos los diferentes métodos para la PhysicalEGM
        // Here are the different methods for the PhysicalEGM

        /*************************************************/
        /************** PARA LA PHYSICAL EGM**************/
        /*************************************************/
        #region "ForPhysicalEGM"

        /// <summary>
        /// Se actualiza la data del ticket
        /// Ticket data is updated
        /// Update info at EGMSettings file in physicalEGM level
        /// It is used at InterfacedLP7C and InterfacedLP7D AT CONTROLLER LEVEL, when LP7C and lp7D arrives to Client
        /// </summary>
        /// <param name="code">The info code to update or overwrite. Depending on code, it can be LocationName, LocationAddress1, LocationAddress2, RestrictedTicketTitle or DebitTicketTitle</param>
        /// <param name="data">The info data</param>
        public void UpdateTicketData(byte code, string data)
        {
            // Call this method to physicalEGM model
            physicalEGM.UpdateTicketData(code, data);
        }
        // 
        /// <summary>
        /// Se actualiza el ValidationType
        /// ValidationType is updated
        /// Update info at EGMSetings level in physicalEGM
        /// It is used at PhysicalLPA0Response AT CONTROLLER LEVEL, when a long poll A0 response arrives to Host
        /// </summary>
        /// <param name="type">The validation type</param>
        public void SetValidationType(int type)
        {
            physicalEGM.SetValidationType(type);
        }


        /// <summary>
        /// Se actualiza la validation extension
        /// Validation extension updated
        /// Update info at EGMSettings file in physicalEGM level
        /// It is used at SetValidationExtensions AT CONTROLLER LEVEL, with no reference to that method
        /// </summary>
        /// <param name="b">The flag to enable validation extensions or disable</param>
        public void SetValidationExtensions(bool b)
        {
            physicalEGM.SetValidationExtensions(b);
        }


        // 
        /// <summary>
        /// Se actualiza o setea la cantidad de juegos
        /// The number of games is updated or set.
        /// Update info at EGMInfo file in physicalEGM level
        /// It is used at PhysicalLP51Resp AT CONTROLLER LEVEL, when a long poll 51 response arrives to Host, processed by the current PhysicalEGMController and relayed to controller
        /// </summary>
        /// <param name="numberOfGames">The number of games in BCD format, please refer to protocol documentation</param>
        public void SetNumberOfGamesImplemented(byte[] numberOfGames)
        {
            physicalEGM.SetNumberOfGames(numberOfGames);
        }

        /// <summary>
        /// Reset meters at EGMAccounting file in physicalEGM level
        /// It is used at RAMClear AT CONTROLLER LEVEL, with no reference to that method
        /// </summary>
        public void RAMClear()
        {
            physicalEGM.RAMClear();
        }


        /// <summary>
        /// Se actualiza o setea las features de la PhysicalEGM
        /// PhysicalEGM features are updated or set.
        /// Update info at EGMSettings file in physicalEGM level
        /// It is used at PhysicalLPA0Response AT CONTROLLER LEVEL, when a long poll A0 response arrives to Host
        /// </summary>
        /// <param name="feat1">features1 byte, please refer to protocol documentation</param>
        /// <param name="feat2">features2 byte, please refer to protocol documentation</param>
        /// <param name="feat3">features3 byte, please refer to protocol documentation</param>
        public void SetFeatures(byte feat1, byte feat2, byte feat3)
        {
            physicalEGM.SetFeatures(feat1, feat2, feat3);
        }


        /// <summary>
        ///  Obtener el selected game number
        ///  Obtain the selected set number
        ///  Get info from EGMStatus file in physicalEGM levelç
        ///  It is used frequently every 30 second
        /// </summary>
        /// <returns>Game Selected number in byte array format</returns>
        public byte[] GetSelectedGameNumber()
        {
            return physicalEGM.GetSelectedGame();
        }

         /// <summary>
        ///  Get features1 from EGMSettings file in physicalEGM level
        /// It is used by GetPhysicalEGMSettings method at controlle level, when api request the PhysicalEGMSettigns
        /// </summary>
        public byte GetFeatures1()
        {
            return physicalEGM.GetEGMSettings().features1;
        }

        /// <summary>
        ///  Get features2 from EGMSettings file in physicalEGM level
        /// It is used by GetPhysicalEGMSettings method at controlle level, when api request the PhysicalEGMSettigns
        /// </summary>
        public byte GetFeatures2()
        {
            return physicalEGM.GetEGMSettings().features2;
        }

        /// <summary>
        ///  Get features3 from EGMSettings file in physicalEGM level
        /// It is used by GetPhysicalEGMSettings method at controlle level, when api request the PhysicalEGMSettigns
        /// </summary>
        public byte GetFeatures3()
        {
            return physicalEGM.GetEGMSettings().features3;
        }



        #endregion

        /*************************************************/
        /****************  PARA EL HOST ******************/
        /*************************************************/

        #region "ForHost"

        /// <summary>
        /// Enviar los meters seleccionados como argumento para el game 0;
        /// Send the selected meters as argument for game 0;
        /// The long poll 2F; It is mainly used at RecipeGetMetersMachine, executed in TimerStartCheckMeters every 1 second
        /// </summary>
        /// <param name="meters">The byte array with each element containing the meter code</param>
        public void SendSelectedMeter(byte[] meters)
        {
            // Obtenemos todos los meters 
            // We obtain all the meters 
            int k = 1;
            byte[] submeters = meters.Take(10).ToArray();
            while (submeters.Count() > 0)
            {
                host.SendSelectedMeter(submeters);
                submeters = meters.Skip(k * 10).Take(10).ToArray();
                k++;
            }
        }

        /// <summary>
        /// Enviar los meters seleccionados como argumento para el game N;
        /// Send the selected meters as argument for the game N;
        /// The long poll 2F; It is mainly used at RecipeGetMetersMultiGame, executed every seconds
        /// </summary>
        /// <param name="gameNumber">The game number in a BCD format</param>
        /// <param name="meters">The byte array with each element containing the meter code</param>
        public void SendSelectedMeter(byte[] gameNumber, byte[] meters)
        {
            // Obtenemos todos los meters 
            // We obtain all the meters 
            int k = 1;
            byte[] submeters = meters.Take(10).ToArray();
            while (submeters.Count() > 0)
            {
                host.SendSelectedMeter(gameNumber, submeters);
                submeters = meters.Skip(k * 10).Take(10).ToArray();
                k++;
            }
        }

        /// <summary>
        /// LP06;
        /// It is used AT CONTROLLER LEVEL, if the LP06interfacing flag  is enabled when long poll 06 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        public void EnableBillValidator()
        {
            host.EnableBillValidator();
        }


        /// <summary>
        /// LP07;
        /// It is used AT CONTROLLER LEVEL, if the LP07Interfacing flag  is enabled when long poll 07 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        public void DisableBillValidator()
        {
            host.DisableBillValidator();
        }


        /// <summary>
        /// LPA4;
        /// It is used AT CONTROLLER LEVEL, when a long poll 51 response arrives to Host;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendCashoutLimit(byte[] gameNumber)
        {
            host.SendCashoutLimit(gameNumber);
        }


        /// <summary>
        /// LPA8;
        /// It is used AT CONTROLLER LEVEL, if the LPA8Interfacing flag  is enabled when long poll A8 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="resetMethod">Longpoll A8 parameter, please refer to protocol documentation</param>
        public void EnableJackpotHandpayResetMethod(byte resetMethod)
        {
            host.EnableJackpotHandpayResetMethod(resetMethod);
        }

        /// <summary>
        /// LP03;
        /// It is used AT CONTROLLER LEVEL, if the LP03Interfacing flag  is enabled when long poll 03 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        public void SoundOff()
        {
            host.SoundOff();
        }

        /// <summary>
        /// LP04;
        /// It is used AT CONTROLLER LEVEL, if the LP04Interfacing flag  is enabled when long poll 04 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        public void SoundOn()
        {
            host.SoundOn();
        }

        /// <summary>
        /// LP0E;
        /// It is used AT CONTROLLER LEVEL, if the LP0EInterfacing flag  is enabled when long poll 0E arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="enable_disable">byte 01 to enable, 00 to disable</param>
        public void EnableDisableRealTimeEvent(byte enable_disable)
        {
            host.EnableDisableRealTimeEvent(enable_disable);
        }

        /// <summary>
        /// LP21;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="seedValue">ROM Verification seed value</param>
        public void SendROMSignatureVerification(byte[] seedValue)
        {
            host.SendROMSignatureVerification(seedValue);
        }

        /// <summary>
        /// LP4C;
        /// It is used AT CONTROLLER LEVEL, if the LP4CInterfacing is enabled when long poll 4C arrives to client;
        /// Also, it is used AT CONTROLLER LEVEL with machineID fixed to 00 00 01 and sequenceNumber fixed to 00 00 00, when the physical exception 3F arrives to Host;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="machineID">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="sequenceID">Long poll parameter, please refer to protocol documentation</param>
        public void SetSecureEnhancedValidationID(byte[] machineID, byte[] sequenceID)
        {
            host.SetSecureEnhancedValidationiID(machineID, sequenceID);
        }

        /// <summary>
        /// LP7F;
        /// It is used AT CONTROLLER LEVEL, if the LP7FInterfacing flag  is enabled when long poll 7F arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="date">Long poll parameter as byte array, please refer to protocol documentation</param>
        /// <param name="time">Long poll parameter as byte array, please refer to protocol documentation</param>
        public void ReceiveDateAndTime(byte[] date, byte[] time)
        {
            host.ReceiveDateAndTime(date, time);
        }

        /// <summary>
        /// LP94;
        /// It is used AT CONTROLLER LEVEL, if the LP94Interfacing flag  is enabled when long poll 94 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        public void ResetHandpayGamingMachine()
        {
            host.ResetHandpay();
        }


        /// <summary>
        /// We set the data for the ticket;
        /// LP7C
        /// It is used AT CONTROLLER LEVEL, if the LP7CInterfacing flag  is enabled when long poll 7C arrives to client;
        /// Send the long poll request to Host
        /// </summary>
        /// <param name="code">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="data">Long poll parameter, please refer to protocol documentation</param>
        public void SetExtendedTicketData(byte code, string data)
        {
            host.SetExtendedTicketData(code, data);
        }

        // Se pide el current status de la EGM
        // The current status of the EGM is requested
        /// <summary>
        /// LP74 Current Status
        /// </summary>
        public void SendStatusRequest()
        {
            host.SendLP74(0xFF, 0x00, new byte[] { 0x00, 0x00 });
        }

        /// <summary>
        /// LP7D;
        /// It is used AT CONTROLLER LEVEL, if the LP7DInterfacing flag  is enabled when long poll 7D arrives to client;
        /// Send the long poll request to Host
        /// </summary>
        /// <param name="HostId">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="expiration">Long poll parameter  please refer to protocol documentation</param>
        /// <param name="location">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="address1">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="address2">Long poll parameter, please refer to protocol documentation</param>
        public void SetTicketData(byte[] HostId,
                                  byte expiration,
                                  byte[] location,
                                  byte[] address1,
                                  byte[] address2)
        {
            host.SetTicketData(HostId, expiration, location, address1, address2);
        }

        /// <summary>
        /// LP02;
        /// It is used AT CONTROLLER LEVEL, if the LP02Interfacing flag is enabled when long poll 02 arrives to client;
        /// Send the long poll request to Host
        /// </summary>
        public void EnableGame()
        {
            host.EnablePlay();
        }

        /// <summary>
        /// LP01;
        /// It is used AT CONTROLLER LEVEL, if the LP01Interfacing flag is enabled when long poll 01 arrives to client;
        /// Send the long poll request to Host
        /// </summary>
        public void LockoutGame()
        {
            host.LockOutPlay();
        }

        /// <summary>
        /// LP08;
        /// It is used at CONTROLLER LEVEL, if the LP08Interfacing flag is enabled when long poll 08 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="billDenominations">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="billAcceptorFlag">Long poll parameter, please refer to protocol documentation</param>
        public void ConfigureBillDenominations(byte[] billDenominations, byte billAcceptorFlag)
        {
            host.SendConfigureBillDenominationsLongPollCommand(billDenominations, billAcceptorFlag);
        }

        /// <summary>
        /// LP1C;
        /// It is used at CONTROLLER LEVEL, at several exceptios handlers like 3D and 3E. These exceptions are captured by host and processed by SASResponseHandler;
        /// Send the long poll request to Host;
        /// </summary>
        public void SendMeters()
        {
            host.SendMeters();
        }

        /// <summary>
        /// LP72 Interrogate
        /// It is used at CONTROLLER LEVEL, at several points where we request to send the AFT Interrogate. After that, we set a boolean 'AFTInterrogationSent'
        /// </summary>
        public void AFTInterrogate()
        {
           host.AFTInit();
           AFTCurrentTransferInterrogated = true;
        }



        // Region RedemptionBehaviour
        #region RedemptionBehaviour

        /// <summary>
        /// LP70;
        /// It is used AT CONTROLLER LEVEL, at 67 exception handler. That exception is captured by host and processed by SASResponseHandler;
        /// Send the long poll request to Host;
        /// </summary>
        public void RedemptionSendTicketValidationData()
        {
            host.SendTicketValidationData();
        }

        /// <summary>
        ///  LP71 + LP2F + LP1C;
        ///  It is used AT CONTROLLER LEVEL, at 68 exception handler. That exception is captured by host and processed by SASResponseHandler;
        ///  Send the long poll request to Host;
        /// </summary>
        public void RedemptionSendtRedeemTicketCommand()
        {
            host.SendtRedeemTicketCommand(0xFF);
            GetMeters(new byte[] { 0x04, 0x0D, 0x11, 0x13, 0x15, 0x16, 0x24, 0x28, 0x29, 0x2A, 0x2B, 0x2C,
                                                   0x2D, 0x35, 0x36, 0x37, 0x38, 0x39, 0x80, 0x81, 0x82, 0x83, 0x84,
                                                   0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                                                   0x90, 0x91, 0x92, 0x93 }); // Obtenemos los meters que figuran a continuación // We get the meters that appear below
            host.SendCredits(); // Consultamos los créditos //  We consult the credits
        }

        /// <summary>
        /// LP71;
        /// It is used AT CONTROLLER LEVEL, if the LP71AcceptingInterfacing flag is enabled when long poll 71 arrives to client;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="transferCode">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="amount">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="parsingCode">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="validationData">Long poll parameter, please refer to protocol documentation</param>
        public void RedemptionSendtRedeemTicketCommand(byte transferCode, int amount, byte parsingCode, byte[] validationData)
        {
            // Hacer algo en PhyisicalEGM //    Do something in PhyisicalEGM
            host.SendtRedeemTicketCommand(transferCode, (uint)amount, parsingCode, validationData);
        }

        #endregion

        // Region ValidationBehaviour
        #region "ValidationBehaviour"

        /// <summary>
        /// LP57;
        /// It is used AT CONTROLLER LEVEL, at 57 exception handler. That exception is captured by host and processed by SASResponseHandler;
        /// Send the long poll request to Host;
        /// </summary>
        public void ValidationSendPendingCashoutInformation()
        {
            host.SendPendingCashoutInformation(); // Enviamos un lp57 (Pending Cashout Information) // We send a lp57 (Pending Cashout Information)
        }

        /// <summary>
        /// LP58;
        /// It is used AT CONTROLLER LEVEL, if the LP58Interfacing flag is enabled when long poll 58 arrives to Host;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="validationSystemID">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="validationNumber">Long poll parameter, please refer to protocol documentation</param>
        public void ValidationSendReceiveValidationNumber(byte validationSystemID, byte[] validationNumber)
        {
            // Enviamos un lp58
            // We send a lp58
            host.SendReceiveValidationNumber(validationSystemID, validationNumber);
        }

        /// <summary>
        /// LP4D;
        /// The long poll 4D; It is mainly used at RecipeShortValidationBuffer and RecipeValidationBuffer, executed when a long poll A0 response or long poll 4D response with succesfull status arrive to Host;
        /// Part of the validation process
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="s">functionCode</param>
        public void ValidationSendEnhancedValidationInformationFunctionCode(byte s)
        {
            host.SendEnhancedValidationInformation(s); // enviamos un lp4D                      
        }


        /// <summary>
        ///  LP4D + LP2F + LP1C;
        ///  It is used AT CONTROLLER LEVEL, at 3D and 3E exceptions handlers. These exceptions are captured by host and processed by SASResponseHandler;
        ///  Send the long poll 4D, 2F and 1C requests to Host
        /// </summary>
        /// <param name="s">4D parameter (functionCode)</param>
        public void ValidationSendEnhancedValidationInformation(byte s)
        {
            host.SendEnhancedValidationInformation(s); // enviamos un lp4D // We send a lp4D
                                                       // Consultamos los meters y los créditos //  We consult the meters and the credits
            GetMeters(new byte[] { 0x03, 0x04, 0x0E, 0x12, 0x13, 0x15, 0x16, 0x23, 0x24, 0x28, 0x29, 0x2A, 0x2B, 0x2C,
                                                   0x2D, 0x35, 0x36, 0x37, 0x38, 0x39, 0x80, 0x81, 0x82, 0x83, 0x84,
                                                   0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                                                   0x90, 0x91, 0x92, 0x93 }); // Obtenemos los meters que figuran a continuación // We get the meters that appear below
            host.SendCredits(); // Consultamos los créditos ... //  We consult the credits ...
        }

        /// <summary>
        /// Execution of recipe 'Get all validation tickets'
        /// </summary>
        public void ValidationGetAllTickets()
        {
            Recipe getvalidationbuffer = new Recipe("ValidationBuffer"); // Instancio una nueva recipe // I instantiate a new recipe
            getvalidationbuffer.Instance.Init(this);
            // Ejecutar la recipe, esperando el status // Run the recipe, waiting for the status
            ActionStatus status = getvalidationbuffer.Instance.Execute();
        }

        /// <summary>
        /// Execution of recipe 'Get  validation tickets from index 4 to 1'
        /// </summary>
        public void ValidationGetTicketsFromIndex4To1()
        {
            Recipe getshortvalidationbuffer = new Recipe("ShortValidationBuffer"); // Instancio una nueva recipe // I instantiate a new recipe
            getshortvalidationbuffer.Instance.Init(this);
            // Ejecutar la recipe, esperando el status // Run the recipe, waiting for the status
            ActionStatus status = getshortvalidationbuffer.Instance.Execute();
        }
        #endregion

        // Region Handpay
        #region "Handpay"

        /// <summary>
        /// LP1B;
        /// It is used AT CONTROLLER LEVEL, at 51 exception handler. This exception is captured by host and processed by SASResponseHandler;
        /// Send the long poll request to Host;
        /// </summary>
        public void HandpaySendHandpayInformation()
        {
            host.SendHandPayInformation();
        }

        #endregion

         /// <summary>
        /// LPB4;
        /// It is used AT INITIAL ROUTINE LEVEL. For each desired game number;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="gameNumber">Long poll parameter, please refer to protocol documentation</param>
        public void SendWagerCategoryInformation(byte[] gameNumber)
        {
            host.SendWagerCategoryInformation(gameNumber);
        }

        /// <summary>
        /// LPB5;
        /// It is used AT CONTROLLER LEVEL for each game number given by EGM through long poll 51 response arriving to Host;
        /// Send the long poll request to Host;
        /// </summary>
        /// <param name="gameNumber">Long poll parameter, please refer to protocol documentation</param>
        public void SendGameNExtendedInformation(byte[] gameNumber)
        {
            host.SentExtendedGameNInformation(gameNumber);
        }


        /// <summary>
        ///  LP80;
        ///  It is used AT CONTROLLER LEVEL, if the LP80Interfacing flag is enabled when long poll 80 arrives to client; 
        ///  Send the long poll request to Host
        /// </summary>
        /// <param name="broadcast">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="group">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="level">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="amount">Long poll parameter, please refer to protocol documentation</param>
        public void SingleLevelProgressiveBroadcast(bool broadcast, byte group, byte level, byte[] amount)
        {
            host.SendLP80(broadcast, group, level, amount);
        }

        /// <summary>
        /// LP86;
        /// It is used AT CONTROLLER LEVEL, if the LP86Interfacing flag is enabled when long poll 86 arrives to Client;
        /// Send the long poll request to Host
        /// </summary>
        /// <param name="broadcast">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="group">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="amountsandlevels">Long poll parameter, please refer to protocol documentation</param>
        public void MultipleLevelProgressiveBroadcast(bool broadcast, byte group, List<Tuple<byte, byte[]>> amountsandlevels)
        {
            host.SendLP86(broadcast, group, amountsandlevels);
        }

        /// <summary>
        /// LP2E;
        /// It is used AT CONTROLLER LEVEL, if the LP2EInterfacing flag is enabled when long poll 2E arrives to Client;
        /// Send the long poll request to Host
        /// </summary>
        /// <param name="bufferAmount">Long poll parameter, please refer to protocol documentation</param>
        public void SendGameDelayMessage(byte[] bufferAmount)
        {
            host.SendLP2E(bufferAmount);
        }

        /// <summary>
        /// It is not long poll
        ///  Update Asset from Interfacing Settings;
        ///  It is used at several points AT CONTROLLER LEVEL, for example, when a long poll 7B response or long poll 74 response arrives to Host, after a assetnumber update;
        ///  Also it is used in AFTSettings of CONTROLLER LEVEL, when a API request to update the assetnumber from there;
        ///  API Endpoint ->POST  /V0/Settings/AFTSettings -> then WebApiHandler throws AFTSettings event, controller captures this event and then makes a call to this method
        ///  Updates the internal assetnumber attribute of Host;
        /// </summary>
        /// <param name="assetnumber"></param>
        public void UpdateAssetNumberFromInterfacing(int assetnumber)
        {
            host.SetAssetNumber(assetnumber);
        }

        /// <summary>
        ///  LP55;
        ///  It is used AT CONTROLLER LEVEL, at 8C exception handler. This exception is captured by host and processed by SASResponseHandler
        ///  Send the long poll request to Host
        /// </summary>
        public void SendSelectedGameNumber()
        {
            host.SendSelectedGameNumber();
        }

        /// <summary>
        /// Get Address from Host. It is used at several recipes like RecipeInitialRoutine, RecipeRegistration for building long polls
        /// </summary>
        /// <returns>The address which the host build all long polls</returns>
        public byte GetAddress()
        {
            return host.GetAddress();
        }


        /// <summary>
        /// LP8A;
        /// It is used AT CONTROLLER LEVEL, if LP8AInterfacing flag is enabled when long poll 8A arrives to client;
        /// Send the long poll request to Host;
        /// Waits 10 seconds after that to send 9A if bonusing is enabled;
        /// </summary>
        /// <param name="bonusAmount"></param>
        /// <param name="taxStatus"></param>
        public void InitiateLegacyBonus(byte[] bonusAmount, byte taxStatus)
        {
            host.InitiateLegacyBonus(bonusAmount, taxStatus);
            Timer10SecondsAfter8A.Start();
        }


        ///// <summary>
        ///// LP9A;
        ///// 
        ///// </summary>
        ///// <param name="gameNumber"></param>
        //public void SendLegacyBonusMeters(byte[] gameNumber)
        //{
        //    host.SendLP9A(gameNumber);
        //}

        ///// <summary>
        ///// LP48;
        ///// </summary>
        //public void SendLastBillAcceptedInformation()
        //{
        //    host.SendLastBillAcceptedInformation();
        //}

        #endregion



        #endregion



        /******************************TIMERS ACTIONS*********************************/


        #region TIMER ACTIONS

        /// <summary>
        /// Un timer que establece el intervalo de 10 segundos después de mandarse el 8A
        /// Rutina que se ejecuta al cumplirse 10 segundos después de mandarse el 8A
        /// 
        /// A timer that sets the 10-second interval after the 8A is commanded
        /// Routine to be executed 10 seconds after sending 8A
        /// </summary>
        public System.Timers.Timer Timer10SecondsAfter8A;


        /// <summary>
        /// Método que se ejecuta transcurrido el tiempo establecido  y arranca meter 9A
        /// Method that executes after the set time and starts meter 9A
        /// It is the handler of Timer10SecondsAfter8A
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerStart10SecondsAfter8A(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Stop the timer to avoid a second execution
            Timer10SecondsAfter8A.Stop();
            // Send 9A if BonusingEnabled is true
            if (InterfacingSettings.Singleton().BonusingEnabled == true)
                host.SendLP9A(new byte[] { 0x00, 0x00 });

        }


        /// <summary>
        ///  Un timer que establece el intervalo de consulta de meters
        ///  Rutina que se ejecuta al cumplirse 1 segundo, para chequear meters
        /// 
        ///  A timer that sets the meters query interval.
        ///  Routine to be executed after 1 second, to check meters
        /// </summary>
        public System.Timers.Timer TimerCheckMeters;


        /// <summary>
        /// Método que se ejecuta transcurrido el tiempo establecido  y arranca el chequeo de Meters
        /// Method that is executed after the set time has elapsed and starts the Meters check.
        /// It is the handler of TimerCheckMeters
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerStartCheckMeters(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Si ya está iniciada
            // If already started
            if (TimerMetersInitiated)
            {
                TimerMetersRetries++; // espera // Wait 
                if (TimerMetersRetries > 10) // Si ya esperó en 10 intentos // If you have already waited in 10 attempts 
                {
                    TimerMetersInitiated = false; // Resetea la bandera // Reset the flag 
                }
            }
            // Si no está en ninguna redemption, validation o transacción, 
            // If you are not in any redemption, validation or transaction, 
            if (NoCurrentRedemption()
            && NoCurrentValidation()
            && NoCurrentTransaction()
            // Si no se inició ninguna rutina similar
            // If no similar routine was started
            && !TimerMetersInitiated)
            {
                TimerMetersRetries = 0;
                TimerMetersInitiated = true;
                // Chequea que en la cola de long polls a enviar, no exista el 2F
                // Check that in the queue of long polls to be sent, there is no 2F.
                if (!host.LongPollCodeExistsInQueue(0x2F))
                {
                    Recipe getmetersmachine = new Recipe("GetMetersMachine"); // Instancio una nueva recipe // I am launching a new recipe
                    getmetersmachine.Instance.Init(this);
                    // Ejecutar la recipe, esperando el status // Execute the recipe, waiting for the status
                    ActionStatus status = getmetersmachine.Instance.Execute();
                    if (LastEGMResponseReceivedAt != null)
                        physicalEGM.SetLastEGMResponseTS(LastEGMResponseReceivedAt.Value);
                }
            }

        }

        /// <summary>
        ///  A timer every 3 seconds
        /// </summary>
        public System.Timers.Timer Timer3Seconds;


        /// <summary>
        /// Method that is executed after the set time has elapsed (Timer 3 Seconds)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerStart3Seconds(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Si ya está iniciada
            // If already started
            if (Timer3SecondsInitiated)
            {
                Timer3SecondsRetries++; // espera // Wait 
                if (Timer3SecondsRetries > 10) // Si ya esperó en 10 intentos // If you have already waited in 10 attempts 
                {
                    Timer3SecondsInitiated = false; // Resetea la bandera // Reset the flag 
                }
            }
            if (// Si no se inició ninguna rutina similar
            // If no similar routine was started
             !Timer3SecondsInitiated)
            {
                //Timer3SecondsRetries = 0;
                //Timer3SecondsInitiated = true;
                host.SendLP74(0xFF, 0x00, new byte[] { 0x00, 0x00 });  
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xB8, 0xB9, 0xBA, 0xBB });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xBC, 0xBD , 0x17, 0x18, 0x32, 0x2E});
            }
        }

        /// <summary>
        /// Un timer que establece el intervalo de 30 segundos
        /// Rutina que se ejecuta al cumplirse 30 segundos
        ///
        /// A timer that sets the interval to 30 seconds
        /// Routine to be executed at the 30-second mark
        /// </summary>
        public System.Timers.Timer Timer30Seconds;

        /// <summary>
        /// Método que se ejecuta transcurrido el tiempo establecido  y arranca el chequeo de Meters cada 30 segundos
        /// Method that runs after the set time has elapsed and starts the Meters check every 30 seconds.
        /// It is the handler of TimerStart30Seconds
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerStart30Seconds(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Si ya está iniciado
            // If you are already started
            if (Timer30SecondsInitiated)
            {
                Timer30SecondsRetries++; // espera // wait
                if (Timer30SecondsRetries > 10) // Si ya esperó en 10 intentos // If you have already waited in 10 attempts 
                {
                    Timer30SecondsInitiated = false; // Resetea la bandera // Reset the flag 
                }
            }
            // Si no está en ninguna redemption, validation o transacción, 
            // If you are not in any redemption, validation or transaction, 
            if (NoCurrentRedemption()
            && NoCurrentValidation()
            && NoCurrentTransaction()
            // Si no se inició ninguna rutina similar
            // If no similar routine was started
            && !Timer30SecondsInitiated)
            {
                //Timer30SecondsRetries = 0;
                //Timer30SecondsInitiated = true;
                // Chequea que en la cola de long polls a enviar, no exista el 20 
                // Check that in the queue of long polls to be sent, there is no 20 
                if (!host.LongPollCodeExistsInQueue(0x20))
                {
                    // Envía el long poll 20
                    // Send the long poll 20
                    host.SendTotalDollarValueOfBillsMeters();
                }
                // Chequea que en la cola de long polls a enviar, no exista el 55        
                // Check that in the queue of long polls to be sent, there is no 55        
                if (!host.LongPollCodeExistsInQueue(0x55))
                {
                    // Envía el long poll 55
                    // Send the long poll 55
                    host.SendSelectedGameNumber();
                }
                // Chequea que en la cola de long polls a enviar, no exista el 56
                // Check that there is no 56 in the queue of long polls to be sent.
                if (!host.LongPollCodeExistsInQueue(0x56))
                {
                    // Envía el long poll 56
                    // Send the long poll 56
                    host.SendEnabledGameNumbers();
                }
                // Chequea que en la cola de long polls a enviar, no exista el B1
                // Check that there is no B1 in the queue of long polls to be sent.
                if (!host.LongPollCodeExistsInQueue(0xB1))
                {
                    // Envía el long poll B1
                    // Send the long poll B1
                    host.SendCurrentPlayerDenomination();
                }
                // Chequea que en la cola de long polls a enviar, no exista el 7E
                // Check that in the queue of long polls to be sent, there is no 7E.
                if (!host.LongPollCodeExistsInQueue(0x7E))
                {
                    // Envía el long poll 7E
                    // Send the long poll 7E
                    host.SendCurrentDateTimeGamingMachine();
                }
                // Chequea que en la cola de long polls a enviar, no exista el 2A
                // Check that in the queue of long polls to be sent, there is no 2A.
                if (!host.LongPollCodeExistsInQueue(0x2A))
                {
                    // Envía el long poll 2A
                    // Send long poll 2A
                    host.SendTrueCoinIn();
                }
                // Chequea que en la cola de long polls a enviar, no exista el 2B
                // Check that in the queue of long polls to be sent, there is no 2B.
                if (!host.LongPollCodeExistsInQueue(0x2B))
                {
                    // Envía el long poll 2B
                    // Send long poll 2B
                    host.SendTrueCoinOut();
                }
                // Check that in the queue of long poll to be sent, there is no 73
                if (!host.LongPollCodeExistsInQueue(0x73))
                {
                    // Send long poll 73 Interrogate
                    host.AFTRegistration(0xFF, new byte[] {}, -1);
                }
                // Check that in the queue of long poll to be sent, there is not 54
                if (!host.LongPollCodeExistsInQueue(0x54))
                {
                    // Send long poll 54 
                    host.SendSASVersionAndMachineSerialNumber();
                }

                // Request the lp74 info
                host.SendLP74(0xFF, 0x00, new byte[] { 0x00, 0x00 });  

                // Request the meter code 23
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0x23 });

                RecipeGetMetersMultiGame getmetersmachine0 = new RecipeGetMetersMultiGame(); // Instancio una nueva recipe // I am launching a new recipe
                getmetersmachine0.Init(this);
                getmetersmachine0.SetGameForMeters(new byte[] { 0x00, 0x00 });
                getmetersmachine0.Execute();


                RecipeGetMetersMultiGame getmetersmachine = new RecipeGetMetersMultiGame(); // Instancio una nueva recipe // Instancio una nueva recipe 
                getmetersmachine.Init(this);
                getmetersmachine.SetGameForMeters(GetSelectedGameNumber());
                getmetersmachine.Execute();


            }
        }

        /// <summary>
        /// Un timer que establece el intervalo de 300 segundos
        /// Rutina que se ejecuta al cumplirse 300 segundos
        /// 
        /// A timer that sets the interval to 300 seconds
        /// Routine to be executed at the end of 300 seconds
        /// </summary>
        public System.Timers.Timer Timer300Seconds;

        /// <summary>
        ///  Método que se ejecuta transcurrido el tiempo establecido  y arranca el chequeo de Meters cada 300 segundos
        ///  Method that runs after the set time has elapsed and starts the Meters check every 300 seconds.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerStart300Seconds(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Si ya está iniciado
            // If you are already started
            if (Timer300SecondsInitiated)
            {
                Timer300SecondsRetries++; // espera // wait
                if (Timer300SecondsRetries > 10) // Si ya esperó en 10 intentos // If you have already waited in 10 attempts 
                {
                    Timer300SecondsInitiated = false; // Resetea la bandera // Reset the flag
                }
            }
            // Si no está en ninguna redemption, validation o transacción, 
            // If you are not in any redemption, validation or transaction, 
            if (NoCurrentRedemption()
            && NoCurrentValidation()
            && NoCurrentTransaction()
            // Si no se inició ninguna rutina similar
            // If no similar routine was started
            && !Timer300SecondsInitiated)
            {
                //Timer300SecondsRetries = 0;
                //Timer300SecondsInitiated = true;
                // Chequea que en la cola de long polls a enviar, no exista el B2
                // Check that in the queue of long polls to be sent, there is no B2.
                if (!host.LongPollCodeExistsInQueue(0xB2))
                {
                    // Envia el long poll B2 
                    // Send the long poll B2 
                    host.SendEnabledPlayerDenominations();
                }
                // Chequea que en la cola de long polls a enviar, no exista el B3       
                // Check that in the queue of long polls to be sent, there is no B3.       
                if (!host.LongPollCodeExistsInQueue(0xB3))
                {
                    // Envia el long poll B3
                    // Send the long poll B3
                    host.SendTokenDenomination();
                }
                if (InterfacingSettings.Singleton().BonusingEnabled == true)
                    host.SendLP9A(new byte[] { 0x00, 0x00 });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0x03, 0x04, 0x09, 0x0A, 0x0B, 0x0D, 0x0E, 0x11, 0x12, 0x13 });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x2F, 0x30, 0x31, 0xFA });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0x33, 0x34, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0xFB });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0x5F, 0x79, 0x7A, 0x7F, 0x80, 0x81, 0x82, 0x83, 0x84, 0x85 });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0x86, 0x87, 0x88, 0x89, 0x90, 0x91, 0x92, 0x93, 0xA0, 0xA1 });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xAE, 0xAF, 0xB0, 0xB1, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD });
                host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xFC, 0xFD, 0xFE, 0xFF });


            }
        }

        #endregion



        /************************** AUXILIAR FUNCTIONS *******************************/

        #region AUXILIAR FUNCTIONS


        /// <summary>
        /// Función que determina que no hay current redemption en ejecución
        /// Function determining that no current redemption is running
        /// References: 
        /// * TimerStartCheckMeters, checking it with other conditions to send the long poll 2F
        /// * TimerStart30Seconds, checking it with other conditions to send several long polls: 20, 55, 56, B1, 7E, 2A, 2B, 2F
        /// * TimerStart300Seconds, checking it with other conditions to send several long polls: B2, B3, and 9A if bonusing is enabled
        /// * GetHostStatus, stamping the return value in field MainController_InterfacedRedemptionInProcess and send to API
        /// * GetMetersFromPhysicalEGM, checking it with other conditions to stamp the boolean CreditsTransactionInProgress and send it to API
        /// </summary>
        /// <returns></returns>
        public bool NoCurrentRedemption()
        {
            // Check that the status of InterfacedRedemption state machine is in Idle or Completed
            return (InterfacedRedemption.Instance().status == InterfacedRedemptionStatus.Idle
                || InterfacedRedemption.Instance().status == InterfacedRedemptionStatus.Completed);
        }

        /// <summary>
        /// Función que determina que no hay current transaction en ejecución
        /// Function that determines that no current transaction is running
        /// References: 
        /// * TimerStartCheckMeters, checking it with other conditions to send the long poll 2F
        /// * TimerStart30Seconds, checking it with other conditions to send several long polls: 20, 55, 56, B1, 7E, 2A, 2B, 2F
        /// * TimerStart300Seconds, checking it with other conditions to send several long polls: B2, B3, and 9A if bonusing is enabled
        /// * GetHostStatus, stamping the return value in field MainController_CurrentTransactionInProcess and send to API
        /// * GetMetersFromPhysicalEGM, checking it with other conditions to stamp the boolean CreditsTransactionInProgress and send it to API
        /// </summary>
        /// <returns></returns>
        public bool NoCurrentTransaction()
        {
            // Check that the status of AFTCurrentTransaction state machine is in Created or Completed
            return (AFTCurrentTransaction.Instance().status == AFTCurrentTransactionStatus.Created
                || AFTCurrentTransaction.Instance().status == AFTCurrentTransactionStatus.Completed);
        }

        /// <summary>
        /// Función que determina que no hay current transaction en ejecución
        /// Function that determines that the initial routine is running or the registration recipe is running
        /// References: 
        /// * TryStopHost, checking it with other conditions to stop the host
        /// </summary>
        /// <returns></returns>
        public bool InitialRoutineInProgress()
        {
            LaunchLog(new string[] { "SERVICE MANAGEMENT" }, $"initial routine {initialroutine.Instance.InProgress()}", new EventArgs());
            LaunchLog(new string[] { "SERVICE MANAGEMENT" }, $"registration {registrationRecipe.Instance.InProgress()}", new EventArgs());

            return (initialroutine.Instance.InProgress()
                || registrationRecipe.Instance.InProgress());
        }



        /// <summary>
        /// Función que determina que no hay current validation en ejecución
        /// Function determining that there is no current validation running
        /// References: 
        /// * TimerStartCheckMeters, checking it with other conditions to send the long poll 2F
        /// * TimerStart30Seconds, checking it with other conditions to send several long polls: 20, 55, 56, B1, 7E, 2A, 2B, 2F
        /// * TimerStart300Seconds, checking it with other conditions to send several long polls: B2, B3, and 9A if bonusing is enabled
        /// * GetHostStatus, stamping the return value in field MainController_InterfacedValidationInProcess and send to API
        /// * GetMetersFromPhysicalEGM, checking it with other conditions to stamp the boolean CreditsTransactionInProgress and send it to API
        /// </summary>
        /// <returns></returns>
        public bool NoCurrentValidation()
        {
            return (InterfacedValidation.Instance().status == InterfacedValidationStatus.Idle);
        }


        // Truncate string
        /// <summary>
        /// Truncar una string a los primeros N caracteres, con N el parámetro que me viene como `length`
        /// Truncate a string to the first N characters, with N being the parameter that comes as `length`.
        /// Is used by 'IsResponse', truncating long polls up to two characters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string Truncate(string value, int length)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            return value.Length <= length ? value : value.Substring(0, length);
        }

        /// <summary>
        /// Determina si la respuesta `response` pertenece al long poll `cmd`
        /// Determines if the response `response` belongs to the long poll `cmd`.
        /// Is used by 'cmdReceived' checking that response arrived from EGM belongs to the last long poll sent, transitioning to LongPollResponseSuccesfull status
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static bool IsResponse(string response, string cmd)
        {
            // Si la response tiene más de 5 caracteres
            // If the response is longer than 5 characters (AA-BB...)
            if (response.Length >= 5)
            {
                // Si la cmd tiene más de 5 caracteres
                // If cmd is longer than 5 characters (CC-DD....)
                if (cmd.Length >= 5)
                {
                    // Chequea que los primeros 4 caracteres coincidan 
                    // Check the first 4 characters match (AA-BB==CC-DD)
                    return Truncate(cmd, 4) == Truncate(response, 4);
                }
            }
            // Si la cmd tiene exactamente 2 caracteres
            // If the cmd has exactly 2 characters
            else if (cmd.Length == 2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Acceso a índice con handleo de errores: buf[index]
        ///  Index access with error handling: buf[index]
        ///  It is used mainly by 'transactionReceived' to access to specific indexes without any exception.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        static byte? GetByteFromArrayIndex(byte[] buf, int index)
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
        /// Dado una colección de meters, se envía un longpoll a la EGM para consultarlos
        /// Given a collection of meters, a longpoll is sent to the EGM for consultation.
        /// The references for this method are the following:
        /// handpayReset52, RedemptionSendtRedeemTicketCommand and ValidationSendEnhancedValidationInformation
        /// </summary>
        /// <param name="meters">The byte array which each element containing the long poll code</param>
        void GetMeters(byte[] meters)
        {
            // Obtenemos todos los meters 
            // We obtain all the meters 
            int k = 1;
            // Obtengo los primeros 10 meters
            // I obtain the first 10 meters
            byte[] submeters = meters.Take(10).ToArray();
            // Mientras haya al menos un meter en el resultado de obtener los primeros 10 meters
            // As long as there is at least one meter in the result of obtaining the first 10 meters
            while (submeters.Count() > 0)
            {
                // Mando al host el long poll
                // I send to the host the long poll
                host.SendSelectedMeter(submeters);
                // Skipeo los 10 elementos y vuelvo a tomar los 10 elementos
                // I ski the 10 elements and retake the 10 elements.
                submeters = meters.Skip(k * 10).Take(10).ToArray();
                k++;
            }
        }

        /// <summary>
        /// Method Launch Log. It throws the event to main controller for print the service log. It is used at several points
        /// </summary>
        /// <param name="tags">Tags for classify the service log</param>
        /// <param name="message">The proper message to print </param>
        /// <returns></returns>
        public void MLaunchLog(string[] tags, string message)
        {
            LaunchLog(tags, message, new EventArgs());
        }

        /// <summary>
        /// Send Long Poll with state machine synchronization. Return an ActionStatus. It could be Failed if the EGM didn't response or Completed if EGM replied correctly
        /// References: It is used in the Initial Routine Recipe and the Registration Recipe. These two recipes are the only ones which requires a state machine for send long polls
        /// </summary>
        /// <param name="lp">The long poll byte array</param>
        /// <returns></returns>
        public ActionStatus SendLongPollWaitingResponse(byte[] lp)
        {
            SendingLongPollSM.Instance().longpoll = BitConverter.ToString(lp); // At SendingLongPollSMStatus singleton, save the actual lp as longpoll
            SendingLongPollSM.Instance().Transition(SendingLongPollSMStatus.LongPollSentToHost);  // At SendingLongPollSMStatus singleton, transition to LongPollSentToHost
            host.SendLongPoll(lp); // Enqueue the long poll lp
            while (SendingLongPollSM.Instance().status != SendingLongPollSMStatus.LongPollResponseSuccesfull // Wait until status is LongPollResponseSuccesfull or LongPollNoResponse
                && SendingLongPollSM.Instance().status != SendingLongPollSMStatus.LongPollNoResponse) { }

            // If EGM Responded
            if (SendingLongPollSM.Instance().status == SendingLongPollSMStatus.LongPollResponseSuccesfull)
            {
                // Transition to Idle
                SendingLongPollSM.Instance().Transition(SendingLongPollSMStatus.Idle);
                // Return Completed
                return ActionStatus.Completed;
            }
            // If EGM didn't response
            else if (SendingLongPollSM.Instance().status == SendingLongPollSMStatus.LongPollNoResponse)
            {
                // Transition to Idle
                SendingLongPollSM.Instance().Transition(SendingLongPollSMStatus.Idle);
                LaunchLog(new string[] { "SERVICE MANAGEMENT", "INITIAL ROUTINE" }, $"Warning: No response for {BitConverter.ToString(lp)}", new EventArgs());
                if (lp.Count() > 1)
                {
                    if (!InitialRoutineLPsWithNoResponse.Contains(lp[1]))
                        InitialRoutineLPsWithNoResponse.Add(lp[1]);
                }              
                // Return Completed (but failed)
                return ActionStatus.Completed;
            }
            else
            {
                // Return Failed
                return ActionStatus.Failed;
            }
        }

        #endregion


    }
}
