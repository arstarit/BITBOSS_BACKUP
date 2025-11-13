using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using SASComms;
using System.Collections;
using System.Linq;


namespace SASComms
{

    /// <summary>
    /// La clase del puerto para Linux
    /// The serial port class for Linux
    /// </summary>
    public class SerialPortLinux : SerialPortInterface
    {
        [DllImport("SerialPortController", EntryPoint = "OpenPort")]
        public static extern int OpenPort(string portName);

        [DllImport("SerialPortController", EntryPoint = "ClosePort")]
        public static extern void ClosePort(int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "readBytes")]
        public static extern int readBytes(byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "readSpaceBytes")]
        public static extern int readSpaceBytes(byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "writeBytes")]
        public static extern void writeBytes(byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "writeBytesWithWakeup")]
        public static extern void writeBytesWithWakeup(byte[] fb, byte[] sb, int maxsize, int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "set_wakeup")]
        public static extern int set_wakeup(int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "set_space")]
        public static extern int set_space(int FileDescriptor);

        [DllImport("SerialPortController", EntryPoint = "flushPort")]
        public static extern void flushPort(int FileDescriptor);


        static int SerialPortFileDescriptor;


        // Reads from the SerialPortFileDescriptor with the length of the buffer
        private int readPort(byte[] buffer)
        {
            return readSpaceBytes(buffer, buffer.Length, SerialPortFileDescriptor); // Reading from SerialPortFileDescriptor with buffer length
        }

        // Reads from the SerialPortFileDescriptor with the length of the buffer for a wake-up
        private int readWakeupPort(byte[] buffer)
        {
            return readBytes(buffer, buffer.Length, SerialPortFileDescriptor); // Reading from SerialPortFileDescriptor for wake-up with buffer length
        }

        // Writes to the SerialPortFileDescriptor using the provided buffer
        private void writePort(byte[] buffer)
        {
            writeBytes(buffer, buffer.Length, SerialPortFileDescriptor); // Writing to SerialPortFileDescriptor using buffer with its length
        }


        private static string port_ = "";
        /// <summary>
        /// Setear el puerto
        /// Set the serial port
        /// </summary>
        /// <param name="port"></param>
        public override void SetPort(string port)
        {
            port_ = port;
        }
        /// <summary>
        /// Obtener el PortName
        /// Obtain the port name
        /// </summary>
        /// <returns></returns>
        public override string PortName()
        {
            return port_;
        }
        /// <summary>
        /// Abrir el puerto
        /// Open the port
        /// </summary>
        public override void OpenPort()
        {
            SerialPortFileDescriptor = OpenPort(port_);
        }
        /// <summary>
        /// Cerrar el puerto
        /// CLose the port
        /// </summary>
        public override void ClosePort()
        {
            ClosePort(SerialPortFileDescriptor);
        }
        /// <summary>
        ///  Flushear el puerto
        /// Flush the port
        /// </summary>
        public override void FlushPort()
        {
		flushPort(SerialPortFileDescriptor);
        }

        /// <summary>
        ///  Enviar un mensaje con el bit wakeup en 1
        /// Send the message with the wakeup bit in 1
        /// </summary>
        /// <param name="buffer"></param>
        public override void SendWithWakeup(byte[] buffer)
        {

            byte[] cmd = buffer;
            byte[] byteWithWakeup = { cmd[0] };
            byte[] bytesWithSpace = cmd.Skip(1).ToArray();
            writeBytesWithWakeup(byteWithWakeup, bytesWithSpace, buffer.Length, SerialPortFileDescriptor);

        }
        /// <summary>
        /// Recibir un mensaje
        /// Receive a message
        /// </summary>
        /// <returns></returns>
        public override byte[] ReceiveMessage()
        {
            byte[] _respond = new byte[128];
            int howmuch = readPort(_respond) + 1;
            _respond = _respond.ToList().Take(howmuch).ToArray();
            return _respond;
        }
        /// <summary>
        /// Recibir un mensaje con el bit de paridad en 1
        /// Receive a message with the parity bit in 1
        /// </summary>
        /// <returns></returns>
        public override byte[] ReceiveWakeupMessage()
        {
            byte[] _respond = new byte[128];
            int howmuch = readWakeupPort(_respond) + 1;
            _respond = _respond.ToList().Take(howmuch).ToArray();
            return _respond;
        }

    }
}
