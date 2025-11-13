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
    public class SASHost
    {
        // MAIN 
        private static SASHost _instance = null;
        protected SASHost() { }             
        public static SASHost Instance()
        {
            if (_instance == null)
                _instance = new SASHost();
            return _instance;
        }

        // METHODS
        public object transferToEGM(int cashablecentsAmount, int restristedcentsAmount, int nonrestrictedcentsAmount)
        {
            return PipeCommand.Command($"afttransferfunds {cashablecentsAmount} {restristedcentsAmount} {nonrestrictedcentsAmount}");
        }

        // METHODS
        public object cashoutToEGM(int cashablecentsAmount, int restristedcentsAmount, int nonrestrictedcentsAmount)
        {
            return PipeCommand.Command($"aftcashoutfunds {cashablecentsAmount} {restristedcentsAmount} {nonrestrictedcentsAmount}");
        }

    }
}
