using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Pipes;
using Microsoft.AspNetCore.Authorization;

namespace BitBossWebApiController.Controllers
{
    public class AFTTransferParameters
    {
        public int CashableValue { get; set; }
        public int RestrictedValue {get; set;}
        public int NonRestrictedValue {get; set;}
        public string Code { get; set; }
    }

    [ApiController]
    [Route("V0/Transactions/AFT")]
    public class AFTController : ControllerBase
    {
       
        private readonly ILogger<AFTController> _logger;

        public AFTController(ILogger<AFTController> logger)
        {
            _logger = logger;
        }

        [HttpPost("AFTTransfer")]
        public async Task<ActionResult<object>> AFTTransfer(AFTTransferParameters afttransferparameters)
        {
            object t = new object();
            if (afttransferparameters.Code == "0x00")
            {
                return WebAPIController.Instance().transferToEGM(afttransferparameters.CashableValue, afttransferparameters.RestrictedValue, afttransferparameters.NonRestrictedValue);
            }
            else if (afttransferparameters.Code == "0x80")
            {
                return WebAPIController.Instance().cashoutToEGM(afttransferparameters.CashableValue, afttransferparameters.RestrictedValue, afttransferparameters.NonRestrictedValue);
            }
            else
            {
                throw new FormatException("Code must be 0x00 or 0x80");
            }
        }

        [HttpGet("CurrentTransfer")]
        public async Task<ActionResult<object>> AFTCurrentTransfer()
        {

            return WebAPIController.Instance().getCurrentTransfer();

        }
    }

    [ApiController]
    [Route("V0/Transactions/Tito")]
    public class TitoController : ControllerBase
    {
        public class CentsAmount
        {
            public int Value { get; set; }

        }
        private readonly ILogger<TitoController> _logger;

        public TitoController(ILogger<TitoController> logger)
        {
            _logger = logger;
        }
    }

}
