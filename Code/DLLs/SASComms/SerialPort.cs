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
    /// La clase SerialPort
    /// The serialport class
    /// </summary>
    public class SerialPort
    {
        /// <summary>
        /// El puerto a inicializar mediante el método de instanciación
        /// The port to initialize using the instantiation method
        /// </summary>
        public SerialPortInterface Port;

        /// <summary>
        ///  Instanciación: Toma un sistema operativo en formato string y en base a este parámetro instancia el puerto en ese sistema operativo
        /// Instantiation: Take an operating system in string format and based on this parameter, creates the serial port in that operating system
        /// </summary>
        /// <param name="os"></param>
        public SerialPort(string os)
        {
            switch (os)
            {
                case "linux":
                    {
                        Port = new SerialPortLinux();
                        break;
                    }
                case "windows":
                    {
                        Port = new SerialPortNetCore();
                        break;
                    }
                default:
                    break;
            }
        }

    }
}
