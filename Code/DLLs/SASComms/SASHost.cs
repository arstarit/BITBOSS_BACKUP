using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SASComms;
using System.Collections;
using System.Linq;
using System.Diagnostics;

namespace SASComms
{
    /// <summary>
    /// El SASHost, que tiene conexión con la EGM. A diferencia del client, posee un serial port de tipo SerialPortLinux. Dicha clase contiene las primitivas para poder usar el puerto a nivel Unix. 
    /// Tiene diferentes eventos, como por ejemplo el link down. 
    /// Posee un longpollfactory para construir long polls y enviarselos a la EGM, y SASResponseHandler que analiza las respuestas de la máquina

    /// The SASHost, has a conection with the EGM. In this case, has a serial port of SerialPortLinux type. This class contains the primitive for use the port at unix level.
    /// It has different events, for example the link down
    /// It has a long poll factory for build long polls and send them to EGM. It has too a SASResponseHandler for analyze the machine responses
    /// </summary>
    public class SASHost
    {

        /// <summary>
        /// Un evento que se lanza cuando se envía un long poll
        /// An event that it is throwed when a long poll is sent
        /// </summary>
        public event CommandSentHandler CommandSent; public delegate void CommandSentHandler(string cmd, bool crc, bool retry, EventArgs e);

        /// <summary>
        ///  Un evento que se lanza cuando se recibe un long poll
        /// An event that it is throwed when a long poll is received
        /// </summary>
        public event CommandReceivedHandler CommandReceived; public delegate void CommandReceivedHandler(string cmd, bool crc, EventArgs e);

        /// <summary>
        ///  Un evento que se lanza cuando se recibe un dato o información de la EGM o client
        /// An event that it is throwed when a data or information is received from EGM or client
        /// </summary>
        public event ShowMessageHandler DataReceived; public delegate void ShowMessageHandler(string cmd, EventArgs e);


        public EventArgs e = null;

        /// <summary>
        /// Un puerto serial, por donde se comunica el host
        /// A serial port, through which the host communicates
        /// </summary>
        static SerialPort _serialPort;

        /// <summary>
        /// Un timer de polleo. Cada un cierto intervalo de tiempo, el host envía comandos
        /// A polling timer. Every certain interval of time, the host sends commands
        /// </summary>
        static PollingTimer _pollingTimer;

        /// <summary>
        ///  UN timer que espera chirping, en donde espera bytes con error de paridad, el cuál significa que recibe la dirección de la EGM
        ///  A timer waiting chirping, where waits bytes with parity error, which means that receives the EGM direction
        /// </summary>
        static PollingTimer _waitForChirpingTimer;


        /// <summary>
        /// Un array de bytes representando el último poll enviado
        /// A byte array representing the last sent poll
        /// </summary>
        public byte[] _lastSend;

        /// <summary>
        /// Un datetime representando el último momento en que se envió un long poll
        /// A datetime representing the last time stamp where a long poll is sent
        /// </summary>
        public DateTime _lastSendTS;

        /// <summary>
        /// Un array de bytes representando el último poll recibido
        /// A byte array representing the last received poll
        /// </summary>
        public byte[] _lastReceived;

        /// <summary>
        /// Un datetime representando el último momento en que se recibió un long poll
        /// A datetime representing the last moment where a long poll is received
        /// </summary>
        public DateTime _lastReceivedTS;

        public bool? communication = null;
        
        /// <summary>
        ///  Un evento que se lanza cuando se recibe un dato o información de la EGM o client
        /// An event that it is throwed when a data or information is received from EGM or client
        /// </summary>
        public event CommunicationEventHandler CommunicationEvent; public delegate void CommunicationEventHandler(bool communication, EventArgs e);
        public byte address = 0x01;
        private string egm_communication_enabled_msg = "EGM COMMUNICATION ENABLED";
        private string egm_communication_link_down = "EGM COMMUNICATION LINK DOWN";

        /// <summary>
        /// La fase de polleo. En este momento está en estado Stopped
        /// The polling phase. In this moment is on Stopped state
        /// </summary>
        public string phase = "Stopped";

        /// <summary>
        /// La frecuencia de polleo. Inicialmente está seteado en el valor siguiente
        /// The polling frequency. Initially it is set on the following value
        /// </summary>
        public int PollingFrecuency = 40;

        /// <summary>
        /// Una cola de long polls exclusivos (diferentes al 0x81)
        /// A queue of exclusive long polls (differents to 0x81)
        /// </summary>
        private ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();

        /// <summary>
        /// Asumimos el AssetNumber = 12345
        /// We asume the asset number = 12345
        /// </summary>
        public int _assetNumber = 12345;

         /// <summary>
        /// Asumimos el PoolID = 00 00
        /// We asume the PoolID = 00 00
        /// </summary>
        public byte[] PoolID = new byte[] {0x00, 0x00};

        /// <summary>
        /// monitor queue size
        /// </summary>
        /// <returns></returns>
        public int qsize()
        {
            return queue.Count;
        }


        /// <summary>
        /// Chequeo de CRC, toma los dos últimos bytes y lo compara con el crc del resto del array de bytes
        /// checking of CRC. Take the last two bytes and it compares with crc of the rest of byte array
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
            return true;

        }


        /// <summary>
        ///  Función que captura la indexación indebida. Si el índice supera el size del buffer, retorna null, si no retorna el buffer indexado en el índice
        ///  Function that captures the unexpected index. If index is greater than buffer size, it returns null, else returns the buffer in the index position
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
        ///  Analiza la respuesta y en base a su estructura, lanza ciertos eventos 
        ///  Analyze the response and, based on its structure, launchs certain events
        /// </summary>
        /// <param name="_respond"></param>
        private void Analyze(byte[] _respond)
        {
            SASResponseHandler.Singleton.Analyze(_respond);
            /* Si la respuesta no es un 00 ni un 1F */
            /* If the response is neither a 00 nor a 1F*/
            if (BitConverter.ToString(_respond) != "00" && BitConverter.ToString(_respond) != "1F")
            {
                if (_respond.Length > 1) // Si la respuesta tiene un tamaño mayor a 1 -- If the response has a length greater than 1
                {
                    // Divide el segundo elemento de la respuesta en casos
                    // Divide the second element of the response in cases
                    switch (_respond[1])
                    {
                        case 0x1C:
                            //generar el mensaje y levantar el evento
                            // generates the message and throws the event
                            string _longPollAnswer1 = LongPollInterpreter.Singleton.GetLP1C(_respond);
                            DataReceived(_longPollAnswer1, e);
                            break;
                        case 0x2F:
                            try
                            {
                                if (_respond[5] == 0x0C)
                                {
                                    byte[] credits = { _respond[6], _respond[7], _respond[8], _respond[9] };
                                    DataReceived($"Credits {int.Parse(BitConverter.ToString(credits).Replace("-", ""))}", e);
                                }
                            }
                            catch
                            {


                            }
                            break;
                        default:
                            break;
                    }
                }
                // Si la respuesta tiene un tamaño igual a 1
                // If the response has a length equals to 1
                if (_respond.Length == 1)
                {
                    // Divide el primer elemento de la respuesta en casos
                    // Divides the first element of the response in cases
                    switch (_respond[0])
                    {
                        case 0x00:
                            break;
                        case 0x01:
                            break;
                        default:
                            string excep = LongPollInterpreter.Singleton.GetException(_respond[0]);
                            if (excep != "")
                            {
                                DataReceived(excep, e);
                            }
                            else
                            {
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Lanza un proceso o thread para que corra en paralelo
        /// Launches a process or thread to be executed in parallel
        /// </summary>
        /// <param name="act"></param>
        private void spawn(ThreadStart act)
        {
            ThreadStart work = act;
            Thread thread = new Thread(work);
            thread.Start();

        }

        /// <summary>
        /// Process. Rutina que procesa la respuesta de la EGM
        /// Process. Routine that processes the response of EGM
        /// </summary>
        /// <param name="_respond"></param>
        private void Process(byte[] _respond)
        {
            // Castea a string la respuesta
            /// Cast to string the response
            string _longPollAnswer2 = BitConverter.ToString(_respond);
            // Si el answer es un mensaje  válido
            // If the answer is a valid message
            if (_longPollAnswer2 != "")
                // Lanza el evento CommandReceived con la respuesta
                // Throws the event CommandReceived with the response
                CommandReceived(string.Format("{0}", _longPollAnswer2), true, e); 
            Analyze(_respond);
        }

        /// <summary>
        /// Inicialización del host
        /// Initialization of host
        /// </summary>
        public SASHost()
        {
            // Si el sistema operativo en el cuál corre es Windows
            // If the operating system is Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                /* Configuro el puerto para que corra en windows, por defecto en COM1 */
                // Configure the port to be executed in windows, by default in COM1 */
                _serialPort = new SerialPort("windows");
                _serialPort.Port.SetPort("COM1");
            }
            else
            {
                /* Configuro el puerto para que corra en linux, por defecto en /dev/ttyS0 */
                /* Configure the port to be executed in linux, by default in /dev/ttyS0 */
                _serialPort = new SerialPort("linux");
                _serialPort.Port.SetPort("/dev/ttyS0");
            }
            // Abro el puerto
            // Opens the port
            _serialPort.Port.OpenPort();
            // Si el timer es null, lo configuro para que corra cada 5 segundos
            // If the timer is null, configures to be executed every 5 seconds
            if (_waitForChirpingTimer is null)
            {
                _waitForChirpingTimer = new PollingTimer(8000);
                _waitForChirpingTimer.Elapsed += _waitForChirpingTimer_Elapsed;
            }
            // _chirpingTimer.Start();

            GC.KeepAlive(_waitForChirpingTimer);
            //_sendNext = null;
            //t = new Task(Listen);

            Thread.Sleep(10);
        }


        /// <summary>
        /// Rutina que corre en tiempo de ejecución del host, para configurar el puerto
        /// Routine executed in runtime of host, to configure the serial port
        /// </summary>
        /// <param name="portName"></param>
        public void SetSerialPort(string portName)
        {
            // Detengo el timer de chirping, cierro el puerto
            // Stop the chirping timer, close the serial port
            // _chirpingTimer.Stop();
            _serialPort.Port.ClosePort();

            // Configuro el puerto
            // Configure the serial port
            _serialPort.Port.SetPort(portName);
            // Abro el puerto
            // Open the serial port
            _serialPort.Port.OpenPort();
            // _chirpingTimer.Start();
        }

        /// <summary>
        /// Rutina que  corre en tiempo de ejecución del host, para setear la frecuencia de polleo
        /// Routine that executes in runtime of host, to set the polling frequencie
        /// </summary>
        /// <param name="pf"></param>
        /// <returns></returns>
        public int setPFMiliseconds(int pf)
        {
            // Si el host está detenido
            // If the host is stopped
            if (phase == "Stopped")
            {
                // La frecuencia de polleo se ajustará entre 60 ms y 10000 ms
                // The polling frequency will be adjusted betwenn 60 ms and 10000 ms
                int pf_ = pf <= PollingFrecuency ? PollingFrecuency : (pf >= 10000 ? 10000 : pf);
                // Inicializo el timer de poleo
                // Initialize the polling timer
                // Si el timer de polleo aún no se inicializó...
                // If the polling timer is not initiliazed yet
                if (_pollingTimer is null)
                {
                    // Instancio un nuevo PollingTimer
                    // Instantiates a new Polling Timer
                    _pollingTimer = new PollingTimer(pf_);
                    _pollingTimer.Elapsed += _pollingTimer_Elapsed;
                }
                // Si se inicializó...
                // If it is initiated
                else
                    // Seteo el intervalo a la nueva frecuencia de polleo
                    // Set the interval to the new polling frequency
                    _pollingTimer.Interval = pf_;
                return pf_; // Retorno la frecuencia de polleo nueva -- Return the new polling frequency
            }
            else
            {
                return -1; // Retorno -1, en representación del host corriendo -- Return -1, representing the running host
            }
        }

        // 
        /// <summary>
        /// Configuro el AssetNumber
        /// Configure the Asset number
        /// </summary>
        /// <param name="assetNumber"></param>
        public void SetAssetNumber(int assetNumber)
        {
            if (assetNumber > 0)
            {
                _assetNumber = assetNumber;
            }
        }

        /// <summary>
        /// Configuro el PoolID
        /// Configure the pool id
        /// </summary>
        /// <param name="poolID"></param>
        public void SetPoolID(byte[] poolID)
        {
            PoolID = poolID;
        }


        /// <summary>
        /// Obtener la info del host
        /// Obtain the host info
        /// </summary>
        /// <returns></returns>
        public string GetHostInfo()
        {
            return string.Format(@"Using {0} / Asset Number: {1}",
                 _serialPort.Port.PortName(), _assetNumber);
        }


        // 
        /// <summary>
        /// Rutina de inicialización de polleo
        /// Routine of polling initialization
        /// </summary>
        public void StartPolling()
        {

            // Paso la fase de poleo a "Polling"
            // Transition to polling phase 'Polling'
            phase = "Polling";
            // Detengo el timer de chirp
            // Stop the chirping timer 
            // _chirpingTimer.Stop();
            // Si el long poll a mandar es nulo
            // If the long poll to send is null
            /*if (_sendNext is null)
            {
                // Obtengo el sync (80) para enviar
                _sendNext = LongPollFactory.Singleton.GetSync();
            }*/
            // Inicializo el timer de poleo
            // Initialize the polling timer
            if (_pollingTimer is null)
            {
                _pollingTimer = new PollingTimer(PollingFrecuency);
                _pollingTimer.Elapsed += _pollingTimer_Elapsed;
            }
            // DEPRECATED Empiezo a correr el timer de poleo
            // DEPRECATED Start the polling timer
            _pollingTimer.Start();
            GC.KeepAlive(_pollingTimer);
        }


        /// <summary>
        /// Rutina de detención del polleo
        /// Routine of polling stopping
        /// </summary>
        public void StopPolling()
        {
            // Paso la fase de polleo a "Stopped"
            // Transition to polling phase 'Stopped'
            phase = "Stopped";
            // Detengo el timer de poleo
            // Stop the polling timer
            _pollingTimer.Stop();
            // DEPRECATED Empiezo a correr el timer de chirping
            // DEPRECATED Start the chirping timer
            // _chirpingTimer.Start();

        }

        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************               HOST Functions          *********************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region "Host Functions"

        /// <summary>
        /// Get Address from hostGet Address from host
        /// </summary>
        /// <returns></returns>
        public byte GetAddress()
        {
            return address;
        }

        /// <summary>
        ///  Longpoll 01
        /// </summary>
        public void LockOutPlay()
        {
            queue.Enqueue(LongPollFactory.Singleton.LockOutPlay(address));
        }


        /// <summary>
        /// Longpoll 02
        /// </summary>
        public void EnablePlay()
        {
            queue.Enqueue(LongPollFactory.Singleton.EnablePlay(address));
        }

        /// <summary>
        /// Longpoll 03
        /// </summary>
        public void SoundOff()
        {
            queue.Enqueue(LongPollFactory.Singleton.SoundOff(address));
        }


        /// <summary>
        /// Longpoll 04
        /// </summary>
        public void SoundOn()
        {
            queue.Enqueue(LongPollFactory.Singleton.SoundOn(address));
        }


        /// <summary>
        /// Longpoll 06
        /// </summary>
        public void EnableBillValidator()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP06(address));
        }


        /// <summary>
        /// Longpoll 07
        /// </summary>
        public void DisableBillValidator()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP07(address));
        }


        /// <summary>
        /// Longpoll 08
        /// </summary>
        /// <param name="billDenominations"></param>
        /// <param name="billAcceptorActionFlag"></param>
        public void SendConfigureBillDenominationsLongPollCommand(Byte[] billDenominations,
                                                                Byte billAcceptorActionFlag)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildConfigureBillDenominationsLongPollCommand(address,
                                                                                                billDenominations,
                                                                                                billAcceptorActionFlag));
        }


        /// <summary>
        /// Longpoll 0A   
        /// </summary>
        public void EnterMaintenanceMode()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetEnterMaintenanceMode(address));
        }


        /// <summary>
        ///  Longpoll 0B 
        /// </summary>
        public void ExitMaintenanceMode()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetExitMaintenanceMode(address));
        }


        /// <summary>
        /// Longpoll 0E 
        /// </summary>
        /// <param name="enable_disable"></param>
        public void EnableDisableRealTimeEvent(byte enable_disable)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildEnableDisableRealTimeEvent(address, enable_disable));
        }


        /// <summary>
        /// Longpoll 1B
        /// </summary>
        public void SendHandPayInformation()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildHandPayInformationCommand(address));
        }


        /// <summary>
        /// Longpoll 1C" 
        /// </summary>
        /// <returns></returns>
        public string SendMeters()
        {
            string _answer = "";

            queue.Enqueue(LongPollFactory.Singleton.GetSendMeters(address));

            return _answer;
        }


        /// <summary>
        /// Longpoll 1E
        /// </summary>
        public void SendMultipleMeterLongPollGMLP1E()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP1E(address));
        }


        /// <summary>
        /// Longpoll 1F
        /// </summary>
        public void SendGamingMachineIDAndInformation()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetGamingMachineIDAndInformation(address));
        }


        /// <summary>
        ///  Longpoll 20
        /// </summary>
        public void SendTotalDollarValueOfBillsMeters()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP20(address));
        }


        /// <summary>
        ///  Longpoll 2A
        /// </summary>
        public void SendTrueCoinIn()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP2A(address));
        }

        /// <summary>
        /// Longpoll 2B
        /// </summary>
        public void SendTrueCoinOut()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP2B(address));
        }


        /// <summary>
        /// Longpoll 2D
        /// </summary>
        /// <param name="game_number"></param>
        public void SendTotalHandpaidCancelledCredits(byte[] game_number)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetLP2D(address, game_number));
        }


        /// <summary>
        /// Longpoll 2E
        /// </summary>
        /// <param name="bufferAmount"></param>
        public void SendLP2E(byte[] bufferAmount)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetLP2E(address, bufferAmount));
        }


        /// <summary>
        ///  Longpoll 2F
        /// </summary>
        public void SendCredits()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetSendSelectedMeters(address, new byte[] { 0x00, 0x00 }, new byte[] { 0x0C, 0x1B }));
        }

        /// <summary>
        /// Longpoll 2F
        /// </summary>
        /// <param name="codes"></param>
        public void SendSelectedMeter(byte[] codes)
        {
            if (codes.Count() <= 11)
            {
                queue.Enqueue(LongPollFactory.Singleton.GetSendSelectedMeters(address, new byte[] { 0x00, 0x00 }, codes));
            }
        }


        /// <summary>
        /// Longpoll 2F
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="codes"></param>
        public void SendSelectedMeter(byte[] gameNumber, byte[] codes)
        {
            if (codes.Count() <= 11)
            {
                queue.Enqueue(LongPollFactory.Singleton.GetSendSelectedMeters(address, gameNumber, codes));
            }
        }


        /// <summary>
        /// Longpoll 2F
        /// </summary>
        /// <param name="code"></param>
        public void SendSelectedMeter(byte code)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetSendSelectedMeters(address, new byte[] { 0x00, 0x00 }, new byte[] { code }));
        }

        /// <summary>
        /// Longpoll 48
        /// </summary>
        public void SendLastBillAcceptedInformation()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetSendLastBillAcceptedInformation(address));
        }

        /// <summary>
        /// Longpoll 21
        /// </summary>
        /// <param name="seedValue"></param>
        public void SendROMSignatureVerification(byte[] seedValue)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildROMSignatureVerification(address, seedValue));
        }

        /// <summary>
        /// Longpoll 4C
        /// </summary>
        /// <param name="machineID"></param>
        /// <param name="sequenceNumber"></param>
        public void SetSecureEnhancedValidationiID(byte[] machineID, byte[] sequenceNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildSetSecureEnhancedValidationID(address, machineID, sequenceNumber));
        }


        /// <summary>
        /// Longpoll 4D
        /// </summary>
        /// <param name="functionCode"></param>
        public void SendEnhancedValidationInformation(byte functionCode)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetSendEnhancedValidationInformation(address, functionCode));
        }


        /// <summary>
        /// Longpoll 50
        /// </summary>
        /// <param name="validationType"></param>
        public void SendValidationMeters(byte validationType)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildSendValidationMetersComand(address, validationType));
        }


        /// <summary>
        /// Longpoll 51
        /// </summary>
        public void SendNumberOfGamesImplemented()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetLP51(address));
        }


        /// <summary>
        /// Longpoll 53 
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendGameNConfiguration(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetLP53(address, gameNumber));
        }


        /// <summary>
        /// Longpoll 54
        /// </summary>
        public void SendSASVersionAndMachineSerialNumber()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetSASVersionAndMachineSerialNumber(address));
        }


        /// <summary>
        /// Longpoll 55
        /// </summary>
        public void SendSelectedGameNumber()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP55(address));
        }

        /// <summary>
        /// Longpoll 56
        /// </summary>
        public void SendEnabledGameNumbers()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP56(address));
        }

        /// <summary>
        ///  Longpoll 57
        /// </summary>
        public void SendPendingCashoutInformation()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetPendingCashoutInformation(address));
        }


        /// <summary>
        /// Longpoll 58
        /// </summary>
        /// <param name="validationSystemID"></param>
        /// <param name="validationNumber"></param>
        public void SendReceiveValidationNumber(byte validationSystemID, byte[] validationNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetReceiveValidationNumber(address, validationSystemID, validationNumber));
        }


        /// <summary>
        /// Longpoll 6F
        /// </summary>
        /// <param name="game_number"></param>
        /// <param name="meter_codes"></param>
        public void SendExtendedMeters(byte[] game_number, byte[] meter_codes)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetSendExtendMeters(address, game_number, meter_codes));
        }


        /// <summary>
        /// Longpoll 70
        /// </summary>
        public void SendTicketValidationData()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildSendTicketValidationDataCommand(address));
        }


        /// <summary>
        /// Longpoll 71
        /// </summary>
        /// <param name="transferCode"></param>
        /// <param name="transferAmount"></param>
        /// <param name="parsingCode"></param>
        /// <param name="validationData"></param>
        /// <param name="restrictedExpiration"></param>
        /// <param name="poolId"></param>
        public void SendtRedeemTicketCommand(Byte transferCode,
                                            uint transferAmount,
                                            Byte parsingCode,
                                            Byte[] validationData,
                                            DateTime restrictedExpiration,
                                            Byte[] poolId)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildRedeemTicketCommand(address,
                                                                            transferCode,
                                                                            transferAmount,
                                                                            parsingCode,
                                                                            validationData,
                                                                            restrictedExpiration,
                                                                            poolId));
        }


        /// <summary>
        /// Longpoll 71
        /// </summary>
        /// <param name="transferCode"></param>
        /// <param name="transferAmount"></param>
        /// <param name="parsingCode"></param>
        /// <param name="validationData"></param>
        public void SendtRedeemTicketCommand(Byte transferCode,
                                        uint transferAmount,
                                        Byte parsingCode,
                                        Byte[] validationData)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildRedeemTicketCommand(address,
                                                                            transferCode,
                                                                            transferAmount,
                                                                            parsingCode,
                                                                            validationData));
        }


        /// <summary>
        /// Longpoll 71
        /// </summary>
        /// <param name="transferCode"></param>
        public void SendtRedeemTicketCommand(Byte transferCode)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildRedeemTicketCommand(address,
                                                                            transferCode));
        }



        /// <summary>
        /// Longpoll 72
        /// </summary>
        public void AFTInit()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetAFTInt(address, 0x00));
        }

        /// <summary>
        /// Longpoll 72
        /// </summary>
        /// <param name="transaction_index"></param>
        public void AFTInit(Byte transaction_index)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetAFTInt(address, transaction_index));
        }


        /// <summary>
        /// Longpoll 72. ¡¡¡¡¡¡USED BY SASCONSOLE!!!!!!
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="cashableAmount"></param>
        public void AFTTransferFunds(byte transferType, long cashableAmount, byte[] TransactionID)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetAFTTransferFunds(address, _assetNumber, PoolID, 0x00, transferType, cashableAmount, TransactionID));
        }


        /// <summary>
        /// Longpoll 72
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="cashableAmount"></param>
        /// <param name="restrictedAmount"></param>
        /// <param name="nonrestrictedAmount"></param>
        public void AFTTransferFunds(byte transferType, long cashableAmount, long restrictedAmount, long nonrestrictedAmount, byte[] transactionID, byte[] registrationKey, byte[] Expiration)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetAFTTransferFunds(address, _assetNumber, PoolID, 0x00, transferType, cashableAmount, restrictedAmount, nonrestrictedAmount, transactionID, registrationKey, Expiration));
        }


        /// <summary>
        /// Longpoll 73. USED BY SASCONSOLE
        /// </summary>
        /// <param name="registrationCode"></param>
        public void AFTRegistration(byte registrationCode)
        {
            queue.Enqueue(LongPollFactory.Singleton.GetAFTRegistration(address, registrationCode, _assetNumber, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05,
                        0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20 }, 0));
        }

        /// <summary>
        /// Longpoll 73 parameters
        /// </summary>
        /// <param name="registrationCode"></param>
        public void AFTRegistration(byte registrationCode, byte[] registrationKey, int posID)
        {
            if (registrationKey != null)
            {
                if (registrationKey.Length == 0)
                {
                    queue.Enqueue(LongPollFactory.Singleton.GetAFTRegistration(address, registrationCode));
                }
                else 
                {
                    queue.Enqueue(LongPollFactory.Singleton.GetAFTRegistration(address, registrationCode, _assetNumber, registrationKey, posID));
                }
            }
            else
            {
               queue.Enqueue(LongPollFactory.Singleton.GetAFTRegistration(address, registrationCode));
            }
        }

        /// <summary>
        ///  Longpoll 74
        /// </summary>
        public void LockEGM()
        {
            queue.Enqueue(LongPollFactory.Singleton.GetLockLP74(address));
        }

        /// <summary>
        ///  Longpoll 74
        /// </summary>
        public void SendLP74(byte lockCode, byte transferCondition, byte[] lockTimeout)
        {
            if (lockTimeout.Count() >= 2)
                queue.Enqueue(LongPollFactory.Singleton.GetLockLP74(address, lockCode, transferCondition, lockTimeout[0], lockTimeout[1]));
        }
        
        /// <summary>
        /// Longpoll 7B
        /// </summary>
        /// <param name="control_mask1"></param>
        /// <param name="control_mask2"></param>
        /// <param name="status_bit_control_states1"></param>
        /// <param name="status_bit_control_states2"></param>
        /// <param name="cashable_ticket_and_receipt_expiration1"></param>
        /// <param name="cashable_ticket_and_receipt_expiration2"></param>
        /// <param name="restricted_ticket_default_expiration1"></param>
        /// <param name="restricted_ticket_default_expiration2"></param>
        public void ExtendedValidationStatus(Byte control_mask1, Byte control_mask2,
                                            Byte status_bit_control_states1, Byte status_bit_control_states2,
                                            Byte cashable_ticket_and_receipt_expiration1, Byte cashable_ticket_and_receipt_expiration2,
                                            Byte restricted_ticket_default_expiration1, Byte restricted_ticket_default_expiration2)
        {
            queue.Enqueue(LongPollFactory.Singleton.ExtendedValidationStatus(address, control_mask1, control_mask2, status_bit_control_states1, status_bit_control_states2, cashable_ticket_and_receipt_expiration1, cashable_ticket_and_receipt_expiration2, restricted_ticket_default_expiration1, restricted_ticket_default_expiration2));
        }


        /// <summary>
        /// Longpoll 7C
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        public void SetExtendedTicketData(byte code, string data)
        {
            List<Tuple<byte, string>> elements = new List<Tuple<byte, string>>();
            elements.Add(new Tuple<byte, string>(code, data));
            queue.Enqueue(LongPollFactory.Singleton.BuildSetExtendedTicketDataCommand(address, elements));
        }


        /// <summary>
        ///  Longpoll 7D
        /// </summary>
        /// <param name="hostID"></param>
        /// <param name="expiration"></param>
        /// <param name="location"></param>
        /// <param name="address1"></param>
        /// <param name="address2"></param>
        public void SetTicketData(byte[] hostID,
                                  byte expiration,
                                  byte[] location,
                                  byte[] address1,
                                  byte[] address2)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildSetTicketData(address,
                                                                       hostID,
                                                                       expiration,
                                                                       location,
                                                                       address1,
                                                                       address2));
        }



        /// <summary>
        ///  Longpoll 7D without data
        /// </summary>
        public void SetTicketData()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildSetTicketData(address));
        }


        /// <summary>
        ///  Longpoll 7E
        /// </summary>
        public void SendCurrentDateTimeGamingMachine()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP7E(address));
        }


        /// <summary>
        ///  Longpoll 7F
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        public void ReceiveDateAndTime(byte[] date, byte[] time)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildReceiveDateAndTimeCommand(address, date, time));
        }


        /// <summary>
        /// Longpoll 80
        /// </summary>
        /// <param name="broadcast"></param>
        /// <param name="group"></param>
        /// <param name="level"></param>
        /// <param name="amount"></param>
        public void SendLP80(bool broadcast,
                             byte group,
                             byte level,
                             byte[] amount)
        {
            // LONGPOLL BROADCAST, si está seteado en true el parámetro broadcast, envía la dirección 00, 
            // caso contrario envía la dirección propia
            byte _address = broadcast ? (byte)0x00 : (byte)address;
            queue.Enqueue(LongPollFactory.Singleton.BuildLP80(_address, group, level, amount));
        }

        /// <summary>
        ///  Long poll 83
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendLP83(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP83(address, gameNumber));
        }


        /// <summary>
        ///  Longpoll 84
        /// </summary>

        public void SendLP84()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP84(address));
        }


        /// <summary>
        /// Longpoll 85
        /// </summary>

        public void SendLP85()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP85(address));
        }


        /// <summary>
        /// Longpoll 86
        /// </summary>
        /// <param name="broadcast"></param>
        /// <param name="group"></param>
        /// <param name="AmountsAndLevels"></param>
        public void SendLP86(bool broadcast,
                             byte group,
                             List<Tuple<byte, byte[]>> AmountsAndLevels)
        {
            // LONGPOLL BROADCAST, si está seteado en true el parámetro broadcast, envía la dirección 00, 
            // caso contrario envía la dirección propia
            byte _address = broadcast ? (byte)0x00 : (byte)address;
            queue.Enqueue(LongPollFactory.Singleton.BuildLP86(_address, group, AmountsAndLevels));
        }


        /// <summary>
        ///  Longpoll 87
        /// </summary>
        public void SendLP87()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP87(address));
        }


        /// <summary>
        /// Longpoll 8C
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="time"></param>
        /// <param name="credits"></param>
        /// <param name="pulses"></param>
        public void SendLP8C(byte[] gameNumber, byte[] time, byte[] credits, byte pulses)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP8C(address, gameNumber, time, credits, pulses));
        }

        /// <summary>
        /// Longpoll 8A
        /// </summary>
        /// <param name="bonusAmount"></param>
        /// <param name="taxStatus"></param>
        public void InitiateLegacyBonus(byte[] bonusAmount, byte taxStatus)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP8A(0x01, bonusAmount, taxStatus));
        }


        /// <summary>
        /// Longpoll 94
        /// </summary>
        public void ResetHandpay()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildResetHandpayCommand(address));
        }


        /// <summary>
        /// Longpoll 95
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendTournamentGamesPlayed(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP95(address, gameNumber));
        }


        /// <summary>
        /// Longpoll 96
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendTournamentGamesWon(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP96(address, gameNumber));
        }

        /// <summary>
        /// Longpoll 97
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendTournamentCreditsWagered(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP97(address, gameNumber));
        }


        /// <summary>
        ///  Longpoll 98
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendTournamentCreditsWon(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP98(address, gameNumber));
        }


        /// <summary>
        /// Longpoll 99
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendTournamentMeters(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP99(address, gameNumber));
        }

        /// <summary>
        /// Longpoll 9A
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendLP9A(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLP9A(address, gameNumber));
        }

        /// <summary>
        /// Longpoll A0
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendEnabledFeatures(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildSendEnabledFeaturesCommand(address, gameNumber));
        }


        /// <summary>
        /// Longpoll A4
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SendCashoutLimit(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPA4(address, gameNumber));
        }


        /// <summary>
        /// Longpoll A8
        /// </summary>
        /// <param name="resetMethod"></param>
        public void EnableJackpotHandpayResetMethod(byte resetMethod)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPA8(address, resetMethod));
        }


        /// <summary>
        /// Longpoll B1
        /// </summary>
        public void SendCurrentPlayerDenomination()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPB1(address));
        }

        /// <summary>
        /// Longpoll B2
        /// </summary>
        public void SendEnabledPlayerDenominations()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPB2(address));
        }


        /// <summary>
        ///  Longpoll B3
        /// </summary>
        public void SendTokenDenomination()
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPB3(address));
        }

        /// <summary>
        ///  Longpoll B4
        /// </summary>
        public void SendWagerCategoryInformation(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPB4(address, gameNumber));
        }



        /// <summary>
        /// Longpoll B5
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SentExtendedGameNInformation(byte[] gameNumber)
        {
            queue.Enqueue(LongPollFactory.Singleton.BuildLPB5(address, gameNumber));
        }

        /// <summary>
        ///  Longpoll generic
        /// </summary>
        /// <param name="lp"></param>
        public void SendLongPoll(byte[] lp)
        {
            queue.Enqueue(lp);
        }

        /// <summary>
        /// Función que determina si el code de determinado long poll existe en la cola de long polls
        /// Function that determines if certain long poll code exists on long poll queue
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool LongPollCodeExistsInQueue(byte code)
        {
            return queue.Where(b => GetByteFromArrayIndex(b, 1) == code).Count() > 0;
        }

        #endregion



        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************              EXCEPTIONS PARSER          *******************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region "Exceptions Parser"

        /// <summary>
        /// Chequeo de excepciones
        /// Exception checking
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool IsException(byte? b)
        {
            if (b == null)
                return false;
            if ((b >= 0x11 && b <= 0x1E) ||
                (b >= 0x20 && b <= 0x25) ||
                (b >= 0x27 && b <= 0x2E) ||
                (b >= 0x31 && b <= 0x57) ||
                (b >= 0x60 && b <= 0x61) ||
                (b >= 0x66 && b <= 0x72) ||
                (b >= 0x74 && b <= 0x7C) ||
                (b >= 0x7E && b <= 0x8C) ||
                (b >= 0x8E && b <= 0x8F) ||
                (b >= 0x98 && b <= 0x9B))
                return true;
            return false;
        }


        /// <summary>
        /// Chequeo de excepciones prioritarias
        /// Checking of priority exceptions
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool IsPriorityException(byte? b)
        {
            if (b == null)
                return false;
            if ((b >= 0x3D && b <= 0x3F) ||
                (b >= 0x51 && b <= 0x52) ||
                (b >= 0x55 && b <= 0x57) ||
                (b >= 0x67 && b <= 0x6D) ||
                (b >= 0x6F && b <= 0x70) ||
                (b == 0x8F))
                return true;
            return false;
        }

        /// <summary>
        /// Get Exception
        /// </summary>
        /// <param name="commandBytes"></param>
        /// <returns></returns>
        private byte? GetException(byte[] commandBytes)
        {
            // Si el length es 1, se devuelve el mismo mensaje. La EGM no tiene habilitado el real-time
            // If the length is equals to 1, returns the same message. The EGM doesn't have the realtime enabled
            if (commandBytes.Length == 1)
            {
                SASResponseHandler.Singleton.SendRealTimeEvent(0x00);
                return commandBytes.FirstOrDefault();
            }
            // Si el length es mayor estricto que 3, 
            // nos fijamos que el primer byte sea distinto a FF
            // y que el segundo byte sea FF, refiriendonse al comando FF para excepciones.
            // Esto quiere decir que la EGM tiene habilitado el real-time
            // Y devolvemos el tercer byte
            
            /* If the length is greater than 3, we check that the first byte is distinct to FF and the second byte is FF
              This means that EGM has the real time enabled
              And return the third byte */
            else if (commandBytes.Length > 3)
            {
                if (commandBytes[0] != 0xFF)
                {
                    if (commandBytes[1] == 0xFF)
                    {
                        SASResponseHandler.Singleton.SendRealTimeEvent(0x01);
                        return commandBytes[2];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get Exception Data
        /// </summary>
        /// <param name="commandBytes"></param>
        /// <returns></returns>
        private byte[] GetExceptionData(byte[] commandBytes)
        {
            // Si el length es 1, no se devuelve nada. La EGM no tiene habilitado el real-time
            // If the length is equals to 1, returns nothing. The EGM doesn't have the real time enabled
            if (commandBytes.Length == 1)
            {
                return new byte[] { };
            }
            // Si el length es mayor estricto que 3, 
            // nos fijamos que el primer byte sea distinto a FF
            // y que el segundo byte sea FF, refiriendonse al comando FF para excepciones.
            // Esto quiere decir que la EGM tiene habilitado el real-time
            // Y devolvemos el array de bytes sin tener en cuenta los primeros 3 bytes ni los últimos 2

            /* If the length is greater than 3,
               check that the first byte is distinct to FF and the second byte is FF 
               This means that EGM has the real time enabled
               And return the byte array without neither the first 3 bytes nor the last 2 bytes*/
            else if (commandBytes.Length > 3)
            {
                if (commandBytes[0] != 0xFF)
                {
                    if (commandBytes[1] == 0xFF)
                    {
                        byte[] data = commandBytes.Skip(3).ToArray();
                        data = data.Reverse().ToArray().Skip(2).Reverse().ToArray();
                        return data;
                    }
                }
            }
            return null;
        }

        #endregion

        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************     Long poll Sending        ******************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region "Envios de long pollss"

        /// <summary>
        /// Función que determina que cierta respuesta corresponde a un long poll
        /// Function that determines that certain response corresponds to a long poll
        /// </summary>
        /// <param name="response"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool is_response_from(byte[] response, byte[] command)
        {
            if (command.Length > 1 && response.Length > 1)
                if (response[1] == command[1])
                    return true;
                else
                    return false;
            return true;
        }

        /// <summary>
        /// Función que devuelve una respuesta al comando que se envía. Intenta en tryouts veces para encontrar una respuesta válida. Una vez agotados los intentos, retorna "FF-FF"
        /// Function that returns a response to the command. Try in 'tryouts' times to find a valid response. Once depleted the tries, returns 'FF-FF'
        /// </summary>
        /// <param name="commandBytes"></param>
        /// <param name="tryouts"></param>
        /// <param name="isRetry"></param>
        /// <returns></returns>
        private byte[] sendPoll_(byte[] commandBytes, int tryouts, bool isRetry)
        {
            _serialPort.Port.FlushPort();
            _serialPort.Port.SendWithWakeup(commandBytes);
            CommandSent(BitConverter.ToString(commandBytes), CheckCRC(commandBytes), isRetry, e);

            if (!commandBytes.SequenceEqual(new byte[] { 0x80 }))
            {
                byte[] _respond = _serialPort.Port.ReceiveMessage();
                if(commandBytes.Length > 1 && commandBytes[1] == 0x21)
                {
                    if (_respond.Length != 0)
                    {
                        Console.WriteLine($"LP21 sendPoll_ response: {BitConverter.ToString(_respond)}, length: {_respond.Length}");
                        if (_respond.SequenceEqual(new byte[] { address }))
                        {
                            Console.WriteLine($"LP21 ACK received");
                        }
                    }
                }
                
                // NO ANSWER
                if (_respond.Length == 0)
                {
                    if (tryouts > 0)
                    {
                        // BROADCAST
                        if (commandBytes[0] == 0x00)
                        {
                            return new byte[] { 0xff, 0xff };
                        }
                        // NO BROADCAST
                        else
                        {
                            return sendPoll_(commandBytes, tryouts - 1, true);
                        }
                    }
                    else
                    {
                        if (communication != false)
                        {
                           WaitForChirping();
                        }
                        CommandReceived($"Retry exhausted for long poll {BitConverter.ToString(commandBytes)}", false, e);
                        // Console.WriteLine($"Retry exhausted for long poll {BitConverter.ToString(commandBytes)}");
                    }
                }
                // ANSWER
                else
                {
                    StopWaitForChirping();
                    byte? exception = GetException(_respond);
                    // IS EXCEPTION
                    if (IsException(exception))
                    {
                        if (IsPriorityException(exception))
                        {
                            return _respond;
                        }
                        else
                        {
                                // WHAT WE'VE SENT IS GENERAL POLL
                                if (commandBytes.SequenceEqual(new byte[] { (byte)(0x80 | address)  }))
                                {
                                    byte[] exceptionData = GetExceptionData(_respond);
                                    SASResponseHandler.Singleton.LaunchException(exception.Value, exceptionData);
                                    return _respond;
                                }
                                // WHAT WE'VE SENT IS LONG POLL
                                else
                                {
                                     // Exception is really NACK?
                                    if (exception == (byte)(0x80 | address) )
                                    {
                                        // if tryouts left
                                        if (tryouts > 0) 
                                        {
                                            // resend poll
                                            return sendPoll_(commandBytes, tryouts-1, true);
                                        }
                                        else
                                        {
                                            // Retry exhausted
                                        }
                                    }
                                    // Exception is not NACK
                                    else
                                    {
                                        byte[] exceptionData = GetExceptionData(_respond);
                                        SASResponseHandler.Singleton.LaunchException(exception.Value, exceptionData); //Lanzo la exception data
                                        Process(_respond); // Proceso la exception
                                        SendLongPoll(commandBytes); // Vuelvo a intentar mandar el comando que no me respondieron la exception
                                    }
                              }
                        }
                    }
                    // IS A RESPONSE
                    else
                    {
                        // IS GENERAL POLL
                        if (commandBytes.SequenceEqual(new byte[] { (byte)(0x80 | address) }))
                        {
                            if(_respond.Length > 1 && _respond[1] == 0x21)
                            {
                                // ALEX: I think 0x00 is the "no response data" response to a general poll
                                Console.WriteLine($"General Poll response for LP21: {BitConverter.ToString(_respond)}");
                                // Return the async LP21 response to get sent back to Host/Smib
                                return _respond;
                            }

                            byte? resp001F = GetException(_respond);
                            if (resp001F == 0x00 || resp001F == 0x1F)
                            {
                                return _respond;
                            }
                            else
                            {
                                //smlog=smlog+"->"+BitConverter.ToString(_respond); 
                                //return sendPoll_(commandBytes, tryouts); //ELIMINO ESTA TRANSICION
                            }
                        }
                        // IS LONG POLL
                        else
                        {
                            // THE MACHINE RESPONDS WITH ACK
                            if (_respond.SequenceEqual(new byte[] { address }))
                            {
                                return _respond;
                            }
                            else
                            {
                                // THE MACHINE RESPONDS WITH NACK
                                if (_respond.SequenceEqual(new byte[] { (byte)(0x80 | address) }))
                                {
                                    if (tryouts > 0)
                                    {
                                        return sendPoll_(commandBytes, tryouts-1, true);
                                    }
                                    else
                                    {
                                        // Retry exhausted
                                    }
                                }
                                else
                                {
                                    bool tryoutON = false;
                                    // CRC OK
                                    if (is_response_from(_respond, commandBytes) && CheckCRC(_respond))
                                    {
                                        return _respond;
                                    }
                                    else if (is_response_from(_respond, commandBytes) &&!CheckCRC(_respond))
                                    {

                                        byte[] _respond1 = _serialPort.Port.ReceiveMessage();

                                        _respond = _respond.Concat(_respond1).ToArray();
                                        if (CheckCRC(_respond))
                                        {
                                            return _respond;
                                        }
                                        else
                                        {
                                            tryoutON = true;
                                        }
                                    }
                                    else
                                    {
                                        tryoutON = true;
                                    }
                                    if (tryoutON)
                                    {
                                        if (tryouts > 0)
                                        {
                                            return sendPoll_(commandBytes, tryouts - 1, true);
                                        }
                                        else
                                        {
                                            CommandReceived(BitConverter.ToString(_respond), false, e);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///  Función que devuelve una respuesta al comando que se envía. Devuelve el mismo comando por razones de implementación.
        ///  Function that returns a response to the command. Returns the same command by implementation reasons.  No delay needed if sending specific bytes to machine as a command, long poll, etc.
        /// </summary>
        /// <param name="commandBytes"></param>
        /// <returns></returns>
        private byte[] sendPoll(byte[] commandBytes) // 
        {
            _pollingTimer.Stop();

            byte[] answer = sendPoll_(commandBytes, 2, false);

            _lastReceived = answer;
            _lastReceivedTS = DateTime.Now;

            if (answer != null)
            {           
                Process(answer);        
            }
            _pollingTimer.Start();
            return commandBytes;
        }


        /// <summary>
        /// Una vez que se cumplió el tiempo para enviar el poll, se ejecuta esta rutina. Pasa a la fase Polling
        /// Once the time is elapsed to send the long poll, this routine is execute. Transitions to phase 'Polling'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _pollingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (phase == "Polling")

            {
                //did I sent 80?
                if (StructuralComparisons.StructuralEqualityComparer.Equals(_lastSend, LongPollFactory.Singleton.GetSync()))
                {
                    // If yes,
                    // I check if there is a long poll in the queue
                    if (queue.Count() > 0)
                    {
                        // Send the long poll of the queue
                        byte[] lp = null;
                        if (queue.TryDequeue(out lp))
                        {
                            if(lp.Length > 1 && lp[1] == 0x21) 
                            {
                                Console.WriteLine($"_pollingTimer_Elapsed sending lp {BitConverter.ToString(lp)}");
                            }
                            
                            _lastSend = sendPoll(lp);
                            _lastSendTS = DateTime.Now;

                        }
                    }
                    else
                    {
                        // Send the general poll 81
                        _lastSend = sendPoll(LongPollFactory.Singleton.GetGeneralPoll(address));
                        _lastSendTS = DateTime.Now;

                    }
                }
                else
                {
                    // If not, i send the sync byte 80
                    _lastSend = sendPoll(LongPollFactory.Singleton.GetSync());
                    _lastSendTS = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Método para esperar el primer chirping. Lanza un timer de 8 segundos
        /// Method to wait the first chirping. Launches a timer of 8 seconds
        /// </summary>
        private void WaitForChirping()
        {
            if (_waitForChirpingTimer.Enabled == false)
            {
                _waitForChirpingTimer.Interval = 1500;
                _waitForChirpingTimer.Start();
            }
        }

        /// <summary>
        /// Método que detiene la espera del chirping
        /// Method to stop waiting for chirp
        /// </summary>
        private void StopWaitForChirping()
        {
            if (communication != true)
            {
                Console.WriteLine($"[{DateTime.Now}] {egm_communication_enabled_msg}");
                communication = true;
                CommunicationEvent(communication.Value, e);

            }
            _waitForChirpingTimer.Stop();
        }

        /// <summary>
        /// Función que determina que un array no está vacío
        /// Function that determines if an array is empty 
        /// </summary>
        /// <param name="byteArry"></param>
        /// <returns></returns>
        private bool IsNotEmpty(byte[] byteArry)
        {
            if (byteArry != null)
            {
                if (byteArry.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Método que se ejecuta cuando el timer de espera de chirping cumple los 8 segundos de frecuencia. Lee el puerto en búsqueda del chirpeo para determinar que hay comunicación
        /// Method that is executed when the chirping wait timer reaches 8 seconds of frequency. Read the port in search of the chirp to determine that there is communication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e1"></param>
        private void _waitForChirpingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e1)
        {
            _waitForChirpingTimer.Stop();
            byte[] _respond = _serialPort.Port.ReceiveWakeupMessage();
            if (IsNotEmpty(_respond))
            {
                if (_respond.SequenceEqual(new byte[] { address }))
                {
                    if (communication != true)
                    {
                        Console.WriteLine($"[{DateTime.Now}] {egm_communication_enabled_msg}");
                        communication = true;
                        CommunicationEvent(communication.Value, e);

                    }
                }
            }
            else
            {
                if (communication != false)
                {
                    Console.WriteLine($"[{DateTime.Now}] {egm_communication_link_down}");
                    communication = false;
                    CommunicationEvent(communication.Value, e);
                }
                _waitForChirpingTimer.Interval = 1500;
                _waitForChirpingTimer.Start();
            }

        }
        #endregion

    }
}
