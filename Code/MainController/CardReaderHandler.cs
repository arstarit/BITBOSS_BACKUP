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
using BitbossCardReaderController;

namespace MainController
{
    /// <summary>
    /* La clase que recibe y envía mensajes hacia el CardReader. En principio el card reader forma parte de esta clase y hace uso de los métodos. Pero contiene pipes para comunicarse con un hipotético process del card reader controller */

    /* The class that receives and sends messages to the CardReader. In principle the card reader is part of this class and makes use of the methods. But it contains pipes to communicate with a hypothetical process of the card reader controller. */
    /// </summary>
    class CardReaderHandler
    {

        private bool stopped = false;
        /// <summary>
        ///  Un handler o controlador que se comunica con el cardReaderController 
        ///  A handler or controller that communicates with the cardReaderController 
        /// </summary>
        static CardReaderController cardReaderController;
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
        // El Handler del WebApi tiene
        // Una task t
        // The WebApi Handler has
        // A task t
        Task t;
        public static EventArgs e = null;
        // El WebApi lanza los siguientes eventos
        // WebApi launches the following events
        /// <summary>
        /// Occurs when a request for this arrived
        /// </summary>
        public event MessageReceivedHandler MessageReceived; public delegate void MessageReceivedHandler(string msg, EventArgs e);

        // Diccionario de pipes
        // Pipe dictionary
        static Dictionary<string, string> pipe_name = new Dictionary<string, string>();


        /// <summary>
        /// Writes the pipe.
        /// </summary>
        /// <param name="obj">Object.</param>
        static void WritePipe(dynamic obj)
        {
            string json = JsonConvert.SerializeObject(obj);

            using (NamedPipeClientStream pipeStreamSend = new NamedPipeClientStream(pipe_name["Send"]))
            {
                pipeStreamSend.Connect();
                using (StreamWriter sw = new StreamWriter(pipeStreamSend))
                {
                    sw.WriteLine(json);
                }
                pipeStreamSend.Close();

            }
        }

        /// <summary>
        /// Listens the pipe.
        /// </summary>
        void ListenPipe()
        {
            //Create a named pipe using the current process ID of this application
            while (true)
            {
                using (NamedPipeServerStream pipeStreamRecv = new NamedPipeServerStream(pipe_name["Receive"]))
                {
                    //wait for a connection from another process
                    pipeStreamRecv.WaitForConnection();

                    // Se usa un stream reader para la lectura de mensajes
                    // A stream reader is used to read messages.
                    using (StreamReader sr = new StreamReader(pipeStreamRecv))
                    {
                        // Queda a la espera de nuevos mensajes
                        // We look forward to receiving further messages
                        while (sr.Peek() >= 0)
                        {
                            string msg = sr.ReadLine();
                            
                            
                            if (stopped == true) // Si está en stopped // If it is stopped 
                            {
                                string rsp = "Error: Stopped";
                                WritePipe(rsp);
                            }
                            else if (stopped == false)
                            {
                               
                            }

                        }
                    }

                }
            }
        }

        public void SetEGMIdRequest(SetEGMIdRequest request)
        {
            cardReaderController.SetEGMId(request.AssetNumber,
                                          request.Denom,
                                          request.SerialNumber, 
                                          request.Location);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainController.WebApiHandler"/> class.
        /// </summary>
        public CardReaderHandler(string port)
        {

            pipe_name.Add("Receive", "Abcd1");
            pipe_name.Add("Send", "Abcd2");

             t = new Task(ListenPipe);

            cardReaderController = new CardReaderController(port); //args[2]);
            cardReaderController.CommandReceived += new CardReaderController.CommandReceivedHandler((msg, e) => MessageReceived(msg, e));
           
        }

        /// <summary>
        /// Start this instance.
        /// </summary>
        public void Start()
        {
            // t.Start();
            // t.Wait();
            // Starteo del CardReader
            cardReaderController.Start();
        }
    }
}
