using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitBossWebApiController
{
    public class VirtualEGM
    {       
       // MAIN 
        private static VirtualEGM _instance = null;
        protected VirtualEGM()
        {
      

        }             
        public static VirtualEGM Instance()
        {
            if (_instance == null)
                _instance = new VirtualEGM();
            return _instance;
        }

        // METHODS

    }
}
