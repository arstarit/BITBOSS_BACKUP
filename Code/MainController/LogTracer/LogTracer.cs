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

    // Definición del LogTracer en base al LoggerInterface
    // LogTracer definition based on LoggerInterface
    public class LogTracer {

        // El LoggerInterface
        // The LoggerInterface
        public LoggerInterface Logger;

        public LogTracer(string mode)
        {
            // De acuerdo al modo de dejar logs
            // According to the log mode
            switch (mode){
                case "host-api": // Por api // By Api
                {
                    Logger = new HostLiveTraceController();
                    break;
                }
                case "client-api": // Por api // By Api
                    {
                    Logger = new ClientLiveTraceController();
                    break;
                }
                case "host-elastic-search": // Por Elastic Search // For Elastic Search 
                    {
                    Logger = new HostElasticSearchTrace();
                    break;
                }
                case "client-elastic-search": // Por Elastic Search // For Elastic Search 
                    {
                    Logger = new ClientElasticSearchTrace();
                    break;
                }                
                default:
                    break;
            }
        }

    }
}
