using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;
using System.Linq;

namespace MainController
{
    /* Interfaz del Logger */
    /* Logger Interface */
    public abstract class LoggerInterface
    {
        // Obtener el PortName // Obtain the PortName 
        public abstract void AddTrace(string message, string type, bool crc, bool isRetry);
        // Setear el puerto // Set the port 
        public abstract void Init();
        // Guardar los logs y devolverlos // Save logs and return them 
        public abstract void GetTrace(ref object trace);
        
    }
}
