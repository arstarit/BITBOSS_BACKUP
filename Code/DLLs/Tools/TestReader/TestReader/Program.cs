using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;

namespace TestReader
{
    class Program
    {


        static SerialPort _serialPort;
        private static System.Timers.Timer aTimer;

        private static void DataReceivedHandler(
                            object sender,
                            SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            byte[] _response;
            SerialPort sp = (SerialPort)sender;
            if (sp.BytesToRead > 0)
            {
                _response = new byte[sp.BytesToRead];
                sp.Read(_response, 0, _response.Length);
                Console.WriteLine("Received: " + BitConverter.ToString(_response));
            }
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            byte[] message = new byte[] { 0x81, 0x00 };
            _serialPort.Write(message, 0, message.Length);
            Console.WriteLine("Sent: " + BitConverter.ToString(message));

        }

        public static void Main(string[] args)
        {
            string port = "/dev/ttyS0";
            // Opción de EnableSASTrace
            if (args.Length >= 1)
                port = args[0];
            else
            {
                Console.WriteLine("You must set a port, usage: ./GetReaderStatus [Port]");
                return;
            }
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
            _serialPort.Open();
            aTimer.Start();
            Console.ReadKey();
            aTimer.Stop();
            _serialPort.Close();
        }

    }
}
