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
    public class PhysicalEGM
    {
        // MAIN 
        private static PhysicalEGM _instance = null;
        protected PhysicalEGM() { }

        public object GetInfo()
        {
            return PipeCommand.Command($"GetPhysicalEGMInfo");
        }
        public object GetMeters()
        {
            return PipeCommand.Command($"GetMetersFromPhysicalEGM");
        }

        public object GetCurrentTransfer()
        {
            return PipeCommand.Command($"GetCurrentTransfer");
        }
        public static PhysicalEGM Instance()
        {
            if (_instance == null)
                _instance = new PhysicalEGM();
            return _instance;
        }

        // METHODS

    }
}
