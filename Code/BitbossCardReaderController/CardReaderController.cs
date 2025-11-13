using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Linq;
using BitbossCardReaderController.Responses;
namespace BitbossCardReaderController
{

    /// <summary>
    /// El status del Polleo, que puede ser
    /// Polling
    /// Stopped
    /// </summary>
    public enum PollingStatus
    {
        Polling,
        Stopped
    }

    /// <summary>
    /// La estructura del poll
    /// A grandes rasgos, consta de la dirección, el código de la función, el length y el payload
    /// </summary>
    public class PollStructure
    {
        private byte Address;
        private byte FunctionCode;
        private byte? Length;
        private byte[] PayLoad;

        
        /// <summary>
        /// Para instanciar una estructura de poll, simplemente necesitamos la function code, la dirección del remitente y la dirección de destino
        /// </summary>
        public PollStructure(byte addressFrom,
                             byte addressTo,
                             byte functioncode)
        {
            Address = (byte)(((int)addressFrom << 4) | (int)addressTo); // (Sender << 4) | Recipient
            FunctionCode = functioncode;
            Length = null;
            PayLoad = null;
        }

        /// <summary>
        /// Otra manera para instanciar una estructura de poll, necesitamos la function code, la dirección del remitente, la dirección de destino y el payload
        /// </summary>
        public PollStructure(byte addressFrom,
                             byte addressTo,
                             byte functioncode,
                             byte[] payload)
        {
            Address = (byte)(((int)addressFrom << 4) | (int)addressTo); // (Sender << 4) | Recipient
            FunctionCode = functioncode;
            PayLoad = payload;
            Length = (byte)(4 + PayLoad.Count()); // 4 + the number of payload bytes
        }

        /// <summary>
        /// Construimos el array resultante de esta instancia
        /// </summary>   
        public byte[] ToArray()
        {
            byte[] body = new byte[] {};
            // Si hay length, significa que hay payload
            if (Length!= null)
            {
                // Construyo el array con address + functioncode, length + payload
                body = ArrayMethodsModule.join( new byte[] {Address}, 
                                                new byte[] {FunctionCode}, 
                                                new byte[] {Length.Value},
                                                PayLoad);
            }
            else
            {
                // Construyo el array con addres + functioncode
                body = ArrayMethodsModule.join( new byte[] {Address}, 
                                                new byte[] {FunctionCode});
                // Retorno el body sin el checksum
                return body;
            }
            // Tengo el body, retorno con el checksum
            // checksum = (NOT(Addr+FC+Len+Payload)) + 1
            return ArrayMethodsModule.join(body,
                                           new byte[] {(byte)((int)(~(ArrayMethodsModule.sum(body)))+(int)1)});
        }
    }

    /// <summary>
    /// La clase CardReaderController
    /// </summary>
    public class CardReaderController
    {

            /// <summary>
            /// Un evento que se lanza cuando se envía un long poll
            /// </summary>
            public event CommandSentHandler CommandSent; public delegate void CommandSentHandler(string cmd, EventArgs e);

            /// <summary>
            ///  Un evento que se lanza cuando se recibe un long poll
            /// </summary>
            public event CommandReceivedHandler CommandReceived; public delegate void CommandReceivedHandler(string cmd, EventArgs e);

            public ResponseHandler responseHandler;
            // El puerto serial
            static SerialPort _serialPort;
            // El estado de polleo
            private PollingStatus pollingStatus;
            // Un timer, para establecer la frecuencia de polleo
            private System.Timers.Timer aTimer;
            // La dirección del remitente, en este caso la dirección del controller
            private byte addressFrom = 0x08;
            // La dirección destino, en este caso del card reader
            private byte addressTo = 0x01;
            private GetReaderStatus_Response response_GetReaderStatus = new GetReaderStatus_Response();
            public byte CardReaderStatus;
            public byte? CardType;
            public byte? Track1Status;
            public byte? Track1Len;
            public byte[] Track1Data;
            public byte? Track2Status;
            public byte? Track2Len;
            public byte[] Track2Data;
            public DateTime CardReaderStatusLastUpdated;
            /// <summary>
            ///  Inicialización del CardReaderController
            /// </summary>
            public CardReaderController(string port)
            {
                // Instancio el response interpreter
                responseHandler = new ResponseHandler();
                // Actualización de la data del reader status 
                responseHandler.GetReaderStatus_Received += new ResponseHandler.ResponseReceivedHandler<GetReaderStatus_Response>(
                    (resp, e) => 
                    {
                        if (!response_GetReaderStatus.Equals(resp))
                        {
                            response_GetReaderStatus = resp;
                            CardReaderStatus = response_GetReaderStatus.CardReaderStatus;
                            CardType = response_GetReaderStatus.CardType;
                            Track1Status = response_GetReaderStatus.Track1Status;
                            Track1Len = response_GetReaderStatus.Track1Len;
                            Track1Data = response_GetReaderStatus.Track1Data;
                            Track2Status = response_GetReaderStatus.Track2Status;
                            Track2Len = response_GetReaderStatus.Track2Len;
                            Track2Data = response_GetReaderStatus.Track2Data;
                            CardReaderStatusLastUpdated = DateTime.Now;
                        }
                    }
                );
                // Comienza en Stopped
                pollingStatus = PollingStatus.Stopped;
                // Create a new SerialPort object with default settings.
                _serialPort = new SerialPort();

                // Allow the user to set the appropriate properties.
                _serialPort.ParityReplace = (byte)'\0';
                _serialPort.ReadBufferSize = 128;
                _serialPort.WriteBufferSize = 128;
                _serialPort.Parity = Parity.None; // None
                _serialPort.DataBits = 8; // 8
                _serialPort.StopBits = StopBits.One; // CAMBIAR A 1
                _serialPort.Handshake = Handshake.None; // None

                _serialPort.PortName = port;
                _serialPort.BaudRate = 115200; // CAMBIAR a 115200

                // Set the read/write timeouts
                _serialPort.ReadTimeout = 1000;
                _serialPort.WriteTimeout = 1000;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                // Create a timer with a two second interval.
                aTimer = new System.Timers.Timer(2000);
                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
            }

            /// <summary>
            ///  Handler cuando recibe data
            /// </summary>
            private void DataReceivedHandler(
                                object sender,
                                SerialDataReceivedEventArgs e)
            {
                // Se duerme 100 milisegundos
                Thread.Sleep(100);
                byte[] _response;
                SerialPort sp = (SerialPort)sender;
                if (sp.BytesToRead > 0)
                {
                    // Armo el array _response para guardar la data
                    _response = new byte[sp.BytesToRead];
                    // Guardo la data
                    sp.Read(_response, 0, _response.Length);
                    responseHandler.Analyze(_response);
                    // Lanzo el evento CommandReceived
                    try{ CommandReceived(BitConverter.ToString(_response), null);} catch {}
                }
            }

            /// <summary>
            ///  Handler cuando hay un Tick del timer
            /// </summary>

            private void OnTimedEvent(Object source, ElapsedEventArgs e)
            {
                // Instancio un poll con comando 0x00
                PollStructure poll = new PollStructure(addressFrom, addressTo, 0x00);
                // Envío el poll
                SendPoll(poll);

            }

            /// <summary>
            ///  Método que envia un poll, toma una estructura de poll, lo convierte a array y lo envía a través del puerto
            /// </summary>
            private void SendPoll(PollStructure poll)
            {
                byte[] message = poll.ToArray();
                 _serialPort.Write(message, 0, message.Length);  
                 try{CommandSent(BitConverter.ToString(message), null);} catch {}
            }
        
            /// <summary>
            ///  Método Start del controller. Cambia el estado a Polling, abro el puerto y disparo el timer
            /// </summary>
            public void Start()
            {
                pollingStatus = PollingStatus.Polling;
                _serialPort.Open();
                aTimer.Start();
            }

            /// <summary>
            ///  Método Stop del controller. Cambia el estado a Stopped, cierro el puerto y detengo el timer
            /// </summary>
            public void Stop()
            {
                pollingStatus = PollingStatus.Stopped;
                aTimer.Stop();
                _serialPort.Close();
            }

            /// <summary>
            ///  Método SetEGMId del controller
            /// </summary>
            public void SetEGMId(byte[] AssetNumber,
                                 byte[] Denom,
                                 byte[] SerialNum,
                                 byte[] Location)
            {
                if (SerialNum.Count() > 40)
                {
                    SerialNum = SerialNum.Take(40).ToArray();
                }

                if (Location.Count() > 40)
                {
                    Location = Location.Take(40).ToArray();
                }

                byte[] PayLoad = ArrayMethodsModule.join(AssetNumber,
                                                         Denom,
                                                         new byte[] { (byte) SerialNum.Count() },
                                                         SerialNum,
                                                         new byte[] { (byte) Location.Count() },
                                                         Location);
                // Instancio un poll con comando 0x06 con el Payload construido
                PollStructure poll = new PollStructure(addressFrom, addressTo, 0x06, PayLoad);
                SendPoll(poll);
            }

            /// <summary>
            ///  Método GetCardReaderStatus del controller
            /// </summary>
            public void GetCardReaderStatus()
            {

                // Instancio un poll con comando 0x01 con el Payload construido
                PollStructure poll = new PollStructure(addressFrom, addressTo, 0x01, new byte[] {});
                SendPoll(poll);
            }

    }
}
