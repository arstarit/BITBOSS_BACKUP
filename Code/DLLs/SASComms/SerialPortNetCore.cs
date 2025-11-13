using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SASComms;

namespace SASComms
{

    /// <summary>
    /// La clase del puerto para Net Core (Windows)
    /// The serial port class for net core (Windows) 
    /// </summary>
    public class SerialPortNetCore : SerialPortInterface
    {

        static System.IO.Ports.SerialPort _serialPort;
        /// <summary>
        /// Setear el puerto
        /// Set the port
        /// </summary>
        /// <param name="port"></param>
        public override void SetPort(string port)
        {
            _serialPort = new System.IO.Ports.SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = port;
            _serialPort.BaudRate = 19200;
            _serialPort.ParityReplace = (byte)'\0';
            _serialPort.ReadBufferSize = 128;
            _serialPort.WriteBufferSize = 128;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.Two;
            _serialPort.Handshake = Handshake.None;
            // Set the read/write timeouts
            _serialPort.ReadTimeout = 50;
            _serialPort.WriteTimeout = 50;
            //Console.WriteLine($"Using port {port}");
        }

        /// <summary>
        ///  Obtener el PortName
        /// Obtain the port name
        /// </summary>
        /// <returns></returns>
        public override string PortName()
        {
            return _serialPort.PortName;
        }

        /// <summary>
        ///  Abrir el puerto
        /// Open the port
        /// </summary>
        public override void OpenPort()
        {
            try
            {
                _serialPort.Open();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Cerrar el puerto
        /// Close the port
        /// </summary>
        public override void ClosePort()
        {
            _serialPort.Close();
        }

        /// <summary>
        ///  Flushear el puerto
        ///  Flush the port
        /// </summary>
        public override void FlushPort()
        {
            _serialPort.DiscardInBuffer();
        }

        /// <summary>
        /// Enviar un mensaje con el bit wakeup en 1
        /// Send a message with the wakeup bit in 1
        /// </summary>
        /// <param name="buffer"></param>
        public override void SendWithWakeup(byte[] buffer)
        {

            _serialPort.Parity = Parity.Mark;
            _serialPort.Write(buffer, 0, 1);

            if (buffer.Length > 1)
            {
                Thread.Sleep(1);
                _serialPort.Parity = Parity.Space;
                _serialPort.Write(buffer, 1, buffer.Length - 1);
            }
        }

        /// <summary>
        ///  Chequeo de CRC, toma los dos últimos bytes y lo compara con el crc del resto del array de bytes
        ///  CRC check, takes the last two bytes and compares it to the crc of the rest of the byte array
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
        /// Función que recibe un mensaje del puerto y lo concatena al final de la vieja respuesta que viene de parámetro 
        /// Function that receives a message from the port and concatenates it to the end of the old response that comes from parameter
        /// </summary>
        /// <param name="_respondOld"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte[] _ReceiveMessageConcat_(byte[] _respondOld, int i)
        {
            byte[] _respond = { };
            try
            {
                Thread.Sleep(100);
                if (_serialPort.IsOpen == true)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        _respond = new byte[_serialPort.BytesToRead];
                        _serialPort.Read(_respond, 0, _respond.Length);
                    }
                }
            }
            catch
            {

            }
            _respond = _respondOld.ToList().Concat(_respond.ToList()).ToArray();
            if (CheckCRC(_respond))
                return _respond;
            else if (i > 0)
            {
                byte[] _respond1 = _ReceiveMessageConcat_(_respond, i - 1);
                return _respond1;
            }
            return _respond;
        }

        /// <summary>
        ///  Recibir un mensaje
        /// Receive a message
        /// </summary>
        /// <returns></returns>
        public override byte[] ReceiveMessage()
        {
            byte[] _respond = { };
            try
            {
                if (_serialPort.IsOpen == true)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        _respond = new byte[_serialPort.BytesToRead];
                        _serialPort.Read(_respond, 0, _respond.Length);
                    }
                }
            }
            catch
            {

            }
            if (CheckCRC(_respond))
                return _respond;
            else
            {
                byte[] _respond1 = _ReceiveMessageConcat_(_respond, 5);
                return _respond1;
            }
        }

        /// <summary>
        ///  Recibir un mensaje con el bit de paridad en 1
        ///  Receive a message with the parity bit set to 1
        /// </summary>
        /// <returns></returns>
        public override byte[] ReceiveWakeupMessage()
        {
            byte[] _respond = { };
            try
            {
                Thread.Sleep(100);
                if (_serialPort.IsOpen == true)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        _respond = new byte[_serialPort.BytesToRead];
                        _serialPort.Read(_respond, 0, _respond.Length);
                    }
                }
            }
            catch
            {

            }
            return _respond;
        }
    }
}
