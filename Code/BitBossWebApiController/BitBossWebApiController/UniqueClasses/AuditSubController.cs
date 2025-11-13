using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitBossWebApiController
{
    public class AuditSubController
    {

        // MAIN 
        private static AuditSubController _instance = null;
        protected AuditSubController()
        {
        }
        public static AuditSubController Instance()
        {
            if (_instance == null)
                _instance = new AuditSubController();
            return _instance;
        }

        // METHODS
        public object getHostStatus()
        {
            return PipeCommand.Command($"GetHostStatus");
        }
        public object getPhysicalAFTTransactionHistory()
        {
            return PipeCommand.Command($"GetPhysicalAFTTransactionHistory");
        }

        // METHODS
        public object getHostLiveTrace()
        {
            return PipeCommand.Command($"GetHostLiveTrace");
        }

        // METHODS
        public object getClientLiveTrace()
        {
            return PipeCommand.Command($"GetClientLiveTrace");
        }
    }
}
