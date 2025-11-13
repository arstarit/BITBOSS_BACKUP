using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Recipes;
using BitbossCardReaderController;
using System.Text;

namespace MainController
{
    // Definición de ValidTransition.   
    // ValidTransition Definition.     

    /// <summary>
    /// Definición de ValidTransition.  
    /// Representa una transición. Dado un estado status de tipo T, se lista todos los estados a los cuales se puede transicionar en next_statusde tipo T
    ///
    /// ValidTransition Definition   
    /// It represents a transition. Given a Type T status, it lists all the states to which transition can be made in next_status type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValidTransition<T>
    {
        public T status;
        public T[] next_status;
    }

    class Program
    {
        /***********************************************************************************************************************/
        /*Main Controller attributes definitions: physicalEGMController, virtualEGMController, cardReaderHandler, webApiHandler. These are the main components
         *Defines also the three timers: To Stop Host, To Stop Client and to Set the EGM Id periodically
         *Defines also the different flags for example the EGM Communication status, Smib communication status, the live trace flag 
         *Defines also the byte array representing the different auto-relayed exceptions
        /***********************************************************************************************************************/
        #region DEFINITIONS
        // The MainController consist in three parts:

        /// <summary>
        /// A handler or controller communicating with the PhysicalEGM
        /// </summary>
        static PhysicalEGMBehaviourController physicalEGMController;
        /// <summary>
        ///  A handler or controller communicating with the VirtualEGM 
        /// </summary>
        static VirtualEGMBehaviourController virtualEGMController;
        /// <summary>
        /// A handler or controller communicating with the cardReaderController 
        /// </summary>
        static CardReaderHandler cardReaderHandler;
        /// <summary>
        /// A handler or controller communicating with the WebApi 
        /// </summary>
        static WebApiHandler webApiHandler;
        /// <summary>
        ///  A timer that sets the query interval of running processes
        ///  Routine that is executed after 1 second, to query the running processes, and based on that, stop the client.
        /// </summary>
        static System.Timers.Timer TimerTryStopClient;
        ///  A timer that sets the query interval of running processes
        ///  Routine that is executed after 1 second, to query for running processes, and based on that, stop the host.
        /// </summary>
        static System.Timers.Timer TimerTryStopHost;
        ///  A timer that sets the query interval of running processes
        ///  Routine that is executed after 1 second, to query for running processes, and based on that, stop the host.
        /// </summary>
        static System.Timers.Timer TimerSetEGMId;
        /// <summary>
        ///  A Boolean that indicates whether or not there is communication with the EGM.
        /// </summary>
        public static bool? EGMcommunication = false;
        /// <summary>
        ///  A boolean that indicates if the link to the SMIB is down.
        /// </summary>
        public static bool? SMIBLinkDown = true;

        static bool livetrace = false;

        /// <summary>
        /// Exceptions of the EGM, which will have to be forwarded, through the virtualEGM, to the client.
        /// </summary>
        static List<byte> AutoRelayedExceptions = (new byte[] {0x00,
                                                               0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E,
                                                               0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E,
                                                               0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C,
                                                               0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                                                               0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50,
                                                               0x53, 0x55,
                                                               0x60, 0x61, 0x6C, 0x6D, 0x6E,
                                                               0x70, 0x71, 0x72, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A ,0x7B, 0x7E, 0x7F,
                                                               0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x88, 0x8C}).ToList();

        #endregion

        /***********************************************************************************************************************/
        /*Main Controller auxiliar functions: Int to byte array converter; A bit extractor from a byte
        /***********************************************************************************************************************/
        #region AUXILIAR

        /// <summary>
        /// Dado un array (o lista) de array de bytes, la idea es "aplanar" la lista en un sólo array que concatene cada elemento
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        static private Byte[] join(params Byte[][] arr)
        {
            Byte[] b = new Byte[] { };
            foreach (Byte[] b_ in arr)
            {
                b = b.ToList().Concat(b_.ToList()).ToArray();
            }
            return b;
        }

        /// <summary>
        /// Function that converts an integer into an array of bytes.
        /// InttoBCD5, If the argument is ABCDEF, the resulting byte array is 0xAB 0xCD and 0XEF.
        /// It is used at PhysicalLP57 Response handler, casting the amount from the long poll to byte.
        /// It is used at PhysicalLP51 Response handler, casting each game number to byte array  to use in other long polls
        /// </summary>
        /// <param name="numericvalue">The unsigned integer value</param>
        /// <param name="bytesize">The byte array size. The exceeded bytes will be 00</param>
        /// <returns>The BCD format value of its integer</returns>
        static private byte[] intToBCD5_v2(uint numericvalue, int bytesize = 5)
        {
            byte[] bcd = new byte[bytesize];
            for (int byteNo = 0; byteNo < bytesize; ++byteNo)
                bcd[byteNo] = 0;
            for (int digit = 0; digit < bytesize * 2; ++digit)
            {
                uint hexpart = numericvalue % 10;
                bcd[digit / 2] |= (byte)(hexpart << ((digit % 2) * 4));
                numericvalue /= 10;
            }

            bcd = bcd.Reverse().ToArray();
            return bcd;
        }

        /// <summary>
        ///  Given a byte and a position, the function returns the bit in the position pos.
        ///  It is used mainly at PhysicalLPA0Response, for access to the different bits of feature bytes.
        /// </summary>
        /// <param name="b">The byte</param>
        /// <param name="pos">The bit position to extract</param>
        /// <returns>The bit in the position as argument</returns>
        private static bool getBit(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        ///// <summary>
        ///// Setting Validation Extensions for the Physical and Virtual EGM
        ///// IT HAS NO REFERENCE TO THAT METHOD
        ///// </summary>
        ///// <param name="b">The boolean or bit enabling the validation extensions</param>
        //static void SetValidationExtensions(bool b)
        //{
        //    virtualEGMController.SetValidationExtensions(b);
        //    physicalEGMController.SetValidationExtensions(b);
        //}



        ///// <summary>
        /////  RAM Clear, for the physicalEGM and virtualEGM
        ///// </summary>
        //public static void RAMClear()
        //{
        //    virtualEGM.RAMClear();
        //    physicalEGM.RAMClear();
        //}

        #endregion

        /***********************************************************************************************************************/
        /*Main Controller initiliazation: This region describes the different initialization of the main components:
         *Remind that the main componets are the PhysicalEGMController, VirtualEGMController, CardReaderHandler and WebApiHandler
        /***********************************************************************************************************************/
        #region MAIN_CONTROLLER_INIT

        /*......................................................................................................................*/
        /*This subregion describes the initialization of WebApiHAndler. There is only one method suscribing all api events
        /*......................................................................................................................*/
        #region InitWebApiController
        /// <summary>
        ///  Initialization of the web api
        ///  It is used in Main method of controller, when the program starts its execution
        /// </summary>
        static void initialize_webApiHandler()
        {
            webApiHandler = new WebApiHandler();
            // A cashout is performed from the API
            webApiHandler.AFTCashoutFunds += new WebApiHandler.AFTCashoutFundsHandler(AFTCashoutFunds);
            // A transfer is made from the API
            webApiHandler.AFTTransferFunds += new WebApiHandler.AFTTransferFundsHandler(AFTTransferFunds);
            // A query request is made from the API
            webApiHandler.GetMetersFromPhysicalEGM += new WebApiHandler.GetMetersFromPhysicalEGMHandler(GetMetersFromPhysicalEGM);
            // A request for transaction history queries is made from the API.
            webApiHandler.GetPhysicalEGMAFTTransactionHistory += new WebApiHandler.GetPhysicalEGMAFTTransactionHistoryHandler(GetPhysicalEGMAFTTransactionHistory);
            // A request is made to consult the Physical EGM information.
            webApiHandler.GetPhysicalEGMInfo += new WebApiHandler.GetPhysicalEGMInfoHandler(GetPhysicalEGMInfo);
            // A Live Trace Host order is placed
            webApiHandler.GetHostLiveTrace += new WebApiHandler.GetHostLiveTraceHandler(GetHostLiveTrace);
            // An order is placed for the Live Trace Client
            webApiHandler.GetClientLiveTrace += new WebApiHandler.GetClientLiveTraceHandler(GetClientLiveTrace);
            // A request is made to consult the current transfer.
            webApiHandler.GetPhysicalEGMAFTCurrentTransfer += new WebApiHandler.GetPhysicalEGMAFTCurrentTransferHandler(GetPhysicalEGMAFTCurrentTransfer);
            // An order is placed for inferfacing settings.
            webApiHandler.GetInterfacingSettings += new WebApiHandler.GetInterfacingSettingsHandler(GetInterfacingSettings);
            // A request is made to query the host status
            webApiHandler.GetHostStatus += new WebApiHandler.GetHostStatusHandler(GetHostStatus);
            // A controller restart request is made
            webApiHandler.Restart += new WebApiHandler.RestartHandler(Restart);
            // A request is made for interface setting adjustments.
            webApiHandler.AFTSettings += new WebApiHandler.AFTSettingsHandler(AFTSettings);
            // A request is made to consult the EGM and SMIB link healths
            webApiHandler.LinksHealth += new WebApiHandler.LinksHealthHandler(LinksHealth);
            // A request is made to query the physical EGM Settings
            webApiHandler.GetPhysicalEGMSettings+= new WebApiHandler.GetPhysicalEGMSettingsHandler(GetPhysicalEGMSettings);
            // API starts
            webApiHandler.Start();

        }

        #endregion


        /*......................................................................................................................*/
        /*This subregion describes the initialization of VirtualEGMController.
         * Contains different methods about the initialization, like instantiation of VirtualEGM object, the event suscriber and the client stopper
        /*......................................................................................................................*/
        #region InitVEGMController

        /// <summary>
        /// Method that stops client after a several validations: No redemption, no validation, no transaction and no handpay in process.
        /// It is executed when TimerTryStopClient is elapsed, after a physical egm link is down. If some validation is false, start the timer again until all validations meet the conditions
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void TryStopClient(Object source, System.Timers.ElapsedEventArgs e)
        {

            TimerTryStopClient.Stop();
            // Si no está en ninguna redemption, validation o transacción, 
            // If you are not in any redemption, validation or transaction, 
            if (physicalEGMController.NoCurrentRedemption()
            && physicalEGMController.NoCurrentValidation()
            && physicalEGMController.NoCurrentTransaction()
            && !virtualEGMController.AHandpayInProcess())
            {
                virtualEGMController.DisableVirtualEGM();
            }
            else
            {
                TimerTryStopClient.Start();
            }
        }

        /// <summary>
        /// Registration of events from virtualEGMController: Exceptions, long poll responses and communication link down and up
        /// It is used at initialize_virtualEGM method 
        /// </summary>
        static void VirtualEGMRegisterMethods()
        {
            // Subscription to Smib long poll 01 event
            virtualEGMController.VirtualLP01 += new VirtualEGMBehaviourController.VirtualEGMLP01Handler(VirtualLP01Req);
            // Subscription to Smib long poll 02 event
            virtualEGMController.VirtualLP02 += new VirtualEGMBehaviourController.VirtualEGMLP02Handler(VirtualLP02Req);
            // Subscription to Smib long poll 03 event
            virtualEGMController.VirtualLP03 += new VirtualEGMBehaviourController.VirtualEGMLP03Handler(VirtualLP03Req);
            // Subscription to Smib long poll 04 event
            virtualEGMController.VirtualLP04 += new VirtualEGMBehaviourController.VirtualEGMLP04Handler(VirtualLP04Req);
            // Subscription to Smib long poll 06 event
            virtualEGMController.VirtualLP06 += new VirtualEGMBehaviourController.VirtualEGMLP06Handler(VirtualLP06Req);
            // Subscription to Smib long poll 07 event
            virtualEGMController.VirtualLP07 += new VirtualEGMBehaviourController.VirtualEGMLP07Handler(VirtualLP07Req);
            // Subscription to Smib long poll 08 event
            virtualEGMController.VirtualLP08 += new VirtualEGMBehaviourController.VirtualEGMLP08Handler(VirtualLP08Req);
            // Subscription to Smib long poll 0E event
            virtualEGMController.VirtualLP0E += new VirtualEGMBehaviourController.VirtualEGMLP0EHandler(VirtualLP0EReq);
            // Subscription to Smib long poll 2E event
            virtualEGMController.VirtualLP2E += new VirtualEGMBehaviourController.VirtualEGMLP2EHandler(VirtualLP2EReq);
            // Subscription to Smib long poll 4C event
            virtualEGMController.VirtualLP4C += new VirtualEGMBehaviourController.VirtualEGMLP4CHandler(VirtualLP4CReq);
            // Subscription to Smib long poll 21 event
            virtualEGMController.VirtualLP21 +=  new VirtualEGMBehaviourController.VirtualEGMLP21Handler(VirtualLP21Req);
            // Subscription to Smib long poll 4D event
            virtualEGMController.VirtualLP4DEndValidation += new VirtualEGMBehaviourController.VirtualEGMLP4DEndValidationHandler(VirtualLP4DEndValidationReq);
            // Subscription to Smib long poll 58 event
            virtualEGMController.VirtualLP58 += new VirtualEGMBehaviourController.VirtualEGMLP58Handler(VirtualLP58Req);
            // Subscription to Smib long poll 71 event
            virtualEGMController.VirtualLP71 += new VirtualEGMBehaviourController.VirtualEGMLP71Handler(VirtualLP71ReqAccepting);
            // Subscription to Smib long poll 72 event
            virtualEGMController.VirtualLP72 += new VirtualEGMBehaviourController.VirtualEGMLP72Handler(VirtualLP72Req);
            // Subscription to Smib long poll 72 interrogation event
            virtualEGMController.VirtualLP72Interrogation += new VirtualEGMBehaviourController.VirtualEGMLP72InterrogationHandler(VirtualLP72InterrogationReq);
            // Subscription to Smib long poll 73 event
            virtualEGMController.VirtualLP73 += new VirtualEGMBehaviourController.VirtualEGMLP73Handler(VirtualLP73Req);
            // Subscription to Smib long poll 74 event
            virtualEGMController.VirtualLP74 += new VirtualEGMBehaviourController.VirtualEGMLP74Handler(VirtualLP74Req);
            // Subscription to Smib long poll 7C event
            virtualEGMController.VirtualLP7C += new VirtualEGMBehaviourController.VirtualEGMLP7CHandler(VirtualLP7CReq);
            // Subscription to Smib long poll 7D event
            virtualEGMController.VirtualLP7D += new VirtualEGMBehaviourController.VirtualEGMLP7DHandler(VirtualLP7DReq);
            // Subscription to Smib long poll 7F event
            virtualEGMController.VirtualLP7F += new VirtualEGMBehaviourController.VirtualEGMLP7FHandler(VirtualLP7FReq);
            // Subscription to Smib long poll 80 event
            virtualEGMController.VirtualLP80 += new VirtualEGMBehaviourController.VirtualEGMLP80Handler(VirtualLP80Req);
            // Subscription to Smib long poll 86 event
            virtualEGMController.VirtualLP86 += new VirtualEGMBehaviourController.VirtualEGMLP86Handler(VirtualLP86Req);
            // Subscription to Smib long poll 8A event
            virtualEGMController.VirtualLP8A += new VirtualEGMBehaviourController.VirtualEGMLP8AHandler(VirtualLP8AReq);
            // Subscription to Smib long poll 94 event
            virtualEGMController.VirtualLP94 += new VirtualEGMBehaviourController.VirtualEGMLP94Handler(VirtualLP94Req);
            // Subscription to Smib long poll A8 event
            virtualEGMController.VirtualLPA8 += new VirtualEGMBehaviourController.VirtualEGMLPA8Handler(VirtualLPA8Req);
            // Virtual EGM Communication event
            virtualEGMController.VirtualEGMCommunicationDownEvent += new VirtualEGMBehaviourController.VirtualEGMCommunicationDownEventHandler(VirtualEGMSmibLinkDown);
            // Launch log to service
            virtualEGMController.LaunchLog += new VirtualEGMBehaviourController.LaunchLogHandler(LaunchLog);
        }


        /// <summary>
        /// Initialization of the Live Trace of the virtualEGM. Checks if the flag is enabled to initialize the live trace controller
        /// It is used at initiailze_virtualEGM method (when we initialize the virtualEGMController) and the Restart method
        /// </summary>
        static void VirtualEGMCreateLiveTraceController()
        {
            if (livetrace)
            {
                virtualEGMController.CreateLiveTraceController();
            }
        }

        /// <summary>
        /// Initialization of the VirtualEGM BehaviourController
        ///  It is used in Main method of controller, when the program starts its execution
        /// </summary>
        /// <param name="args">The Main arguments</param>
        static void initialize_virtualEGM(string[] args)
        {
            // Port setting
            if (args.Length >= 2)
                virtualEGMController = new VirtualEGMBehaviourController(args[1]);
            else
                virtualEGMController = new VirtualEGMBehaviourController("COM2");
            // Registration of methods
            VirtualEGMRegisterMethods();
            // Creation of the vegm live trace controller
            VirtualEGMCreateLiveTraceController();
            // VirtualEGM starter
            virtualEGMController.StartVirtualEGM();
        }

        #endregion

        /*......................................................................................................................*/
        /*This subregion describes the initialization of Card Reader Controller. Initializes and starts sending a Set EGM Id request
        /*......................................................................................................................*/
        #region InitCardReaderController
        /// <summary>
        /// Initialization of the Card Reader Controller
        /// It is used in Main method of controller, when the program starts its execution
        /// </summary>
        /// <param name="args">The Main arguments</param>
        static void initialize_CardReaderController(string[] args)
        {
            // Port setting
            if (args.Length >= 3)
                cardReaderHandler = new CardReaderHandler(args[2]);
            else
                cardReaderHandler = new CardReaderHandler("COM2");
            // CardReader starter
            cardReaderHandler.Start();
            Thread.Sleep(3000);
            // Build a EGMId Request
            SetEGMIdRequest request = BuildSetEGMId(null);
            // Send the request through card reader controller to card reader device
            cardReaderHandler.SetEGMIdRequest(request);
        }

        #endregion

        /*......................................................................................................................*/
        /* This subregion describes the initialization of PhysicalEGMController.
         * Contains different methods about the initialization, like instantiation of PhysicalEGM object, the event suscriber and the host stopper
        /*......................................................................................................................*/
        #region InitPhysicalEGMController

        /// <summary>
        /// Method that stops host after a several validations: No redemption, no validation, no transaction, no initial routine in progress and no handpay in process.
        /// It is executed when TimerTryStopHost is elapsed, after a virtual egm link is down. If some validation is false, start the timer again until all validations meet the conditions
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void TryStopHost(Object source, System.Timers.ElapsedEventArgs e)
        {
            TimerTryStopHost.Stop();
            // Si no está en ninguna redemption, validation o transacción, 
            // If you are not in any redemption, validation or transaction, 
            if (physicalEGMController.NoCurrentRedemption()
            && physicalEGMController.NoCurrentValidation()
            && physicalEGMController.NoCurrentTransaction()
            && !physicalEGMController.InitialRoutineInProgress()
            && !virtualEGMController.AHandpayInProcess())
            {
                LaunchLog(new string[] {}, "Host Stopped", new EventArgs());
                physicalEGMController.StopPhysicalEGM();
            }
            else
            {
                TimerTryStopHost.Start();
            }
        }

        /// <summary>
        /// Registration of events from physicalEGMController: Exceptions, long poll responses and communication link down and up
        /// It is used at initialize_physicalEGM method 
        /// </summary>
        static void PhysicalEGMRegisterMethods()
        {
            // Meter Updated, to update into virtualEGM
            physicalEGMController.MeterUpdated += new PhysicalEGMBehaviourController.PhysicalMeterUpdatedHandler(MeterUpdated);
            // Custom Meter (string code) Updated, to update into virtualEGM
            physicalEGMController.StrMeterUpdated += new PhysicalEGMBehaviourController.PhysicalStrMeterUpdatedHandler(MeterUpdated);
            // Exception by default, to send to the client
            physicalEGMController.PhysicalExpDefault += new PhysicalEGMBehaviourController.PhysicalExpDefaultHandler(PhysicalExDefault);
            // Subscription to events: Exception 67,
            physicalEGMController.PhysicalExp67 += new PhysicalEGMBehaviourController.PhysicalExp67Handler(PhysicalEx67);
            // Subscription to events: Exception 68,
            physicalEGMController.PhysicalExp68 += new PhysicalEGMBehaviourController.PhysicalExp68Handler(PhysicalEx68);
            // Subscription to events: Exception 69,
            physicalEGMController.PhysicalExp69 += new PhysicalEGMBehaviourController.PhysicalExp69Handler(PhysicalEx69);
            // Subscription to events: Exception 57,
            physicalEGMController.PhysicalExp57 += new PhysicalEGMBehaviourController.PhysicalExp57Handler(PhysicalEx57);
            // Subscription to events: Exception 3D,
            physicalEGMController.PhysicalExp3D += new PhysicalEGMBehaviourController.PhysicalExp3DHandler(PhysicalEx3D);
            // Subscription to events: Exception 3E,
            physicalEGMController.PhysicalExp3E += new PhysicalEGMBehaviourController.PhysicalExp3EHandler(PhysicalEx3E);
            // Subscription to events: Exception 51,
            physicalEGMController.PhysicalExp51 += new PhysicalEGMBehaviourController.PhysicalExp51Handler(PhysicalEx51);
            // Subscription to events: Exception 52,
            physicalEGMController.PhysicalExp52 += new PhysicalEGMBehaviourController.PhysicalExp52Handler(PhysicalEx52);
            // Subscription to events: Exception 3F,
            physicalEGMController.PhysicalExp3F += new PhysicalEGMBehaviourController.PhysicalExp3FHandler(PhysicalEx3F);
            // Subscription to events: Exception 8C,
            physicalEGMController.PhysicalExp8C += new PhysicalEGMBehaviourController.PhysicalExp8CHandler(PhysicalEx8C);
            //  Reply to lp70,
            physicalEGMController.PhysicalLP70Response += new PhysicalEGMBehaviourController.PhysicalLP70ResponseHandler(PhysicalLP70Resp);
            //  Reply to lp71 with machine Status = 00
            physicalEGMController.PhysicalLP71_SuccesfulResponse += new PhysicalEGMBehaviourController.PhysicalLP71_SuccesfulResponseHandler(PhysicalLP71_SuccesfulResp);
            //  Reply to lp71 with machine Status = 0x40
            physicalEGMController.PhysicalLP71_40Response += new PhysicalEGMBehaviourController.PhysicalLP71_40ResponseHandler(PhysicalLP71_40Resp);
            //  Reply to lp71 with machine Status = Error
            physicalEGMController.PhysicalLP71_FailedResponse += new PhysicalEGMBehaviourController.PhysicalLP71_FailedResponseHandler(PhysicalLP71_FailedResp);
            // Reply to lp72
            physicalEGMController.PhysicalLP72Response += new PhysicalEGMBehaviourController.PhysicalLP72_Handler(PhysicalLP72);
            // Reply to lp57
            physicalEGMController.PhysicalLP57Response += new PhysicalEGMBehaviourController.PhysicalLP57ResponseHandler(PhysicalLP57Resp);
            // Reply to lp58
            physicalEGMController.PhysicalLP58Response += new PhysicalEGMBehaviourController.PhysicalLP58ResponseHandler(PhysicalLP58Resp);
            // Reply to lp4D
            physicalEGMController.PhysicalLP4DResponse += new PhysicalEGMBehaviourController.PhysicalLP4DResponseHandler(PhysicalLP4DResp);
            // Reply to lp1B
            physicalEGMController.PhysicalLP1BResponse += new PhysicalEGMBehaviourController.PhysicalLP1BResponseHandler(PhysicalLP1BResp);
            // Reply to lpA0
            physicalEGMController.PhysicalLPA0Response += new PhysicalEGMBehaviourController.PhysicalLPA0ResponseHandler(PhysicalLPA0Response);
            // Reply to lp1F
            physicalEGMController.PhysicalLP1FResponse += new PhysicalEGMBehaviourController.PhysicalLP1FResponseHandler(PhysicalLP1FResp);
            // Reply to lp51
            physicalEGMController.PhysicalLP51Response += new PhysicalEGMBehaviourController.PhysicalLP51ResponseHandler(PhysicalLP51Resp);
            // Reply to lpa4
            physicalEGMController.PhysicalLPA4Response += new PhysicalEGMBehaviourController.PhysicalLPA4ResponseHandler(PhysicalLPA4Resp);
            // Reply to lp7e
            physicalEGMController.PhysicalLP7EResponse += new PhysicalEGMBehaviourController.PhysicalLP7EResponseHandler(PhysicalLP7EResp);
            // Reply to lp55
            physicalEGMController.PhysicalLP55Response += new PhysicalEGMBehaviourController.PhysicalLP55ResponseHandler(PhysicalLP55Resp);
            // Reply to lp56
            physicalEGMController.PhysicalLP56Response += new PhysicalEGMBehaviourController.PhysicalLP56ResponseHandler(PhysicalLP56Resp);
            // Reply to lpB1
            physicalEGMController.PhysicalLPB1Response += new PhysicalEGMBehaviourController.PhysicalLPB1ResponseHandler(PhysicalLPB1Resp);
            // Reply to lpB2
            physicalEGMController.PhysicalLPB2Response += new PhysicalEGMBehaviourController.PhysicalLPB2ResponseHandler(PhysicalLPB2Resp);
            // Reply to lpB3
            physicalEGMController.PhysicalLPB3Response += new PhysicalEGMBehaviourController.PhysicalLPB3ResponseHandler(PhysicalLPB3Resp);
            // Reply to lp54
            physicalEGMController.PhysicalLP54Response += new PhysicalEGMBehaviourController.PhysicalLP54ResponseHandler(PhysicalLP54Resp);
            // Reply to lpB5
            physicalEGMController.PhysicalLPB5Response += new PhysicalEGMBehaviourController.PhysicalLPB5ResponseHandler(PhysicalLPB5Resp);
            // Reply to lp48
            physicalEGMController.PhysicalLP48Response += new PhysicalEGMBehaviourController.PhysicalLP48ResponseHandler(PhysicalLP48Resp);
            // Reply to lp21
            physicalEGMController.PhysicalLP21Response += new PhysicalEGMBehaviourController.PhysicalLP21ResponseHandler(PhysicalLP21Resp);
            // Reply to lp7B
            physicalEGMController.PhysicalLP7BResponse += new PhysicalEGMBehaviourController.PhysicalLP7BResponseHandler(PhysicalLP7BResp);
            // Reply to lp74
            physicalEGMController.PhysicalLP74Response += new PhysicalEGMBehaviourController.PhysicalLP74ResponseHandler(PhysicalLP74Resp);
            // Reply to lp73
            physicalEGMController.PhysicalLP73Response += new PhysicalEGMBehaviourController.PhysicalLP73ResponseHandler(PhysicalLP73Resp);
            // Response to realtime
            physicalEGMController.PhysicalEGMRealTimeEvent += new PhysicalEGMBehaviourController.PhysicalEGMRealTimeEventHandler(PhysicalEGMRealTimeEv);
            // Reply to AFTTransactionCompleted
            physicalEGMController.PhysicalAFTTransactionCompleted += new PhysicalEGMBehaviourController.PhysicalAFTTransactionCompletedHandler(PhysicalAFTTransactionComp);
            // Physical EGM Communication event
            physicalEGMController.PhysicalEGMCommunicationEvent += new PhysicalEGMBehaviourController.PhysicalEGMCommunicationEventHandler(PhysicalEGMCommunicationEv);
            // Physical EGM Initial Routine Finished
            physicalEGMController.InitialRoutineFinished += new PhysicalEGMBehaviourController.InitialRoutineFinishedHandler(InitialRoutineFinish);
            // Launch log to service
            physicalEGMController.LaunchLog += new PhysicalEGMBehaviourController.LaunchLogHandler(LaunchLog);
        }



        /// <summary>
        /// Initialization of the Live Trace of the physicalEGM. Checks if the flag is enabled to initialize the live trace controller
        /// It is used at initiailze_physicalEGM method (when we initialize the PhysicalEGMController) and the Restart method
        /// </summary>
        static void PhysicalEGMCreateLiveTraceController()
        {
            if (livetrace)
            {
                physicalEGMController.CreateLiveTraceController();
            }
        }


        /// <summary>
        /// Initialization of the PhysicalEGM Behaviour Controller
        ///  It is used in Main method of controller, when the program starts its execution
        /// </summary>
        /// <param name="args">The Main arguments</param>
        static void initialize_physicalEGM(string[] args)
        {
            // Port setting
            if (args.Length >= 1)
                physicalEGMController = new PhysicalEGMBehaviourController(args[0]);
            else
                physicalEGMController = new PhysicalEGMBehaviourController("COM1");

            physicalEGMController.ForceDummyRegistrationOnStartup = InterfacingSettings.Singleton().ForceDummyRegistrationOnStartup;

            // Registration of methods
            PhysicalEGMRegisterMethods();

            // Trace initialization 
            PhysicalEGMCreateLiveTraceController();

            // Note that I do not initialize the PhysicalEGM if the VirtualEGM does not warn me.
        }

        #endregion


        /// <summary>
        /// Initialization of the 3 components. MainController starts its execution here.
        /// </summary>
        /// <param name="args">The Main arguments</param>
        static void Main(string[] args)
        {
            try
            {
                // EnableSASTrace option
                if (args.Length >= 4)
                    if (args[3] == "-EnableSASTrace")
                    {
                        livetrace = true;
                    }
                string portHost = args[0];
                
                LaunchLog(new string[] { }, $"Using Host port {portHost}", new EventArgs() );
                string portClient = args[1];
                LaunchLog(new string[] { }, $"Using Client port {portClient}", new EventArgs() );

                TimerTryStopClient = new System.Timers.Timer(1000);
                TimerTryStopClient.Elapsed += TryStopClient;
                TimerTryStopHost = new System.Timers.Timer(1000);
                TimerTryStopHost.Elapsed += TryStopHost;
                SMWATCHDOG.ResetRedemption += new ResetRedemptionEvent(ResetRedemption);
                SMWATCHDOG.ResetValidation += new ResetValidationEvent(ResetValidation);
                SMWATCHDOG.ResetAFT += new ResetAFTEvent(ResetAFT);
                SMWATCHDOG.Instance().StartWatchDog();
                LaunchLog(new string[] { }, $"Initializing PhysicalEGM and Host", new EventArgs() );
                initialize_physicalEGM(args);
                LaunchLog(new string[] { }, $"Initializing VirtualEGM and Client", new EventArgs() );
                initialize_virtualEGM(args);

                if (args.Length >= 3)
                {
                    string portCardReader = args[2];
                    LaunchLog(new string[] { }, $"Using Card Reader port {portCardReader}", new EventArgs() );
                    LaunchLog(new string[] { }, $"Initializing Card Reader Controller", new EventArgs() );
                    initialize_CardReaderController(args); // portCardReader
                    LaunchLog(new string[] { }, $"Initializing Timer", new EventArgs() );
                    TimerSetEGMId = new System.Timers.Timer(30000);
                    TimerSetEGMId.Elapsed += SetEGMId;
                    TimerSetEGMId.Start();
                }
                LaunchLog(new string[] { }, $"Initializing Web API", new EventArgs() );
                initialize_webApiHandler();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Usage:   ");
                Console.WriteLine("     ./MainController portHost portClient portCardReader [-EnableSASTrace]");
            }

        }


        #endregion

        /***********************************************************************************************************************/
        /*This region describes the card reader controller methods and handlers. Right now there is a method that build the SetEGMId request and a handler of a timer requesting that command continuosly
        /***********************************************************************************************************************/
        #region CARD_READER_CONTROLLER

        /// <summary>
        /// It builds the SetEGMId command with physicalEGMController data.
        /// It is used at initialize_CardReaderController for make a first request to card reader and SetEGMId event executed with timer
        /// </summary>
        /// <param name="e"></param>
        /// <returns>An object representing the command request </returns>
        static SetEGMIdRequest BuildSetEGMId(EventArgs e)
        {
            // Declares a serial number as the serial number value persisted on EGMInfo xml or if it doesn't exist, as default 01 02 03 04
            byte[] sn = physicalEGMController.physicalEGM.GetEGMInfo()?.GMSerialNumber == null ? new byte[] { 0x01, 0x02, 0x03, 0x04 } : physicalEGMController.physicalEGM.GetEGMInfo().GMSerialNumber;

            // Instantiates a Set EGMId request
            SetEGMIdRequest request = new SetEGMIdRequest();
            request.AssetNumber = BitConverter.GetBytes(physicalEGMController.host._assetNumber); // With Asset number of host 
            request.Denom = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // Denom set as 00 
            request.SerialNumber = sn; // The serial number 
            // The Location Name, defined with the EGMSettings location value, or '-' if it is empty
            request.Location = physicalEGMController.physicalEGM.GetEGMSettings()?.LocationName == null ? (System.Text.Encoding.ASCII.GetBytes("-")) : System.Text.Encoding.ASCII.GetBytes(physicalEGMController.physicalEGM.GetEGMSettings().LocationName);

            return request;
        }


        /// <summary>
        /// Método que se ejecuta transcurrido el tiempo establecido  y arranca el chequeo de procesos corriendo para poder detener el host
        /// Method that executes after the set time has elapsed and starts the check of running processes in order to stop the host.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void SetEGMId(Object source, System.Timers.ElapsedEventArgs e)
        {
            SetEGMIdRequest request = BuildSetEGMId(null);
            cardReaderHandler.SetEGMIdRequest(request);
        }





        #endregion

        /***********************************************************************************************************************/
        /*This region describes the web api controller methods and handlers. Most of the are handlers of API user request. Each handler has the respective API endpoint
        /***********************************************************************************************************************/
        #region WEB API

        private static byte[] APIregKey = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        ///  Generación de ID de transacciones. Genera aleatoriamente ids de longitud 11
        /// </summary>
        /// <returns></returns>
        private static byte[] generateTransactionID()
        {
            Encoding encoding = Encoding.Default;
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[11];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var finalString = new String(stringChars);
            return encoding.GetBytes(finalString);
        }

        /// <summary>
        /// Cashout
        /// API Endpoint ->POST V0/Transactions/AFT/AFTTransfer with code = 0x80 in the json body -> then WebApiHandler throws AFTCashoutFunds event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <param name="cashableAmount"></param>
        /// <param name="restrictedAmount"></param>
        /// <param name="nonrestrictedAmount"></param>
        /// <returns>An object of type TransactionAPIResponse</returns>
        static TransactionAPIResponse AFTCashoutFunds(int cashableAmount, int restrictedAmount, int nonrestrictedAmount, EventArgs e)
        {
            if (Program.EGMcommunication != true || Program.SMIBLinkDown != false)
            {
                TransactionAPIResponse resp = new TransactionAPIResponse { status = "Offline" };
                return resp;
            }
            if (InterfacedAFT.Instance().WorkInProgress())
            {
                // Creamos la response correspondiente en busy
                // We create the corresponding response in busy
                TransactionAPIResponse resp = new TransactionAPIResponse { status = "Busy" };
                return resp;
            }
            // Transición de Created -> Sent
            if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Sent))
            {
                byte[] transactionId = generateTransactionID();
                virtualEGMController.DisableTransfers();
                // Creamos la response correspondiente en pending
                // We create the corresponding response in pending
                TransactionAPIResponse resp = new TransactionAPIResponse { status = "Pending", transactionId = System.Text.Encoding.Default.GetString(transactionId) };

                AFTCurrentTransaction.Instance().Amount = cashableAmount;
                AFTCurrentTransaction.Instance().TransferType = 0x80;
                DateTime Expiration = DateTime.Now.AddDays(10);
                byte[] ArrExpiration = join(intToBCD5_v2((uint)Expiration.Month, 1),  intToBCD5_v2((uint)Expiration.Day, 1), intToBCD5_v2((uint)Expiration.Year, 2));

                Task t = new Task(() => physicalEGMController.transfer(0x80, cashableAmount, restrictedAmount, nonrestrictedAmount, transactionId, APIregKey, ArrExpiration)); // Transferimos
                t.Start();

                return resp;
            }
            else
            {
                if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Created))
                {
                    // Transición de Created -> Sent
                    if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Sent))
                    {
                        byte[] transactionId = generateTransactionID();
                        virtualEGMController.DisableTransfers();
                        // Creamos la response correspondiente en pending
                        // We create the corresponding response in pending
                        TransactionAPIResponse resp = new TransactionAPIResponse { status = "Pending", transactionId = System.Text.Encoding.Default.GetString(transactionId) };
                        AFTCurrentTransaction.Instance().Amount = cashableAmount;
                        AFTCurrentTransaction.Instance().TransferType = 0x80;
                        DateTime Expiration = DateTime.Now.AddDays(10);
                        byte[] ArrExpiration =  join(intToBCD5_v2((uint)Expiration.Month, 1),  intToBCD5_v2((uint)Expiration.Day, 1), intToBCD5_v2((uint)Expiration.Year, 2));

                        Task t = new Task(() => physicalEGMController.transfer(0x80, cashableAmount, restrictedAmount, nonrestrictedAmount, transactionId, APIregKey, ArrExpiration)); // Transferimos
                        t.Start();
                        return resp;

                    }
                    else
                    {
                        // Creamos la response correspondiente en busy
                        // We create the corresponding response in busy
                        TransactionAPIResponse resp = new TransactionAPIResponse { status = "Busy" };
                        return resp;
                    }
                }
                else
                {
                    // Creamos la response correspondiente en busy
                    // We create the corresponding response in busy
                    TransactionAPIResponse resp = new TransactionAPIResponse { status = "Busy" };
                    return resp;
                }


            }
        }


        /// <summary>
        /// Transfer
        /// API Endpoint ->POST V0/Transactions/AFT/AFTTransfer with code = 0x00 in the json body -> then WebApiHandler throws AFTCashoutFunds event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <param name="cashableAmount"></param>
        /// <param name="restrictedAmount"></param>
        /// <param name="nonrestrictedAmount"></param>
        /// <returns>An object of type TransactionAPIResponse</returns>
        static TransactionAPIResponse AFTTransferFunds(int cashableAmount, int restrictedAmount, int nonrestrictedAmount, EventArgs e)
        {
            if (Program.EGMcommunication != true || Program.SMIBLinkDown != false)
            {
                TransactionAPIResponse resp = new TransactionAPIResponse { status = "Offline" };
                return resp;

            }
            // Transición de Created -> Sent
            else if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Sent))
            {
                byte[] transactionId = generateTransactionID();
                virtualEGMController.DisableTransfers();
                // Creamos la response correspondiente en pending
                // We create the corresponding response in pending
                TransactionAPIResponse resp = new TransactionAPIResponse { status = "Pending", transactionId = System.Text.Encoding.Default.GetString(transactionId) };
                AFTCurrentTransaction.Instance().Amount = cashableAmount;
                AFTCurrentTransaction.Instance().TransferType = 0x00;
                DateTime Expiration = DateTime.Now.AddDays(10);
                byte[] ArrExpiration = join(intToBCD5_v2((uint)Expiration.Month, 1),  intToBCD5_v2((uint)Expiration.Day, 1), intToBCD5_v2((uint)Expiration.Year, 2));

                Task t = new Task(() => physicalEGMController.transfer(0x00, cashableAmount, restrictedAmount, nonrestrictedAmount, transactionId, APIregKey, ArrExpiration)); // Transferimos
                t.Start();

                return resp;
            }
            else
            {
                if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Created))
                {
                    // Transición de Created -> Sent
                    if (AFTCurrentTransaction.Instance().Transition(AFTCurrentTransactionStatus.Sent))
                    {
                        byte[] transactionId = generateTransactionID();
                        virtualEGMController.DisableTransfers();
                        // Creamos la response correspondiente en pending
                        // We create the corresponding response in pending
                        TransactionAPIResponse resp = new TransactionAPIResponse { status = "Pending", transactionId = System.Text.Encoding.Default.GetString(transactionId) };
                        AFTCurrentTransaction.Instance().Amount = cashableAmount;
                        AFTCurrentTransaction.Instance().TransferType = 0x00;
                        DateTime Expiration = DateTime.Now.AddDays(10);
                        byte[] ArrExpiration = join(intToBCD5_v2((uint)Expiration.Month, 1),  intToBCD5_v2((uint)Expiration.Day, 1), intToBCD5_v2((uint)Expiration.Year, 2));

                        Task t = new Task(() => physicalEGMController.transfer(0x00, cashableAmount, restrictedAmount, nonrestrictedAmount, transactionId, APIregKey, ArrExpiration)); // Transferimos
                        t.Start();
                        return resp;
                    }
                    else
                    {
                        // Creamos la response correspondiente en busy
                        // We create the corresponding response in busy
                        TransactionAPIResponse resp = new TransactionAPIResponse { status = "Busy" };
                        return resp;
                    }
                }
                else
                {
                    // Creamos la response correspondiente en busy
                    // We create the corresponding response in busy
                    TransactionAPIResponse resp = new TransactionAPIResponse { status = "Busy" };
                    return resp;
                }


            }
        }


        /// <summary>
        /// Consultamos los Meters a la physicalEGM
        /// We consult the Meters to the physicalEGM
        /// API Endpoint ->GET V0/Stats/PhysicalEGM/0/Meters -> then WebApiHandler throws GetMetersFromPhysicalEGM event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <returns>An object of type MetersAPIResponse</returns>
        static MetersAPIResponse GetMetersFromPhysicalEGM(EventArgs e)
        {
            MetersAPIResponse resp = new MetersAPIResponse();
            resp.GamesPlayed = physicalEGMController.physicalEGM.GetGamesPlayed();
            resp.GamesWon = physicalEGMController.physicalEGM.GetGamesWon();
            resp.PowerReset = physicalEGMController.physicalEGM.GetPowerReset();
            resp.SlotDoorOpen = physicalEGMController.physicalEGM.GetSlootDoorOpen();
            resp.TotalCoinIn = physicalEGMController.physicalEGM.GetTotalCoinIn();
            resp.TotalCoinOut = physicalEGMController.physicalEGM.GetTotalCoinOut();
            resp.TotalDrop = physicalEGMController.physicalEGM.GetTotalDrop();
            resp.TotalJackPot = physicalEGMController.physicalEGM.GetTotalJackPot();
            resp.M000C_CurrentCredits = physicalEGMController.physicalEGM.Get000C();
            try { resp.CurrentCashableCredits_IN_CENTS = int.Parse(BitConverter.ToString(physicalEGMController.physicalEGM._EGMStatus.currentCashableAmount).Replace("-","")); } catch {}
            resp.M001B_CurrentRestrictedCredits = physicalEGMController.physicalEGM.Get001B();
            try { resp.CurrentNonRestrictedCredits = int.Parse(BitConverter.ToString(physicalEGMController.physicalEGM._EGMStatus.currentNonRestrictedAmount).Replace("-","")); } catch {}
            resp.M00A0_InHouseCashableTransfersToGamingMachine_Cents = physicalEGMController.physicalEGM.GetMeter(0xA0);
            resp.M00A1_InHouseTransfersToGamingMachineWithCashableAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xA1);
            resp.M00A2_InHouseRestrictedTransfersToGamingMachine_Cents = physicalEGMController.physicalEGM.GetMeter(0xA2);
            resp.M00A3_InHouseTransfersToGamingMachineWithRestrictedAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xA3);
            resp.M00A4_InHouseNonRestrictedTransfersToGamingMachine_Cents = physicalEGMController.physicalEGM.GetMeter(0xA4);
            resp.M00A5_InHouseTransfersToGamingMachineWithNonRestrictedAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xA5);
            resp.M00A8_InHouseCashableTransfersToTicket_Cents = physicalEGMController.physicalEGM.GetMeter(0xA8);
            resp.M00A9_InHouseCashableTransfersToTicket_Quantity = physicalEGMController.physicalEGM.GetMeter(0xA9);
            resp.M00AA_InHouseRestrictedTransfersToTicket_Cents = physicalEGMController.physicalEGM.GetMeter(0xAA);
            resp.M00AB_InHouseRestrictedTransfersToTicket_Quantity = physicalEGMController.physicalEGM.GetMeter(0xAB);
            resp.M00AE_BonusCashableTransfersToGamingMachine_Cents = physicalEGMController.physicalEGM.GetMeter(0xAE);
            resp.M00AF_BonusTransfersToGamingMachineWithCashableAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xAF);
            resp.M00B0_BonusNonRestrictedTransfersToGamingMachine_Cents = physicalEGMController.physicalEGM.GetMeter(0xB0);
            resp.M00B1_BonusTransfersToGamingMachineWithNonRestrictedAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xB1);
            resp.M00B8_InHouseCashableTransfersToHost_Cents = physicalEGMController.physicalEGM.GetMeter(0xB8);
            resp.M00B9_InHouseTransfersToHostWithCashableAmount_Quantity = physicalEGMController.physicalEGM.GetMeter(0xB9);
            resp.M00BA_InHouseRestrictedTransfersToHost_Cents = physicalEGMController.physicalEGM.GetMeter(0xBA);
            resp.M00BB_InHouseTransfersToHostWithRestrictedAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xBB);
            resp.M00BC_InHouseNonRestrictedTransfersToHost_Cents = physicalEGMController.physicalEGM.GetMeter(0xBC);
            resp.M00BD_InHouseTransfersToHostWithNonRestrictedAmounts_Quantity = physicalEGMController.physicalEGM.GetMeter(0xBD);
            // Si no está en ninguna redemption, validation o transacción, 
            // If you are not in any redemption, validation or transaction, 
            if (physicalEGMController.NoCurrentRedemption()
            && physicalEGMController.NoCurrentValidation()
            && physicalEGMController.NoCurrentTransaction())
            {
                resp.CreditsTransactionInProgress = false;
            }
            else
            {
                resp.CreditsTransactionInProgress = true;
            }

            return resp;
        }


        /// <summary>
        /// Consultamos las transacciones históricas de la PhysicalEGM
        /// We consult PhysicalEGM's historical transactions.
        /// API Endpoint ->GET V0/Audit/PhysicalEGM/AFTTransactionHistory -> then WebApiHandler throws GetPhysicalEGMAFTTransactionHistory event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <returns>An object of type PhysicalEGMAFTTransactionHistoryAPIResponse</returns>
        static PhysicalEGMAFTTransactionHistoryAPIResponse GetPhysicalEGMAFTTransactionHistory(EventArgs e)
        {
            PhysicalEGMAFTTransactionHistoryAPIResponse resp = new PhysicalEGMAFTTransactionHistoryAPIResponse();
            foreach (TransactionLogLine l in physicalEGMController.transactionsController.GetTransactions())
            {
                PhysicalEGMAFTTransactionHistoryLineResponse line = new PhysicalEGMAFTTransactionHistoryLineResponse();
                line.CashableAmount = l.CashableAmount;
                line.Position = l.Position;
                line.ReceiptStatus = l.ReceiptStatus;
                line.RestrictedAmount = l.RestrictedAmount;
                line.TransactionDateTime = l.TransactionDateTime;
                line.TransactionID = l.TransactionID;
                line.TransferStatus = l.TransferStatus;
                line.TransferType = l.TransferType;
                resp.transactions.Add(line);
            }
            return resp;
        }


        //
        /// <summary>
        ///  Consultamos la info de la PhysicalEGM
        ///  We consult the PhysicalEGM info
        ///  Current with no API endpoint
        /// </summary>
        /// <returns>An object of type EGMInfoResponse</returns>
        static EGMInfoResponse GetPhysicalEGMInfo(EventArgs e)
        {
            EGMInfoResponse resp = new EGMInfoResponse();
            EGMSettings settings = physicalEGMController.physicalEGM.GetEGMSettings();
            EGMInfo info = physicalEGMController.physicalEGM.GetEGMInfo();
            resp.GMSerialNumber = info.GMSerialNumber;
            resp.AdditionalID = info.AdditionalID;
            resp.BasePercentage = info.BasePercentage;
            resp.Denomination = info.Denomination;
            resp.Denomination = info.Denomination;
            resp.GameID = info.GameID;
            resp.GameOptions = settings.GameOptions;
            resp.MaxBet = settings.MaxBet;
            resp.PayTableID = info.PayTableID;
            resp.ProgressiveGroup = settings.ProgressiveGroup;
            resp.GMTransferLimit = settings.gmTransferLimit;

            return resp;
        }


        /// <summary>
        /// Consultamos las trazas en tiempo real
        /// Real-time trace queries
        ///  API Endpoint ->GET V0/Audit/PhysicalEGM/SASTrace -> then WebApiHandler throws GetHostLiveTrace event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <returns>An object of type LiveTraceResponse</returns>
        static LiveTraceResponse GetHostLiveTrace(EventArgs e) {
            // Instanciamos el resultado
            // We instantiate the result
            LiveTraceResponse resp = new LiveTraceResponse();
            // Si el logTracer es no nulo (activo)
            // If logTracer is non-null (active)
            // Si el Logger está activo (no nulo)
            // If the Logger is active (not null)
            if (physicalEGMController.logTracer == null
                || physicalEGMController.logTracer.Logger == null) {
                return resp;
            }
            object lt = null;
            // Obtenemos el SASTrace y lo guardamos en lt
            // We obtain the SASTrace and save it in lt
            physicalEGMController.logTracer.Logger.GetTrace(ref lt);
            if (lt == null) {
                return resp;
            }
            var list = (System.Collections.Generic.List<MainController.LiveTraceLine>)lt;
            list = list.ToList();
            // Recorremos las lines en lt
            // We travel the lines in lt
            foreach (LiveTraceLine l in list)
            {
                // instanciamos una nueva response line
                // we instantiate a new response line
                LiveTraceLineResponse line = new LiveTraceLineResponse();
                line.CRC = l.CRC;
                line.Direction = l.Type;
                line.Message = l.Message;
                line.TimeStamp = l.TimeStamp;
                line.IsRetry = l.IsRetry;
                // Y la guardamos en l
                // And we store it in the
                resp.lines.Add(line);

            }
            // Devolvemos el resultado
            // We return the result
            return resp;
        }


        /// <summary>
        /// Consultamos las trazas en tiempo real
        /// Real-time trace queries
        /// </summary>
        /// <returns></returns>
        static LiveTraceResponse GetClientLiveTrace(EventArgs e)
        {
            LiveTraceResponse resp = new LiveTraceResponse();
            if (virtualEGMController.logTracer == null
                || virtualEGMController.logTracer.Logger == null)
            {
               return resp;
            }

            object lt = null;
            virtualEGMController.logTracer.Logger.GetTrace(ref lt);
            if (lt == null) return resp;
            var list = (System.Collections.Generic.List<MainController.LiveTraceLine>)lt;
            list = list.ToList();
            foreach (LiveTraceLine l in list)
            {
                LiveTraceLineResponse line = new LiveTraceLineResponse();
                line.CRC = l.CRC;
                line.Direction = l.Type;
                line.Message = l.Message;
                line.TimeStamp = l.TimeStamp;
                line.IsRetry = l.IsRetry;
                resp.lines.Add(line);

            }
            return resp;
        }


        /// <summary>
        /// Armado de la clase CurrentTransfer, que se mostrará por API
        /// Assembly of the CurrentTransfer class, to be displayed by API
        /// API Endpoint ->GET  /V0/Transactions/AFT/CurrentTransfer -> then WebApiHandler throws GetPhysicalEGMAFTCurrentTransfer event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <returns>An object of type PhysicalEGMAFTTransactionHistoryLineResponse</returns>
        static PhysicalEGMAFTTransactionHistoryLineResponse GetPhysicalEGMAFTCurrentTransfer(EventArgs e)
        {
            PhysicalEGMAFTTransactionHistoryLineResponse line = new PhysicalEGMAFTTransactionHistoryLineResponse();
            line.CashableAmount = AFTCurrentTransaction.Instance().Amount;
            line.Position = AFTCurrentTransaction.Instance().Position;
            try { line.ReceiptStatus = BitConverter.ToString(new byte[] { AFTCurrentTransaction.Instance().ReceiptStatus }); } catch { }
            line.RestrictedAmount = AFTCurrentTransaction.Instance().RestrictedAmount;
            line.TransactionDateTime = AFTCurrentTransaction.Instance().TransactionDate;
            line.TransactionID = AFTCurrentTransaction.Instance().TransactionID;
            try { line.TransferStatus = BitConverter.ToString(new byte[] { AFTCurrentTransaction.Instance().InternalStatus }); } catch { }
            try { line.TransferType = BitConverter.ToString(new byte[] { AFTCurrentTransaction.Instance().TransferType }); } catch { }

            return line;
        }

        /// <summary>
        /// Armado de la clase HostStatus, que se mostrará por API
        /// Assembly of the HostStatus class, to be displayed by API
        /// API Endpoint ->GET  /V0/Audit/HostStatus -> then WebApiHandler throws GetHostStatus event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <returns>An object of type HostStatusResponse</returns>
        static HostStatusResponse GetHostStatus(EventArgs e)
        {
            HostStatusResponse resp = new HostStatusResponse();
            Encoding encoding = Encoding.Default;
            try { resp.EGMSASVersion = encoding.GetString(physicalEGMController.physicalEGM.GetVersionID()); } catch {}
            try { resp.EGMSerialNUmber  = encoding.GetString(physicalEGMController.physicalEGM.GetGameMachineSerialNumber());} catch {}
            resp.PhysicalEGMController_TimerMetersInitiated = physicalEGMController.TimerMetersInitiated;
            resp.PhysicalEGMController_Timer30SecondsInitiated = physicalEGMController.Timer30SecondsInitiated;
            resp.PhysicalEGMController_Timer300SecondsInitiated = physicalEGMController.Timer300SecondsInitiated;
            resp.PhysicalEGMController_TimerMetersRetries = physicalEGMController.TimerMetersRetries;
            resp.PhysicalEGMController_Timer30SecondsRetries = physicalEGMController.Timer30SecondsRetries;
            resp.PhysicalEGMController_Timer300SecondsRetries = physicalEGMController.Timer300SecondsRetries;
            resp.PhysicalEGMController_FlagTransferToGamingMachine = physicalEGMController.FlagTransferToGamingMachine;
            resp.PhysicalEGMController_FlagTransferFromGamingMachine = physicalEGMController.FlagTransferFromGamingMachine;
            resp.PhysicalEGMController_FlagTransferToPrinter = physicalEGMController.FlagTransferToPrinter;
            resp.PhysicalEGMController_FlagWinAmountPendingCashoutToHost = physicalEGMController.FlagWinAmountPendingCashoutToHost;
            resp.PhysicalEGMController_FlagBonusAwardToGamingMachine = physicalEGMController.FlagBonusAwardToGamingMachine;
            resp.PhysicalEGMController_FlagLockAfterTransferRequestSupported = physicalEGMController.FlagLockAfterTransferRequestSupported;
            resp.Host__lastSend = physicalEGMController.host._lastSend == null ? "" : BitConverter.ToString(physicalEGMController.host._lastSend);
            resp.Host__lastSendTS = physicalEGMController.host._lastSendTS;
            resp.Host__lastReceived = physicalEGMController.host._lastReceived == null ? "" : BitConverter.ToString(physicalEGMController.host._lastReceived);
            resp.Host__lastReceivedTS = physicalEGMController.host._lastReceivedTS;
            resp.Host_communication = physicalEGMController.host.communication == null ? false : physicalEGMController.host.communication.Value;
            resp.Host_address = physicalEGMController.host.address;
            resp.Host_phase = physicalEGMController.host.phase;
            resp.Host_PollingFrecuency = physicalEGMController.host.PollingFrecuency;
            resp.Host__assetNumber = physicalEGMController.host._assetNumber;
            resp.MainController_CurrentTransactionStatus = AFTCurrentTransaction.Instance().status.ToString();
            resp.MainController_CurrentTransactionInProcess = !physicalEGMController.NoCurrentTransaction();
            resp.MainController_InterfacedRedemptionStatus = InterfacedRedemption.Instance().status.ToString();
            resp.MainController_InterfacedRedemptionInProcess = !physicalEGMController.NoCurrentRedemption();
            resp.MainController_InterfacedValidationStatus = InterfacedValidation.Instance().status.ToString();
            resp.MainController_InterfacedValidationInProcess = !physicalEGMController.NoCurrentValidation();
            resp.LastInitUnrepliedLPs = new List<string>();
            foreach (byte lp in physicalEGMController.InitialRoutineLPsWithNoResponse)
            {
                resp.LastInitUnrepliedLPs.Add(BitConverter.ToString(new byte[] {lp}));
            }

            return resp;
        }

        /// <summary>
        /// By API the Physical EGM settings are requested
        ///  API Endpoint ->GET V0/Settings/PhysicalEGMSettings -> then WebApiHandler throws GetPhysicalEGMSettings event, controller captures this event and then makes a call to this method
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static PhysicalEGMSettingsResponse GetPhysicalEGMSettings(EventArgs e)
        {
            byte f1 = physicalEGMController.GetFeatures1();
            byte f2 = physicalEGMController.GetFeatures2();
            byte f3 = physicalEGMController.GetFeatures3();
            PhysicalEGMSettingsResponse resp = new PhysicalEGMSettingsResponse();
            resp.JackpotMultiplier = getBit(f1, 0);
            resp.AFTBonusAwards = getBit(f1, 1);
            resp.LegacyBonusAwards = getBit(f1, 2);
            resp.Tournament = getBit(f1, 3);
            resp.ValidationExtensions = getBit(f1, 4);
            resp.ValidationStyle = (getBit(f1,6) == false && getBit(f1, 5) == false) ? "00" :
                                   (getBit(f1,6) == false && getBit(f1, 5) == true)  ? "01" :
                                   (getBit(f1,6) == true && getBit(f1, 5) == false)  ? "10" : "11";
            resp.TicketRedemption = getBit(f1, 7);
            resp.MeterModelFlag = (getBit(f2,1) == false && getBit(f2,0) == false) ? "00" :
                                  (getBit(f2,1) == false && getBit(f2,0) == true)  ? "01" :
                                  (getBit(f2,1) == true && getBit(f2,0) == false)  ? "10" : "00";
            resp.TicketsToTotalDropAndTotalCancelledCredits = getBit(f2, 2);
            resp.ExtendedMeters =  getBit(f2, 3);
            resp.ComponentAuthentication =  getBit(f2, 4);
            resp.AdvancedFundsTransfer =  getBit(f2, 6);
            resp.MultiDenomExtensions = getBit(f2, 7);
            resp.MaximumPollingRate = getBit(f3, 0);
            resp.MultipleSASProgressiveWinReporting =getBit(f3, 1);
            return resp;
        }



        /// <summary>
        /// By API the interface settings are requested
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static InterfacingSettingsResponse GetInterfacingSettings(EventArgs e)
        {
            InterfacingSettingsResponse resp = new InterfacingSettingsResponse();
            resp.passthrough_lp01 = InterfacingSettings.Singleton().passthrough_lp01;
            resp.passthrough_lp02 = InterfacingSettings.Singleton().passthrough_lp02;
            resp.passthrough_lp03 = InterfacingSettings.Singleton().passthrough_lp03;
            resp.passthrough_lp04 = InterfacingSettings.Singleton().passthrough_lp04;
            resp.passthrough_lp06 = InterfacingSettings.Singleton().passthrough_lp06;
            resp.passthrough_lp07 = InterfacingSettings.Singleton().passthrough_lp07;
            resp.passthrough_lp08 = InterfacingSettings.Singleton().passthrough_lp08;
            resp.passthrough_lp0E = InterfacingSettings.Singleton().passthrough_lp0E;
            resp.passthrough_lp4c = InterfacingSettings.Singleton().passthrough_lp4c;
            resp.passthrough_lp7C = InterfacingSettings.Singleton().passthrough_lp7C;
            resp.passthrough_lp7f = InterfacingSettings.Singleton().passthrough_lp7f;
            resp.passthrough_lp94 = InterfacingSettings.Singleton().passthrough_lp94;
            resp.passthrough_lp80 = InterfacingSettings.Singleton().passthrough_lp80;
            resp.passthrough_lp86 = InterfacingSettings.Singleton().passthrough_lp86;
            resp.validationType = InterfacingSettings.Singleton().validationType.ToString();
            return resp;
        }


        /// <summary>
        /// By API a restart of the interface is requested.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static string Restart(EventArgs e)
        {
            // Si no está en ninguna redemption, validation o transacción, 
            if ((InterfacedRedemption.Instance().status == InterfacedRedemptionStatus.Idle
            || InterfacedRedemption.Instance().status == InterfacedRedemptionStatus.Completed)
            && InterfacedValidation.Instance().status == InterfacedValidationStatus.Idle
            && (AFTCurrentTransaction.Instance().status == AFTCurrentTransactionStatus.Created
            || AFTCurrentTransaction.Instance().status == AFTCurrentTransactionStatus.Completed))
            {
                virtualEGMController.StopVirtualEGM();
                physicalEGMController.StopPhysicalEGM();
                AFTCurrentTransaction.Instance().Destroy();
                InterfacedRedemption.Instance().Destroy();
                InterfacedValidation.Instance().Destroy();
                VirtualEGMCreateLiveTraceController();
                virtualEGMController.StartVirtualEGM();
                virtualEGMController.EnableVirtualEGM();
                PhysicalEGMCreateLiveTraceController();
                physicalEGMController.StartPhysicalEGM();
                physicalEGMController.InitialRoutineLPsWithNoResponse = new List<byte>();
                return "Status:'ACK'";
            }
            else
            {
                return "Status:'Rejected'";
            }

        }


        /// <summary>
        ///  By API you adjust the settings
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static string AFTSettings(dynamic obj, EventArgs e)
        {
            bool sth_updated = false;
            try
            {
                InterfacingSettings.UpdateAssetNumber(int.Parse(obj.AssetNumber.ToString()));
                physicalEGMController.UpdateAssetNumberFromInterfacing(InterfacingSettings.Singleton().AFT_AssetNumber);
                InterfacingSettings.UpdateForceRegistrationOnStartup(bool.Parse(obj.ForceDummyRegistrationOnStartup.ToString()));
                sth_updated = true;
            }
            catch { }
            if (sth_updated)
                return $"Status:'ACK'";
            else
                return "Status:'Error, nothing updated, please review your fields'";
        }

        /// <summary>
        ///  By API get the EGM and Smib link health
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static LinksHealthResponse LinksHealth(EventArgs e)
        {
            LinksHealthResponse resp = new LinksHealthResponse();
            resp.EGMLinkActive = EGMcommunication == true;
            resp.SmibLinkActive = SMIBLinkDown == false;
            resp.LastEGMResponseReceivedAt = physicalEGMController.GetLastEGMResponseReceivedAt();

            return resp;
        }



        #endregion

        /***********************************************************************************************************************/
        /*This region describes the physical egm event handlers: from communication events to exception events. Also it has real time handlers.
        /***********************************************************************************************************************/
        #region PHYSICALEGM_EVENT_HANDLERS

        /*......................................................................................................................*/
        /*Communication event handlers
        /*......................................................................................................................*/
        #region Communication

        /// <summary>
        /// Un evento de comunicación se recibió, con parámetro true si hay comunicación con la EGM, o false si no hay comunicación con la EGM
        /// A communication event was received, with parameter true if there is communication with the EGM, or false if there is no communication with the EGM.
        /// EGM Link down -> Try to stop client; EGM Link up -> enable virtual EGM Controller
        /// </summary>
        /// <param name="communication">The flag representing the communication status of EGM</param>
        /// <param name="e"></param>
        public static void PhysicalEGMCommunicationEv(bool communication, EventArgs e)
        {
            if (communication != EGMcommunication)
            {
                EGMcommunication = communication;
                if (!communication)
                {
                    TimerTryStopClient.Start();
                }
                else
                {
                    virtualEGMController.EnableVirtualEGM();
                }
            }


        }

        #endregion

        /*......................................................................................................................*/
        /*Meter event handlers
        /*......................................................................................................................*/
        #region Meters

        /// <summary>
        /// Meter Updated,
        /// It is used when a meter update event comes from SASResponseHandler and processed by physicalEGMController
        /// Send the update request to virtualEGM
        /// </summary>
        /// <param name="code">Meter byte code</param>
        /// <param name="gameNumber">Game number where the meter belongs</param>
        /// <param name="value">The meter value</param>
        public static void MeterUpdated(byte code, byte[] gameNumber, int value, EventArgs e)
        {
            virtualEGMController.UpdateMeters(code, gameNumber, value);
        }


        /// <summary>
        /// String Meter Updated
        /// It is used when a meter update event comes from SASResponseHandler and processed by physicalEGMController
        /// Send the update request to virtualEGM
        /// </summary>
        /// <param name="str_code">Meter string name</param>
        /// <param name="gameNumber">Game number where the meter belongs</param>
        /// <param name="value">The meter value</param>
        public static void MeterUpdated(string str_code, byte[] gameNumber, int value, EventArgs e)
        {
            virtualEGMController.UpdateMeters(str_code, gameNumber, value);
        }


        #endregion

        /*......................................................................................................................*/
        /*Real Time event handlers
        /*......................................................................................................................*/
        #region RealTime


        /// <summary>
        ///  Realtime event occured in SASResponseHandler, catched and processed by PhysicalEGMController.
        ///  Updates the real time info in VirtualEGM to send the exception in real time mode
        /// </summary>
        /// <param name="rt">True if real time is enabled, False otherwise</param>
        /// <param name="e"></param>
        public static void PhysicalEGMRealTimeEv(byte rt, EventArgs e)
        {
            virtualEGMController.EnableDisableRealTimeEvent(rt);
        }

        #endregion

        /// <summary>
        ///  It is executed when exceptions not covered by other events arrives from EGM to Host, processed by SASResponseHandler and PhysicalEGMController. Some of them arrive with additional data.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="exceptionData"></param>
        /// <param name="e"></param>
        public static void PhysicalExDefault(byte exception, byte[] exceptionData, EventArgs e)
        {
            if (AutoRelayedExceptions.Contains(exception))
                virtualEGMController.EnqueueException(exception, exceptionData);
        }

        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is executed when exception 3D arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Validation SM: from Idle or PhyLP58ResponseReceived or Phy3DReceived -> Phy3DReceived and sends two lp1C and lp4D with 00 to physicalEGM.
        /// </summary>
        /// <param name="e"></param>
        public static void PhysicalEx3D(EventArgs e)
        {
            // Whether it transitioned well to the Phy3DReceived state.
            if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.Phy3DReceived))
            {
                // Send 2 1C
                physicalEGMController.SendMeters();
                physicalEGMController.SendMeters();
                // I send an lp4D
                physicalEGMController.ValidationSendEnhancedValidationInformation(0x00);
            }
        }

        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is executed when exception 3E arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Validation SM: from Idle or Phy3EReceived -> Phy3EReceived and sends two lp1C and lp4D with 00 to physicalEGM.
        /// </summary>
        /// <param name="e"></param>
        public static void PhysicalEx3E(EventArgs e)
        {
            // Whether it transitioned well to the Phy3EReceived state.
            if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.Phy3EReceived))
            {
                // Send 2 1C
                physicalEGMController.SendMeters();
                physicalEGMController.SendMeters();
                // I send an lp4D
                physicalEGMController.ValidationSendEnhancedValidationInformation(0x00);
            }
        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// It is executed when exception 3F arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// The Physical Exception 3F handler. It sends a long poll 4C to the EGM through Host.
        /// </summary>
        public static void PhysicalEx3F(EventArgs e)
        {
            // Mando un lp4C
            physicalEGMController.SetSecureEnhancedValidationID(new byte[] { 0x00, 0x00, 0x01 }, new byte[] { 0x00, 0x00, 0x00 });
        }

        /// <summary>
        /// {{{{{ PART OF HANDPAY PROCESS }}}}}
        /// It is executed when exception 51 arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It sends to PhysicalEGM the long poll 1B
        /// </summary>
        /// <param name="e"></param>
        public static void PhysicalEx51(EventArgs e)
        {
            // Mando un LP1B
            // I send a LP1B
            physicalEGMController.HandpaySendHandpayInformation();
        }


        /// <summary>
        /// {{{{{ PART OF HANDPAY PROCESS }}}}}
        /// It is executed when exception 52 arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It sends the exception 52 to VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void PhysicalEx52(EventArgs e)
        {
            // Mando un ex52 a la virtualEGM
            virtualEGMController.HandPayReset();
        }

        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is executed when exception 57 arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Validation SM: from Idle -> Phy57Received and sends the long poll 57 to physicalEGM.
        /// </summary>
        /// <param name="e"></param>
        public static void PhysicalEx57(EventArgs e)
        {
            // If the Phy57Received state was successfully transitioned to Phy57Received state
            if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.Phy57Received))
            {
                // I send an lp57
                physicalEGMController.ValidationSendPendingCashoutInformation();
            }

        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// It is executed when exception 67 arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Redemption SM: from Idle -> Phy67Received -> PhyLP70Sent and sends a long poll 70 to EGM.
        /// </summary>
        public static void PhysicalEx67(EventArgs e)
        {
            // The interface redemption status is set to Idle.
            if (InterfacedRedemption.Instance().status == InterfacedRedemptionStatus.Idle)
            {
                // If it transitioned well to Phy67Received status
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Phy67Received))
                {
                    // If it transitioned well to PhyLP70Sent
                    if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.PhyLP70Sent))
                    {
                        // I send an lp70
                        physicalEGMController.RedemptionSendTicketValidationData();
                    }
                }
            }
            else
            {
                // If it transitioned well to the Idle state
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Idle))
                {
                    // If it transitioned well to Phy67Received status
                    if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Phy67Received))
                    {
                        // If it transitioned well to PhyLP70Sent
                        if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.PhyLP70Sent))
                        {
                            // I send an lp70
                            physicalEGMController.RedemptionSendTicketValidationData();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// It is executed when exception 68 arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Redemption SM: from PhyLP71ResponsePending -> Phy68Received -> PhyLP71FFSent and sends a long poll 71 to physicalEGM.
        /// </summary>
        public static void PhysicalEx68(EventArgs e)
        {
            // Whether it transitioned well to Phy68Received state
            if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Phy68Received))
            {
                // If it transitioned well to the PhyLP71FFSent state, the PhyLP71FFSent
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.PhyLP71FFSent))
                {
                    // I send an lp71 FF
                    physicalEGMController.RedemptionSendtRedeemTicketCommand();
                }
            }
        }

        /// <summary>
        /// {{{{{ PART OF AFT PROCESS }}}}}
        /// It is executed when exception 69 arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced AFTT SM: from EGMAFTOperationPending -> EGMAFTOperationCompleted -> EGMAFTInterrogated
        /// </summary>
        public static void PhysicalEx69(EventArgs e)
        {
            if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.EGMAFTOperationCompleted))
            {
                if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.EGMAFTInterrogated))
                {
                    physicalEGMController.AFTInterrogate();
                }
                // Nothing at the moment, just transition (The interrogation is made by PhysicalEGMController itself)
            }
        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// It is executed when exception 8C arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It sends a long poll 55 to the EGM through Host.
        /// </summary>
        public static void PhysicalEx8C(EventArgs e)
        {
            physicalEGMController.SendSelectedGameNumber();
        }

        /// <summary>
        /// {{{{{ PART OF HANDPAY PROCESS }}}}}
        /// The Physical LP1B Response handler. 
        /// It is executed when LP1B Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It sends the exception 51 with realtime enabled and info included
        /// </summary>
        /// <param name="progressiveGroup">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="level">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="amount">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="partialPay">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="resetID">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP1BResp(byte progressiveGroup, byte level, byte[] amount, byte[] partialPay, byte resetID, EventArgs e)
        {
            virtualEGMController.HandPay(progressiveGroup,
                                      level,
                                      amount,
                                      partialPay,
                                      resetID);
        }

        /// <summary>
        /// {{{{{ FEATURES AND INFORMATION }}}}}
        /// The Physical LP1F Response handler. 
        /// It is executed when LP1F Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It relay the info to persist in VirtualEGM.
        /// </summary>
        /// <param name="gi">The GamingInfo structure</param>
        /// <param name="e"></param>
        public static void PhysicalLP1FResp(SASResponseHandler.GamingInfo gi, EventArgs e)
        {
            // If you want to disable this, comment
            virtualEGMController.UpdateGamingInfo_GameID(gi._gameID, gi._additionalID, gi._denomination, gi._maxBet, gi._progressiveGroup, gi._gameOptions, gi._paytableID, gi._basePercentage);
        }

        /// <summary>
        /// It is executed when long poll 48 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the last acceted bill information in VirtualEGM persistence
        /// </summary>
        /// <param name="countryCode">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="denominationCode">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="billMeter">Long poll response data. Please refer the protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP48Resp(byte countryCode, byte denominationCode, byte[] billMeter, EventArgs e)
        {
            virtualEGMController.UpdateLastAcceptedBillInformation(countryCode, denominationCode, billMeter);
        }

        /// <summary>
        /// This method is invoked when the long poll 21 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// </summary>
        public static void PhysicalLP21Resp(byte[] romSignature, EventArgs e)
        {
            virtualEGMController.ROMSignatureVerificationResponse(romSignature);
        }

        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is executed when long poll 4D response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Validation SM: from Phy3DReceived or Phy3EReceived -> PhyLP4DResponseReceived and send the exception 3D to Smib and save 4D data 
        /// </summary>
        /// <param name="validationType">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="indexNumber">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="date">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="time">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="validationNumber">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="amount">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="ticketNumber">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="validationSystemId">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="expiration">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="poolId">Long poll response attribute. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP4DResp(byte validationType, byte indexNumber, byte[] date, byte[] time, byte[] validationNumber, byte[] amount, byte[] ticketNumber, byte validationSystemId, byte[] expiration, byte[] poolId, EventArgs e)
        {
            // If the PhyLP4DResponseReceived state was successfully transitioned to PhyLP4DResponseReceived state
            if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.PhyLP4DResponseReceived))
            {
                // I update the data
                InterfacedValidation.Instance().validationType = validationType;
                InterfacedValidation.Instance().indexNumber = indexNumber;
                InterfacedValidation.Instance().date = date;
                InterfacedValidation.Instance().time = time;
                InterfacedValidation.Instance().validationNumber = validationNumber;
                InterfacedValidation.Instance().amount = amount; // CHEQUEAR
                InterfacedValidation.Instance().ticketNumber = ticketNumber;
                InterfacedValidation.Instance().validationSystemId = validationSystemId;
                InterfacedValidation.Instance().expiration = expiration;
                InterfacedValidation.Instance().poolId = poolId;

                // I send the exception and 4D data from the physicalEGM to the VirtualEGM.
                virtualEGMController.PrintingAndSave4DData(InterfacedValidation.Instance().validationType,
                                                 InterfacedValidation.Instance().indexNumber,
                                                 InterfacedValidation.Instance().date,
                                                 InterfacedValidation.Instance().time,
                                                 InterfacedValidation.Instance().validationNumber,
                                                 InterfacedValidation.Instance().amount,
                                                 InterfacedValidation.Instance().ticketNumber,
                                                 InterfacedValidation.Instance().validationSystemId,
                                                 InterfacedValidation.Instance().expiration,
                                                 InterfacedValidation.Instance().poolId,
                                                 InterfacedValidation.Instance().exception);
            }

        }

        /// <summary>
        /// It is executed when long poll 51 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It does some operations looping throught the game numbers from the response.
        /// </summary>
        /// <param name="numberOfGames">Number of games: from long poll response</param>
        /// <param name="e"></param>
        public static void PhysicalLP51Resp(byte[] numberOfGames, EventArgs e)
        {
            // Casteo el byte array a un entero
            // I drop the byte array to an integer
            int numberOfGamesInt = int.Parse(BitConverter.ToString(numberOfGames).Replace("-", ""));
            // Si el número máximo es mayor a 10, actualizo el byte array
            // If the maximum number is greater than 10, update the byte array
            if (numberOfGamesInt > 64)
            {
                numberOfGames = new byte[] { 0x00, 0x64 };
                numberOfGamesInt = 64;
            }
            //Seteo el resultado en physicalEGM
            //Set the result in physicalEGM
            physicalEGMController.SetNumberOfGamesImplemented(numberOfGames);
            //Seteo el resultado en virtualEGM
            //Set the result in virtualEGM
            virtualEGMController.SetNumberOfGamesImplemented(numberOfGames);
            // Casteo en entero el byte array
            // Integer caste byte array

            //Pido a la physicalEGM el cashout limit y la extended information para los 
            //games 0 hasta numberOfGames

            //I ask the physicalEGM for the cashout limit and the extended information for the 
            //games 0 to numberOfGames
            for (int k = 0; k <= numberOfGamesInt; k++)
            {
                byte[] gamebytearray = intToBCD5_v2((uint)k, 2);
                physicalEGMController.SendGameNExtendedInformation(gamebytearray);
                physicalEGMController.SendCashoutLimit(gamebytearray);

                RecipeGetMetersMultiGame getmetersmachine = new RecipeGetMetersMultiGame(); // Instancio una nueva recipe
                getmetersmachine.Init(physicalEGMController);
                getmetersmachine.SetGameForMeters(gamebytearray);
                getmetersmachine.Execute();

            }
        }

        /// <summary>
        /// It is executed when long poll 54 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the versionID and the game machine serial number in VirtualEGM persistence
        /// </summary>
        /// <param name="VersionID">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="GMSerialNumber">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP54Resp(byte[] VersionID, byte[] GMSerialNumber, EventArgs e)
        {
            virtualEGMController.SetVersionID(VersionID);
            virtualEGMController.SetGameMachineSerialNumber(GMSerialNumber);
        }

        /// <summary>
        /// It is executed when long poll 55 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the current game number in Virtual EGM persistence
        /// </summary>
        /// <param name="gameNumber">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP55Resp(byte[] gameNumber, EventArgs e)
        {
            //Seteo el resultado en virtualEGM
            //Set the result in virtualEGM
            virtualEGMController.SetCurrentGameNumber(gameNumber);
        }



        /// <summary>
        /// It is executed when long poll 56 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the enabled game numbers info in Virtual EGM persistence
        /// </summary>
        /// <param name="EnabledGames">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP56Resp(List<byte[]> EnabledGames, EventArgs e)
        {
            //Seteo el resultado en virtualEGM
            //Set the result in virtualEGM
            virtualEGMController.SetEnabledGameNumbers(EnabledGames);
        }


        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is executed when long poll 57 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Validation SM: from Idle or Phy57Received -> Phy57Received -> PhyLP57ResponseReceived and send the exception 57 to Smib -> PhyLP58ResponseReceived
        /// </summary>
        /// <param name="info">The PendingCashoutInformation structure </param>
        /// <param name="e"></param>
        public static void PhysicalLP57Resp(SASResponseHandler.PendingCashoutInformation info, EventArgs e)
        {
            // The status of the interfaced redemption is in Phy57Received.
            if (InterfacedValidation.Instance().status == InterfacedValidationStatus.Phy57Received)
            {
                // If the PhyLP57ResponseReceived state was successfully transitioned to PhyLP57ResponseReceived state
                if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.PhyLP57ResponseReceived))
                {
                    // I update the amount and cashouttype
                    InterfacedValidation.Instance().amount = intToBCD5_v2((uint)info._amount);
                    InterfacedValidation.Instance().cashoutType = info._cashoutType;
                    // I send exception 0x57
                    virtualEGMController.Cashout(InterfacedValidation.Instance().amount, InterfacedValidation.Instance().cashoutType);
                    // If the PhyLP58ResponseReceived state was successfully transitioned to PhyLP58ResponseReceived state
                    if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.PhyLP58ResponseReceived))
                    {
                        // Do Something
                        /*if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.Idle))
                        {

                        }*/
                    }
                }
            }
        }

        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is executed when long poll 58 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Validation SM: from Phy57Received -> PhyLP57ResponseReceived
        /// </summary>
        /// <param name="status">Long poll response attribute</param>
        /// <param name="e"></param>
        public static void PhysicalLP58Resp(byte status, EventArgs e)
        {
            // The status of the interfaced redemption is in PhyLP57ResponseReceived.
            if (InterfacedValidation.Instance().status == InterfacedValidationStatus.PhyLP57ResponseReceived)
            {

            }
        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// It is executed when LP70 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Redemption SM: from PhyLP70Sent -> PhyLP70ResponseReceived -> Vir67Sent and launch exception 67 to Smib through client.
        /// </summary>
        /// <param name="data">The Response70Parameters structure</param>
        /// <param name="e"></param>
        public static void PhysicalLP70Resp(SASResponseHandler.Response70Parameters data, EventArgs e)
        {
            // If the PhyLP70ResponseReceived state was successfully transitioned to PhyLP70ResponseReceived state
            if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.PhyLP70ResponseReceived))
            {
                // Whether it transitioned well to the Vir67Sent state.
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Vir67Sent))
                {
                    // I send exception 0x67
                    virtualEGMController.TicketHasBeenInserted(data.ticketAmount, data.parsingCode, data.validationData);
                }
            }
        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// The Physical LP71 with pending status response handler. 
        /// It is executed when LP71 with pending status response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Redemption SM: from PhyLP71Sent -> PhyLP71ResponsePending.
        /// </summary>
        /// <param name="ticket">The RedeemTicket structure</param>
        /// <param name="e"></param>
        public static void PhysicalLP71_40Resp(SASResponseHandler.RedeemTicket ticket, EventArgs e)
        {
            // If the PhyLP71ResponsePending state was successfully transitioned to PhyLP71ResponsePending state
            if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.PhyLP71ResponsePending))
            {
                // I don't  do nothing
                //virtualEGM.RedeemTicket(ticket.machineStatus);
            }

        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// The Physical LP71 with success status response handler. 
        /// It is executed when LP71 with success status response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Redemption SM: from PhyLP71FFSent -> Accepted -> Vir68Sent -> Completed and launch exception 68 to Smib through client.
        /// </summary>
        /// <param name="ticket">The RedeemTicket structure</param>
        /// <param name="e"></param>
        public static void PhysicalLP71_SuccesfulResp(SASResponseHandler.RedeemTicket ticket, EventArgs e)
        {
            // If the Failed state was successfully transitioned to the Failed state
            if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Accepted))
            {
                // Whether it transitioned well to the Vir68Sent state.
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Vir68Sent))
                {
                    // If it transitioned well to Completed status
                    if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Completed))
                    {
                        // I send exception 0x68
                        virtualEGMController.RedemptionCompleted(ticket.machineStatus);
                        // Send 74
                        physicalEGMController.SendStatusRequest();

                    }
                }
            }

        }

        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// The Physical LP71 with failed status response handler. 
        /// It is executed when LP71 with failed status response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced Redemption SM: PhyLP71FFSent -> Accepted -> Vir68Sent -> Completed and launch exception 68 to Smib through client.
        /// </summary>
        /// <param name="ticket">The RedeemTicket structure</param>
        /// <param name="e"></param>
        public static void PhysicalLP71_FailedResp(SASResponseHandler.RedeemTicket ticket, EventArgs e)
        {
            // If the Failed state was successfully transitioned to the Failed state
            if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Rejected))
            {
                // Whether it transitioned well to the Vir68Sent state.
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Vir68Sent))
                {
                    // If it transitioned well to Completed status
                    if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.Completed))
                    {
                        // I send exception 0x68
                        virtualEGMController.RedemptionCompleted(ticket.machineStatus);
                        // Send 74
                        physicalEGMController.SendStatusRequest();
                    }
                }
            }

        }

        /// <summary>
        /// {{{{{ PART OF AFT PROCESS }}}}}
        /// The Physical LP72 with status of current transfer. 
        /// It is executed when LP72 with current aft operation status response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It changes the state of Interfaced AFT SM in three cases:
        /// ------------------------------------------ [IF TransferStatus is 40] HostAFTOperationSent -> EGMAFTOperationPending.
        /// ------------------------------------------ [IF TransferStatus is error] TODO
        /// ------------------------------------------ [IF there is an interrogation in progress] EGMAFTInterrogated -> ClientException69Sent .
        /// </summary>
        public static void PhysicalLP72(byte status, byte? receiptstatus, byte[] transactionId, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonRestrictedAmount, byte? transferFlags, byte? transferType, byte[] expiration, byte[] poolId, byte? position, DateTime? transactionDate, bool IsCurrentTransferInterrogationResponse, EventArgs e)
        {
            if (status == 0x40)
            {
                if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.EGMAFTOperationPending))
                {
                    // Nothing at the moment, just transition
                }
            }
            else if ((status >= 0x80 && status <= 0x9F))
            {
                if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.EGMAFTOperationRejected))
                {
                    LaunchLog(new string[] {"EGM ->", "-> SMIB"}, $"AFT Transfer rejected. Amount: {BitConverter.ToString(cashableAmount)} Motive: {BitConverter.ToString(new byte[]{ status })}", new EventArgs());
                    // Nothing at the moment, just transition
                }
            }
            if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.ClientException69))
            {
                LaunchLog(new string[] {"EGM ->", "-> SMIB"}, $"AFT Transfer completed. Amount: {BitConverter.ToString(cashableAmount)}", new EventArgs());
                physicalEGMController.SendStatusRequest();
                virtualEGMController.AFTTransferCompleted(status);
            }
            virtualEGMController.AddFinishedAFTTransaction(status, receiptstatus, transactionId, cashableAmount, restrictedAmount, nonRestrictedAmount, transferFlags, transferType, expiration, poolId, position, transactionDate, IsCurrentTransferInterrogationResponse);
            
        }


        /// <summary>
        /// It is executed when long poll 7B response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It process the asset number, update this number in PhysicalEGM and its Host
        /// </summary>
        /// <param name="assetNumber">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="gameLockStatus">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="availableTransfers">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="hostCashoutStatus">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="AFTStatus">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="maxBufferIndex">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="currentCashableAmount">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="currentRestrictedAmount">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="currentNonRestrictedAmount">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="gamingMachineTransferLimit">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="restrictedExpiration">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="restrictedPoolId">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP74Resp(byte[] assetNumber, byte gameLockStatus, byte availableTransfers, byte hostCashoutStatus, byte AFTStatus, byte maxBufferIndex, byte[] currentCashableAmount, byte[] currentRestrictedAmount, byte[] currentNonRestrictedAmount, byte[] gamingMachineTransferLimit, byte[] restrictedExpiration, byte[] restrictedPoolId, EventArgs e)
        {
            int newAssetNumber = BitConverter.ToInt32(assetNumber, 0);
            // Actualiza el Asset Number en la InterfacingSettings
            // Update the Asset Number in the InterfacingSettings
            if (InterfacingSettings.Singleton().AFT_AssetNumber != newAssetNumber)
            {
               InterfacingSettings.UpdateAssetNumber(newAssetNumber);
               LaunchLog(new string[] {"EGM ->"}, $"AssetNumber from long poll 74: received {newAssetNumber}", new EventArgs());
            }
            // Actualizo el asset number hacia el host
            // I update the asset number to the host
            physicalEGMController.UpdateAssetNumberFromInterfacing(InterfacingSettings.Singleton().AFT_AssetNumber);
            // I update the asset number and availableTransfers to virtualEGM and client
            virtualEGMController.Update74ResponseInfo(assetNumber, availableTransfers, gameLockStatus, hostCashoutStatus, AFTStatus, restrictedExpiration, gamingMachineTransferLimit, maxBufferIndex, currentCashableAmount, currentRestrictedAmount, currentNonRestrictedAmount, restrictedPoolId);
        }

        /// <summary>
        /// It is executed when long poll 73 response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It process the registration key and regPosId, update this key in VirtualEGM
        /// </summary>
        public static void PhysicalLP73Resp(byte regStatus, byte[] assetNumber, byte[] registrationKey, byte[] regPOSId, EventArgs e)
        {
            if (physicalEGMController.initialroutine.Instance.InProgress())
            {
                string regkstr = BitConverter.ToString(virtualEGMController.GetRegistrationKey());
                LaunchLog(new string[] { }, $"Current Registration key {(regkstr  == "" ? "Empty" : regkstr)}", new EventArgs());
                regkstr = BitConverter.ToString(registrationKey);
                LaunchLog(new string[] { "EGM ->"}, $"Registration key received at initial routine {(regkstr  == "" ? "Empty" : regkstr)}", new EventArgs());
                LaunchLog(new string[] { "EGM ->"}, $"Registration status received at initial routine: {BitConverter.ToString(new byte[] {regStatus})}", new EventArgs());
            }
            virtualEGMController.UpdateRegInfo(regStatus, registrationKey, regPOSId);


        }

        /// <summary>
        /// It is executed when long poll 7B response arrives to Host, event throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// It process the asset number, update this number in PhysicalEGM, its Host, and VirtualEGM persistence.
        /// </summary>
        /// <param name="assetNumber">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="statusBits">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="cashableTicketAndReceiptExpiration">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="restrictedTicketDefaultExpiration">The long poll info parameter. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP7BResp(byte[] assetNumber, byte[] statusBits, byte[] cashableTicketAndReceiptExpiration, byte[] restrictedTicketDefaultExpiration, EventArgs e)
        {
            // Actualiza el Asset Number en la InterfacingSettings
            // Update the Asset Number in the InterfacingSettings
            InterfacingSettings.UpdateAssetNumber(BitConverter.ToInt32(assetNumber, 0));
            LaunchLog(new string[] { "EGM ->" }, $"AssetNumber from long poll 7B: received {BitConverter.ToInt32(assetNumber, 0)}", new EventArgs());
            // Actualizo el asset number hacia el host
            // I update the asset number to the host
            physicalEGMController.UpdateAssetNumberFromInterfacing(InterfacingSettings.Singleton().AFT_AssetNumber);
            // Manda los datos de la 7B en la VirtualEGM
            // Send the data of the 7B in VirtualEGM
            virtualEGMController.UpdateExtendedValidationStatus(assetNumber, statusBits, cashableTicketAndReceiptExpiration, restrictedTicketDefaultExpiration);
        }

        /// <summary>
        /// The Physical LP7E Response handler. 
        /// It is executed when LP7E Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the date and time in VirtualEGM persistence.
        /// </summary>
        /// <param name="month">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="day">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="year">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="hour">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="minute">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="second">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLP7EResp(int month, int day, int year, int hour, int minute, int second, EventArgs e)
        {
            //Seteo el date en virtualEGM
            //Setting the date in virtualEGM
            virtualEGMController.SetDateAndTime(month, day, year, hour, minute, second);
        }

        /// <summary>
        /// {{{{{ FEATURES AND INFORMATION }}}}}
        /// The Physical LPA0 Response handler. Based on specific bit of feature bytes, it does several steps.
        /// It is executed when LPA0 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// </summary>
        /// <param name="gameNumber">The Long poll response field. Please refer to protocol documentation</param>
        /// <param name="features1">The Long poll response field. Please refer to protocol documentation</param>
        /// <param name="features2">The Long poll response field. Please refer to protocol documentation</param>
        /// <param name="features3">The Long poll response field. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLPA0Response(byte[] gameNumber, byte features1, byte features2, byte features3, EventArgs e)
        {
            // If for byte feature1, bit 6 is 0 and bit 5 is 1
            if (getBit(features1, 6) == false && getBit(features1, 5) == true)
            {
                // The validation type will be System Validation
                InterfacingSettings.UpdateValidationType(InterfacedValidationType.SystemValidation);
                virtualEGMController.SetValidationType(InterfacingSettings.Singleton().validationType.GetHashCode());
                physicalEGMController.SetValidationType(InterfacingSettings.Singleton().validationType.GetHashCode());

            }
            // If for byte feature1, bit 6 is 1 and bit 5 is 0
            else if (getBit(features1, 6) == true && getBit(features1, 5) == false)
            {
                // The validation type will be Enhanced Validation.
                InterfacingSettings.UpdateValidationType(InterfacedValidationType.EnhancedValidation);
                physicalEGMController.ValidationGetAllTickets();
                virtualEGMController.SetValidationType(InterfacingSettings.Singleton().validationType.GetHashCode());
                physicalEGMController.SetValidationType(InterfacingSettings.Singleton().validationType.GetHashCode());

            }

            // Bonusing is enabled
            if (getBit(features1, 2) == true)
            {
                InterfacingSettings.Singleton().BonusingEnabled = true;
                InterfacingSettings.SaveData();
            }
            else
            {
                InterfacingSettings.Singleton().BonusingEnabled = false;
                InterfacingSettings.SaveData();
            }

            // Validation Extensions is enabled
            if (getBit(features1, 4) == true)
            {
                InterfacingSettings.UpdateValidationExtensions(true);
            }
            else
            {
                InterfacingSettings.UpdateValidationExtensions(false);
            }


            // Advanced Funds Transfer
            if (getBit(features2, 6) == false)
            {
               LaunchLog(new string[] {"EGM ->"}, $"********************************************************************", new EventArgs());
               LaunchLog(new string[] {"EGM ->"}, $"****************WARNING: AFT DISABLED IN EGM (by A0)****************", new EventArgs());
               LaunchLog(new string[] {"EGM ->"}, $"********************************************************************", new EventArgs());

            }


            // Setting the features in VirtualEGM and PhysicalEGM
            virtualEGMController.SetFeatures(features1, features2, features3);
            physicalEGMController.SetFeatures(features1, features2, features3);
        }

        /// <summary>
        /// The Physical LPA4 Response handler. 
        /// It is executed when LPA4 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the cashout limit on Virtual EGM persistence.
        /// </summary>
        /// <param name="gameNumber">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="cashoutLimit">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLPA4Resp(byte[] gameNumber, byte[] cashoutLimit, EventArgs e)
        {
            //Seteo el resultado en virtualEGM
            //Set the result in virtualEGM
            virtualEGMController.UpdateCashoutLimit(gameNumber, cashoutLimit);
        }

        /// <summary>
        /// The Physical LPB1 Response handler. 
        /// It is executed when LPB1 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the Current Player denominations in VirtualEGM persistence
        /// </summary>
        /// <param name="currentPlayerDenomination">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLPB1Resp(byte currentPlayerDenomination, EventArgs e)
        {
            //Seteo el resultado en virtualEGM
            //Set the result in virtualEGM
            virtualEGMController.SetCurrentPlayerDenomination(currentPlayerDenomination);
        }

        /// <summary>
        /// The Physical LPB2 Response handler. 
        /// It is executed when LPB2 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the different player denominations in VirtualEGM persistence
        /// </summary>
        /// <param name="NumberOfDenominations">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="PlayerDenominations">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLPB2Resp(byte NumberOfDenominations, byte[] PlayerDenominations, EventArgs e)
        {
            //Seteo el resultado en virtualEGM            
            //Set the result in virtualEGM            
            virtualEGMController.SetDenominations(NumberOfDenominations, PlayerDenominations);

        }

        /// <summary>
        /// The Physical LPB3 Response handler. 
        /// It is executed when LPB3 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the token denomination in VirtualEGM persistence
        /// </summary>
        /// <param name="TokenDenomination">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLPB3Resp(byte TokenDenomination, EventArgs e)
        {
            //Seteo el resultado en virtualEGM
            //Set the result in virtualEGM
            virtualEGMController.SetTokenDenomination(TokenDenomination);

        }

        /// <summary>
        /// The Physical LPB5 Response handler. 
        /// It is executed when LPB5 Response arrives to Host, throwed by SASResponseHandler and processed by PhysicalEGMController.
        /// Updates the game N Info in virtual EGM persistence
        /// </summary>
        /// <param name="gameNumber">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="maxBet">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="progressiveGroup">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="progressiveLevels">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="gameNameLength">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="gameName">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="paytableLength">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="paytableName">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="wagerCategories">Long poll response info. Please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void PhysicalLPB5Resp(byte[] gameNumber, byte[] maxBet, byte progressiveGroup, byte[] progressiveLevels, byte gameNameLength, byte[] gameName, byte paytableLength, byte[] paytableName, byte[] wagerCategories, EventArgs e)
        {
            // Set the game N Info in virtualEGM
            virtualEGMController.UpdateGameNInfo(gameNumber, maxBet, progressiveGroup, progressiveLevels, gameNameLength, gameName, paytableLength, paytableName, wagerCategories);
        }



        #endregion

        /***********************************************************************************************************************/
        /*This region describes the virtual egm event handlers: from communication events to exception events.
        /***********************************************************************************************************************/
        #region VIRTUALEGM_EVENT_HANDLERS

        #region Communication

        /// <summary>
        /// Un evento de caída de la Smib desde la VirtualEGM se recibió, con parámetro true si es verdad o false caso contrario.
        /// A Smib crash event from the VirtualEGM was received, with parameter true if true or false otherwise.
        /// SmibLinkDown -> Try to stop host; SmibLinkUp -> Start physicalEGMController
        /// </summary>
        /// <param name="truth">The flag as true if vegm link is down, or false otherwise</param>
        /// <param name="e"></param>
        public static void VirtualEGMSmibLinkDown(bool truth, EventArgs e)
        {
            if (truth != SMIBLinkDown)
            {
                SMIBLinkDown = truth;
                if (truth)
                {
                    TimerTryStopHost.Start();
                }
                else
                {
                    TimerTryStopHost.Stop();
                    physicalEGMController.StartPhysicalEGM();
                }
            }


        }
        #endregion
        /// <summary>
        /// Shutdown (lock out play) 
        /// Interfacing method: Checking if the lp01 flag is enabled to send the longpoll 01 to the physicalEGM;
        /// It is used when the event LP01 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP01Req(EventArgs e)
        {
            // Relay de long poll 01 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp01)
                physicalEGMController.LockoutGame();
        }


        /// <summary>
        /// Startup (enable play)
        /// Interfacing method: Checking if the lp02 flag is enabled to send the longpoll 02 to the physicalEGM;
        /// It is used when the event LP02 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP02Req(EventArgs e)
        {
            // Relay de long poll 01 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp02)
                physicalEGMController.EnableGame();
        }

        /// <summary>
        /// Sound off (all sounds disabled)
        /// Interfacing method: Checking if the lp03 flag is enabled to send the longpoll 03 to the physicalEGM;
        /// It is used when the event LP03 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP03Req(EventArgs e)
        {
            // Relay de long poll 03 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp03)
                physicalEGMController.SoundOff();
        }

        /// <summary>
        /// Sound on (all sounds enabled)
        /// Interfacing method: Checking if the lp04 flag is enabled to send the longpoll 04 to the physicalEGM;
        /// It is used when the event LP04 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP04Req(EventArgs e)
        {
            // Relay de long poll 04 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp04)
                physicalEGMController.SoundOn();
        }


        /// <summary>
        /// Enable bill acceptor
        /// Interfacing method: Checking if the lp06 flag is enabled to send the longpoll 06 to the physicalEGM;
        /// It is used when the event LP06 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP06Req(EventArgs e)
        {
            // Relay de long poll 06 to EGM through Host, if passthrough is 
            if (InterfacingSettings.Singleton().passthrough_lp06)
                physicalEGMController.EnableBillValidator();
        }


        /// <summary>
        /// Disable bill acceptor 
        /// Interfacing method: Checking if the lp07 flag is enabled to send the longpoll 07 to the physicalEGM;
        /// It is used when the event LP07 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP07Req(EventArgs e)
        {
            // Relay de long poll 94 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp07)
                physicalEGMController.DisableBillValidator();
        }


        /// <summary>
        /// Configure Bill Denominations Long Poll Command
        /// Interfacing method: Checking if the lp08 flag is enabled to send the longpoll 08 to the physicalEGM;
        /// It is used when the event LP08 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="billDenominations">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="billAcceptorFlag">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP08Req(byte[] billDenominations, byte billAcceptorFlag, EventArgs e)
        {
            // Relay de long poll 08 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp08)
                physicalEGMController.ConfigureBillDenominations(billDenominations, billAcceptorFlag);
        }


        /// <summary>
        /// Enable/Disable Real Time Event Reporting
        /// Interfacing method: Checking if the lp0E flag is enabled to send the longpoll 0E to the physicalEGM;
        /// It is used when the event LP0E arrives to Client and processed to VirtualEGM
        /// </summary>
        /// <param name="enable_disable">LP Parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP0EReq(byte enable_disable, EventArgs e)
        {
            // Relay de long poll 0E to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp0E)
                physicalEGMController.EnableDisableRealTimeEvent(enable_disable);
        }

        /// <summary>
        /// Game Delay Message
        /// Interfacing method: Checking if the lp2E flag is enabled to send the long poll 2E to the physicalEGM;
        /// It is used when the event LP2E arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="bufferAmount">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP2EReq(byte[] bufferAmount, EventArgs e)
        {
            // Relay de long poll 2E to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp2E)
                physicalEGMController.SendGameDelayMessage(bufferAmount);
        }

        /// <summary>
        /// ROM Signature Verification
        /// </summary>
        /// <param name="seedValue">ROM Verification seed value</param>
        /// <param name="e"></param>
        public static void VirtualLP21Req(byte[] seedValue, EventArgs e)
        {
            // Relay long poll 21 to EGM through Host
            // TODO: is a check of a passthrough flag needed?

            physicalEGMController.SendROMSignatureVerification(seedValue);
        }

        /// <summary>
        /// Set Secure Enhanced Validation ID Command
        /// Interfacing method: Checking if the lp4C flag is enabled to send the longpoll 4C to the physicalEGM;
        /// Send LP4C to physicalEGM
        /// </summary>
        /// <param name="machineID">LP Parameter, please refer to protocol documentation</param>
        /// <param name="sequenceNumber">LP Parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP4CReq(byte[] machineID, byte[] sequenceNumber, EventArgs e)
        {
            // Relay de long poll 4C to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp4c)
                physicalEGMController.SetSecureEnhancedValidationID(machineID, sequenceNumber);
        }

        /// <summary>
        /// {{{{{ PART OF VALIDATION PROCESS }}}}}
        /// It is used when the event LP4D arrives to Client and processed by VirtualEGM.
        /// Interfacing method. When Smib sends lp 4D, it changes the validation state:  from PhyLP4DResponseReceived -> Completed -> Idle and execute the recipe of validation tickets from 4 to 1. 
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP4DEndValidationReq(EventArgs e)
        {
            // If it transitioned well to Completed status
            if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.Completed))
            {
                // If you transitioned abien to the Idle state
                if (InterfacedValidation.Instance().Transition(InterfacedValidationStatus.Idle))
                {
                    // Execute the recipe of validation tickets from 4 to 1
                    physicalEGMController.ValidationGetTicketsFromIndex4To1();
                    // Send 74
                    physicalEGMController.SendStatusRequest();
                }
            }
        }


        /// <summary>
        /// Receive Validation Number Command 
        /// Interfacing method: When Smib sends lp 58, it relays the long poll to EGM through Host
        /// It is used when the event LP58 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="validationSystemID">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="validationNumber">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP58Req(byte validationSystemID, byte[] validationNumber, EventArgs e)
        {
            // Relay de long poll 58 to EGM through Host
            physicalEGMController.ValidationSendReceiveValidationNumber(validationSystemID, validationNumber);
        }


        /// <summary>
        /// {{{{{ PART OF REDEMPTION PROCESS }}}}}
        /// Redeem Ticket Long Poll.
        /// It is used when the event LP71 arrives to Client and processed by VirtualEGM.
        /// Interfacing method. When Smib sends lp 71, it changes the redemption state:  from Vir67Sent -> VirLP71Received -> VirLP71ResponsePending -> PhyLP71Sent and send the long poll 71 to EGM. 
        /// </summary>
        /// <param name="transferCode">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="amount">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="parsingCode">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="validationData">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP71ReqAccepting(byte transferCode, int amount, byte parsingCode, byte[] validationData, EventArgs e)
        {
            // If it transitioned well to VirLP71Received state
            if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.VirLP71Received))
            {
                // If the VirLP71ResponsePending state was successfully transitioned to VirLP71ResponsePending state
                if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.VirLP71ResponsePending))
                {
                    // If it transitioned well to PhyLP71Sent
                    if (InterfacedRedemption.Instance().Transition(InterfacedRedemptionStatus.PhyLP71Sent))
                    {
                        // Relay de long poll 71 to EGM through Host
                        physicalEGMController.RedemptionSendtRedeemTicketCommand(transferCode, amount, parsingCode, validationData);
                    }
                }
            }
        }


           /// <summary>
        /// {{{{{ PART OF AFT PROCESS }}}}}
        /// AFTTransfer from VirtualEGM
        /// Interfacing method. From SmibAFTOperationIncoming to HostAFTOperationSent
        /// It is used when the event LP72 arrives to Client and processed by VirtualEGM
        /// It changes the state of Interfaced AFT SM: Idle -> SmibAFTOperationIncoming -> HostAFTOperationSent  and launch aft operation 72 to Host, PhysicalEGM.
        /// </summary>
        /// <param name="cashableAmount"></param>
        /// <param name="restrictedAmount"></param>
        /// <param name="nonrestrictedAmount"></param>
        /// <param name="e"></param>
        public static void VirtualLP72Req(byte transferCode, byte[] cashableAmount, byte[] restrictedAmount, byte[] nonrestrictedAmount, byte[] transactionID, byte[] registrationKey, byte[] expiration, EventArgs e)
        {
             if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.SmibAFTOperationIncoming))
            {
                virtualEGMController.TransferInProgress(true);
                if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.HostAFTOperationSent))
                {
                    Task t = new Task(() => physicalEGMController.transfer(transferCode, long.Parse(BitConverter.ToString(cashableAmount).Replace("-", "")), 
                                                                                         long.Parse(BitConverter.ToString(restrictedAmount).Replace("-", "")),
                                                                                         long.Parse(BitConverter.ToString(nonrestrictedAmount).Replace("-", "")), 
                                                                                         transactionID,
                                                                                         registrationKey,
                                                                                         expiration)); // Transferimos
                    t.Start();
                }
            }

        }

        /// <summary>
        /// {{{{{ PART OF AFT PROCESS }}}}}
        /// AFT 72 Interrogation from VirtualEGM
        /// It is used when the event LP72 interrogation response arrives to Client and processed by VirtualEGM
        /// It changes the state of Interfaced AFT SM: ClientException69 -> SmibAFTRequestedInterrogate -> SmibAFTInterrogateCompleted
        /// </summary>
        public static void VirtualLP72InterrogationReq(EventArgs e)
        {
            if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.SmibAFTRequestedInterrogate))
            {
                if (InterfacedAFT.Instance().Transition(InterfacedAFTStatus.SmibAFTInterrogateCompleted))
                {
                    virtualEGMController.TransferInProgress(false);
                    physicalEGMController.host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xB8, 0xB9, 0xBA, 0xBB });
                    physicalEGMController.host.SendSelectedMeter(new byte[] { 0x00, 0x00 }, new byte[] { 0xBC, 0xBD });
                }
            }
        }

        /// <summary>
        /// AFT Registration
        /// It is used when the event LP73 request arrives to Client and processed by VirtualEGM
        /// </summary>
        public static void VirtualLP73Req(byte registrationCode, byte[] registrationKey, byte[] posID, EventArgs e)
        {
            if (registrationKey == null)
            {
                physicalEGMController.host.AFTRegistration(registrationCode, new byte[] { }, -1);
            }
            else
            {
                if (registrationKey.Length == 0)
                {
                    physicalEGMController.host.AFTRegistration(registrationCode, new byte[] { }, -1);
                }
                else
                {
                    physicalEGMController.host.AFTRegistration(registrationCode, registrationKey, BitConverter.ToInt32(posID));
                }
            }
        }

        /// <summary>
        /// AFT lock
        /// It is used when the event LP74 request arrives to Client and processed by VirtualEGM
        /// </summary>
        public static void VirtualLP74Req(byte lockCode, byte lockCondition, byte[] lockTimeout, EventArgs e)
        {
                    physicalEGMController.host.SendLP74(lockCode, lockCondition, lockTimeout);  

        }

        /// <summary>
        /// Set Extended Ticket Data Command
        /// Interfacing method: Checking if the lp7c flag is enabled to send the longpoll 7C to the physicalEGM;
        /// It is used when the event LP7C arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="code">Data element type code </param>
        /// <param name="data">Data value</param>
        /// <param name="e"></param>
        public static void VirtualLP7CReq(byte code, string data, EventArgs e)
        {
            physicalEGMController.UpdateTicketData(code, data);
            // Relay de long poll 7C to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp7C)
                physicalEGMController.SetExtendedTicketData(code, data);
        }


        /// <summary>
        /// Set Ticket Data Command 
        /// Interfacing method: Checking if the lp7D flag is enabled to send the longpoll 7D to the physicalEGM;
        /// It is used when the event LP7D arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="HostId">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="expiration">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="location">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="address1">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="address2">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP7DReq(byte[] HostId, byte expiration, byte[] location, byte[] address1, byte[] address2, EventArgs e)
        {
            Encoding encoding = Encoding.Default;
            // Update ticket location on local persistence
            physicalEGMController.UpdateTicketData(0x00, encoding.GetString(location));
            // Relay de long poll 7D to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp7D)
                physicalEGMController.SetTicketData(HostId, expiration, location, address1, address2);
        }


        /// <summary>
        /// Receive Date and Time Command
        /// Interfacing method: Checking if the lp7F flag is enabled to send the longpoll 7F to the physicalEGM;
        /// It is used when the event LP7F arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="date">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="time">LP Parameter,  please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP7FReq(byte[] date, byte[] time, EventArgs e)
        {
            // Relay de long poll 7F to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp7f)
                physicalEGMController.ReceiveDateAndTime(date, time);
        }

        /// <summary>
        /// Single Level Progressive Broadcast Format
        /// Interfacing method: Checking if the lp80 flag is enabled to send the longpoll 80 to the physicalEGM;
        /// It is used when the event LP80 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="broadcast">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="group">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="level">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="amount">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP80Req(bool broadcast, byte group, byte level, byte[] amount, EventArgs e)
        {
            // Relay de long poll 80 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp80)
                physicalEGMController.SingleLevelProgressiveBroadcast(InterfacingSettings.Singleton().SASProgressiveBroadcastPassThrough && broadcast, group, level, amount);
        }


        /// <summary>
        /// Multiple Level Progressive Broadcast Format 
        /// Interfacing method: Checking if the lp86 flag is enabled to send the longpoll 86 to the physicalEGM;
        /// It is used when the event LP86 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="broadcast">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="group">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="amountsAndLevels">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP86Req(bool broadcast, byte group, List<Tuple<byte, byte[]>> amountsAndLevels, EventArgs e)
        {
            // Relay de long poll 86 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp86)
                physicalEGMController.MultipleLevelProgressiveBroadcast(InterfacingSettings.Singleton().SASProgressiveBroadcastPassThrough && broadcast, group, amountsAndLevels);
        }



        /// <summary>
        /// Initiate Legacy Bonus Command 
        /// Interfacing method: Checking if the lp8A flag is enabled to send the longpoll 8A to the physicalEGM;
        /// It is used when the event LP8A arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="bonusAmount">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="taxStatus">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLP8AReq(byte[] bonusAmount, byte taxStatus, EventArgs e)
        {
            // Relay de long poll 8A to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp8A)
            {
                physicalEGMController.InitiateLegacyBonus(bonusAmount, taxStatus);
            }
        }

        /// <summary>
        /// Remote Handpay Reset
        /// Interfacing method: Checking if the lp94 flag is enabled to send the longpoll 94 to the physicalEGM;
        /// It is used when the event LP94 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="e"></param>
        public static void VirtualLP94Req(EventArgs e)
        {
            // Relay de long poll 94 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lp94)
                physicalEGMController.ResetHandpayGamingMachine();
        }




        /// <summary>
        /// Enable Jackpot Handpay Reset Method Command 
        /// Interfacing method: Checking if the lpA8 flag is enabled to send the longpoll A8 to the physicalEGM;
        /// It is used when the event LPA8 arrives to Client and processed by VirtualEGM
        /// </summary>
        /// <param name="resetMethod">Long poll parameter, please refer to protocol documentation</param>
        /// <param name="e"></param>
        public static void VirtualLPA8Req(byte resetMethod, EventArgs e)
        {
            // Relay de long poll A8 to EGM through Host, if passthrough is enabled
            if (InterfacingSettings.Singleton().passthrough_lpa8)
                physicalEGMController.EnableJackpotHandpayResetMethod(resetMethod);
        }








        #endregion

        /***********************************************************************************************************************/
        /*This region describes the methods for reset some processes like redemption and validation.
        /***********************************************************************************************************************/
        #region PROCESS_CONTROL

        /// <summary>
        /// Launch log to the service status. Prints a message with specific tags and the associated timestamp
        /// </summary>
        public static void LaunchLog(string[] tags, string message, EventArgs e)
        {
            string tagsSTR = "";
            foreach (string t in tags)
            {
                tagsSTR = tagsSTR + "[" + t + "]";
            }
            Console.WriteLine($"[{DateTime.Now}]{tagsSTR} {message}");
        }
        /// <summary>
        /// AFT Transaction Completed
        /// It is used by physicalEGMController
        /// </summary>
        public static void PhysicalAFTTransactionComp(EventArgs e)
        {
            virtualEGMController.EnableTransfers();
        }

        public static void InitialRoutineFinish(EventArgs e)
        {
            virtualEGMController.EnableTransfers();
        }

        /// <summary>
        /// Reset Redemption method.
        /// It is used at the timer handler of watchdog, setting the current state of redemption state machine to Idle
        /// </summary>
        public static void ResetRedemption(EventArgs e)
        {
            virtualEGMController.ResetCurrentTicketRedemption();
        }

        /// <summary>
        /// Reset Validation method.
        /// It is used at the timer handler of watchdog, setting the current state of validation state machine to Idle
        /// </summary>
        public static void ResetValidation(EventArgs e)
        {
            virtualEGMController.ResetCurrentTicketValidation();
        }

         /// <summary>
        /// Reset AFT method.
        /// It is used at the timer handler of watchdog, setting the current state of AFT state machine to Idle
        /// </summary>
        public static void ResetAFT(EventArgs e)
        {
            virtualEGMController.TransferInProgress(false);
            virtualEGMController.ResetCurrentTransfer();
        }


        #endregion


 
    }
}
