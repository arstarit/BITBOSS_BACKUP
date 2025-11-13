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
    public class MainController
    {
        private static WebAPIController BitbossCTL;

        private static VirtualEGM EGM;

        private static SASHost Host;

        // MAIN 
        private static MainController _instance = null;
        protected MainController()
        {
        }

        public object GetPhysicalEGMSettings()
        {
            return PipeCommand.Command("GetPhysicalEGMSettings");
        }
        public object GetInterfacingSettings()
        {
            return PipeCommand.Command($"GetInterfacingSettings");
        }
        public static MainController Instance()
        {
            if (_instance == null)
                _instance = new MainController();
            return _instance;
        }

        // METHODS
    }
}
