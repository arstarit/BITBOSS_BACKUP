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
using Newtonsoft.Json;

namespace BitBossWebApiController
{
    public class ControlSubController {

        // MAIN 
        private static ControlSubController _instance = null;
        protected ControlSubController() {}

        public static ControlSubController Instance() {
            if (_instance == null)
                _instance = new ControlSubController();
            return _instance;
        }

        // METHODS
        public object Restart() {
            return PipeCommand.Command($"Restart");
        }

        public object APICommHealth() {
            return PipeCommand.Command($"APICommHealth");
        }

        
        public object LinksHealth() {
            return PipeCommand.Command($"LinksHealth");
        }

        public object AFTSettings(dynamic parameters) {
            string json = JsonConvert.SerializeObject(parameters);
            return PipeCommand.Command($"aftsettings {json}");
        }
    }
}
