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
    /// Interfaz del SerialPort, la clase controladora del puerto serial
    /// SerialPort interface, the controller class of serial port
    /// </summary>
    public abstract class SerialPortInterface
    {
        /// <summary>
        /// Obtener el PortName
        /// Obtain the port name
        /// </summary>
        /// <returns></returns>
        public abstract string PortName();
        /// <summary>
        ///  Setear el puerto
        /// Set the port
        /// </summary>
        /// <param name="port"></param>
        public abstract void SetPort(string port);
        /// <summary>
        /// Abrir el puerto
        /// Open the port
        /// </summary>
        public abstract void OpenPort();
        /// <summary>
        /// Cerrar el puerto
        /// Close the port
        /// </summary>
        public abstract void ClosePort();
        /// <summary>
        /// Enviar un mensaje con el bit wakeup en 1
        /// Send a message with the wakeup bit in 1
        /// </summary>
        /// <param name="buffer"></param>
        public abstract void SendWithWakeup(byte[] buffer);
        /// <summary>
        /// Recibir un mensaje
        /// Receive a message
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ReceiveMessage();
        /// <summary>
        /// Recibir un mensaje con el bit de paridad en 1
        /// Receive a message with the parity bit in 1
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ReceiveWakeupMessage();
        /// <summary>
        /// Flushear el puerto
        /// Flush the serial port
        /// </summary>
        public abstract void FlushPort();


    }
}
