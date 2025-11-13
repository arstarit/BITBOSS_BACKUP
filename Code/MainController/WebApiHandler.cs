using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MainController
{
    /// <summary>
    /* La clase que recibe y envía mensajes hacia la API (que es un proceso aparte) a través de los pipes.  */
    /* The class that receives and sends messages to the API (which is a separate process) through the pipes.  */
    /// </summary>
    class WebApiHandler
    {

        private bool stopped = false;
        private static List<string> GetGroupValues(string pattern, string input)
        {
            Regex reg = new Regex(pattern);
            MatchCollection results = reg.Matches(input);
            Match Match_ = results.FirstOrDefault();
            List<string> result_group_values = new List<string>();
            if (Match_ != null)
            {
                if (Match_.Groups.Count > 1)
                {
                    foreach (Group g in Match_.Groups)
                    {
                        if (g.Index != 0)
                            result_group_values.Add(g.Value);
                    }
                }
            }
            return result_group_values;
        }
        // El Handler del WebApi tiene Una task t
        // The WebApi Handler has a task t
        Task t;
        public static EventArgs e = null;
        // El WebApi lanza los siguientes eventos
        // WebApi launches the following events

        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event AFTTransferFundsHandler AFTTransferFunds; public delegate TransactionAPIResponse AFTTransferFundsHandler(int cashableAmount, int restrictedAmount, int nonrestrictedAmount, EventArgs e);
          /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event LinksHealthHandler LinksHealth; public delegate LinksHealthResponse LinksHealthHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event AFTCashoutFundsHandler AFTCashoutFunds; public delegate TransactionAPIResponse AFTCashoutFundsHandler(int cashableAmount, int restrictedAmount, int nonrestrictedAmount, EventArgs e);      
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetPhysicalEGMSettingsHandler GetPhysicalEGMSettings; public delegate PhysicalEGMSettingsResponse GetPhysicalEGMSettingsHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetMetersFromPhysicalEGMHandler GetMetersFromPhysicalEGM; public delegate MetersAPIResponse GetMetersFromPhysicalEGMHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetPhysicalEGMAFTTransactionHistoryHandler GetPhysicalEGMAFTTransactionHistory; public delegate PhysicalEGMAFTTransactionHistoryAPIResponse GetPhysicalEGMAFTTransactionHistoryHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetPhysicalEGMAFTCurrentTransferHandler GetPhysicalEGMAFTCurrentTransfer; public delegate PhysicalEGMAFTTransactionHistoryLineResponse GetPhysicalEGMAFTCurrentTransferHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetPhysicalEGMInfoHandler GetPhysicalEGMInfo; public delegate EGMInfoResponse GetPhysicalEGMInfoHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetHostLiveTraceHandler GetHostLiveTrace; public delegate LiveTraceResponse GetHostLiveTraceHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetClientLiveTraceHandler GetClientLiveTrace; public delegate LiveTraceResponse GetClientLiveTraceHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetInterfacingSettingsHandler GetInterfacingSettings; public delegate InterfacingSettingsResponse GetInterfacingSettingsHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event GetHostStatusHandler GetHostStatus; public delegate HostStatusResponse GetHostStatusHandler(EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event AFTSettingsHandler AFTSettings; public delegate string AFTSettingsHandler(dynamic parameters, EventArgs e);
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event RestartHandler Restart; public delegate string RestartHandler(EventArgs e);

        object ProcessCommand(string msg) {

            if (stopped == true) { // Si está en stopped
                return new { error = $"Stopped" };
            }
            // llega un pedido de transferencia de fondos
            // a funds transfer request arrives
            if (msg.StartsWith("afttransferfunds ")) {
                // Parseo el comando para obtener sus argumentos
                // I parse the command to obtain its arguments
                List<string> values = GetGroupValues("afttransferfunds ([\\d]+) ([\\d]+) ([\\d]+)", msg);
                if (values.Count == 3) {
                    // Lanzo el evento AFTTransferFunds hacia el MainController con sus tres argumentos: Restricted, Cashable y NonRestricted
                    // I fire the AFTTransferFunds event to the MainController with its three arguments: Restricted, Cashable and NonRestricted.
                    // TransactionAPIResponse
                    return AFTTransferFunds(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), e);
                }
                return new { error = $"incorrect argument count {values.Count}" };
            }
            // llega un pedido de cashout
            // a cashout request arrives
            if (msg.StartsWith("aftcashoutfunds ")) {
                // Parseo el comando para obtener sus argumentos
                // I parse the command to obtain its arguments
                List<string> values = GetGroupValues("aftcashoutfunds ([\\d]+) ([\\d]+) ([\\d]+)", msg);
                if (values.Count == 3) {
                    // Lanzo el evento AFTCashoutFunds hacia el MainController con sus tres argumentos: Restricted, Cashable y NonRestricted
                    // I throw the AFTCashoutFunds event to the MainController with its three arguments: Restricted, Cashable and NonRestricted.
                    // TransactionAPIResponse
                    return AFTCashoutFunds(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), e);
                }
                return new { error = $"incorrect argument count {values.Count}" };
            }
            if (msg.StartsWith("aftsettings ")) {
                string value = msg.Replace("aftsettings ", "");
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(value);
                return AFTSettings(obj, e); // string
            }

            switch (msg) {
                case "Restart":
                    // llega un pedido de restart de la interface
                    // an interface restart request arrives
                    stopped = true;
                    string rsp = Restart(e);
                    // WritePipe(rsp);
                    stopped = false;
                    return rsp;
                case "APICommHealth":
                    // an interface health request arrives 
                    return "Communication from SlotController OK";
                case "LinksHealth":
                    return LinksHealth(e); // LinksHealthResponse
                case "GetMetersFromPhysicalEGM":
                    // Lanzo el evento GetMetersFromPhysicalEGM, hacia el MainController
                    // I trigger the GetMetersFromPhysicalEGM event, to the MainController
                    return GetMetersFromPhysicalEGM(e); // MetersAPIResponse
                case "GetPhysicalAFTTransactionHistory":
                    // Lanzo el evento GetPhysicalEGMAFTTransactionHistory, hacia el MainController
                    // I fire the GetPhysicalEGMAFTTransactionHistory event, to the MainController
                    return GetPhysicalEGMAFTTransactionHistory(e); // PhysicalEGMAFTTransactionHistoryAPIResponse
                case "GetPhysicalEGMInfo":
                    // Lanzo el evento GetPhysicalEGMInfo
                    // I launch the GetPhysicalEGMInfo event.
                    return GetPhysicalEGMInfo(e); // EGMInfoResponse
                case "GetHostLiveTrace":
                    // Lanzo el evento GetLiveTrace
                    // Launch GetLiveTrace event
                    return GetHostLiveTrace(e); // LiveTraceResponse
                case "GetClientLiveTrace":
                    // Lanzo el evento GetLiveTrace
                    // Launch GetLiveTrace event
                    return GetClientLiveTrace(e); // LiveTraceResponse
                case "GetCurrentTransfer":
                    // Lanzo el evento GetPhysicalEGMAFTCurrentTransfer
                    // I launch the GetPhysicalEGMAFTCurrentTransfer event.
                    return GetPhysicalEGMAFTCurrentTransfer(e); // PhysicalEGMAFTTransactionHistoryLineResponse
                case "GetInterfacingSettings":
                    // Lanzo el evento GetPhysicalEGMAFTCurrentTransfer
                    // I launch the GetPhysicalEGMAFTCurrentTransfer event.
                    return GetInterfacingSettings(e); // InterfacingSettingsResponse
                case "GetPhysicalEGMSettings":
                    return GetPhysicalEGMSettings(e);
                case "GetHostStatus":
                    // Lanzo el evento GetHostStatus
                    // I trigger the GetHostStatus event
                    return GetHostStatus(e); // HostStatusResponse
            }
            return new { error = $"unknown command {msg}" };
        }
        /// <summary>
        /// Listens the pipe.
        /// </summary>
        void ListenPipe() {
            while (true) {
                try {
                    using (NamedPipeServerStream pipeStream
                        = new NamedPipeServerStream(
                            "Abc1",
                            PipeDirection.InOut,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous
                    )) {
                        pipeStream.WaitForConnection();
                        // Console.WriteLine($"after WaitForConnection");

                        using (StreamReader sr = new StreamReader(pipeStream))
                        using (StreamWriter sw = new StreamWriter(pipeStream))
                        {
                            sw.AutoFlush = true;
                            while (true) {
                                string msg = sr.ReadLine();
                                // Console.WriteLine($"msg: {msg}");
                                var obj = ProcessCommand(msg);
                                // var obj = "test";
                                sw.WriteLine(JsonConvert.SerializeObject(obj));
                                // Console.WriteLine($"after WriteLine");
                            }
                        }

                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainController.WebApiHandler"/> class.
        /// </summary>
        public WebApiHandler()
        {
            t = new Task(ListenPipe);
        }

        /// <summary>
        /// Start this instance.
        /// </summary>
        public void Start()
        {
            t.Start();
            t.Wait();
        }
    }
}
