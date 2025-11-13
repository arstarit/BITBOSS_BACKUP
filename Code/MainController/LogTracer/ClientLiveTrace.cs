using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace MainController
{
    // LiveTrace type: A list of lines // 
    using LiveTrace = System.Collections.Generic.List<MainController.LiveTraceLine>;

    public class LiveTraceLine // La clase o estructura de una line // The type or structure of a line 
    {
        public DateTime TimeStamp; // Fecha // Date
        public string Message; // Bytes 
        public string Type; // El type // The type
        public bool CRC; // El booleano CRC // The Boolean CRC 
        public bool IsRetry; // El booleano EsRetry // The Boolean EsRetry 
    }
    
    public class ClientLiveTraceController : LoggerInterface// El controlador // The controller
    {
        private static LiveTrace livetrace = new LiveTrace();

        public override void GetTrace(ref object trace) // se obtiene el sastrace // sastrace is obtained 
        {
            trace = livetrace;
        }
        public override void AddTrace(string message, string type, bool crc, bool isRetry) // Se añade una line nueva // A new line is added
        {
            LiveTraceLine l = new LiveTraceLine();
            l.TimeStamp = DateTime.Now;
            l.Message = message; // Los bytes / The bytes
            l.Type = type; // El type // The type
            l.CRC = crc; // El crc // The crc 
            l.IsRetry = isRetry; // Es un retry // It's a retry
            livetrace.Add(l);
        }
        public override void Init() // Inicialización // Initialization
        {
                livetrace = new LiveTrace();
        }
    }
}
