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
    [ApiController]
    [Route("V0/Audit")]
    public class AuditController : ControllerBase
    {
        private readonly ILogger<TitoController> _logger;

        public AuditController(ILogger<TitoController> logger)
        {
            _logger = logger;
        }

        [Route("HostStatus")]
        public async Task<ActionResult<object>> HostStatus()
        {
            return WebAPIController.Instance().getHostStatus();
        }
    }

    
    [ApiController]
    [Route("V0/Audit/PhysicalEGM")]
    public class AuditPhisicalEGMControlelr : ControllerBase
    {
       
        private readonly ILogger<AFTController> _logger;

        public AuditPhisicalEGMControlelr(ILogger<AFTController> logger)
        {
            _logger = logger;
        }

        [HttpGet("AFTTransactionHistory")]
        public async Task<ActionResult<object>> AFTTransfer()
        {
            return WebAPIController.Instance().getPhysicalAFTTransactionHistory();
        }

        [HttpGet("SASTrace")]
        public async Task<ActionResult<object>> LiveTrace()
        {
            return WebAPIController.Instance().getHostLiveTrace();
        }

    }

    [ApiController]
    [Route("V0/Audit/VirtualEGM")]
    public class AuditVirtualEGMControlelr : ControllerBase
    {
        public class CentsAmount
        {
            public int Value { get; set; }

        }
        private readonly ILogger<TitoController> _logger;

        public AuditVirtualEGMControlelr(ILogger<TitoController> logger)
        {
            _logger = logger;
        }

        [HttpGet("SmibTrace")]
        public async Task<ActionResult<object>> LiveTrace()
        {
            return WebAPIController.Instance().getClientLiveTrace();
        }
    }

    [ApiController]
    [Route("V0/Audit/Interface")]
    public class AuditInterfaceControlelr : ControllerBase
    {
        public class CentsAmount
        {
            public int Value { get; set; }

        }
        private readonly ILogger<TitoController> _logger;

        public AuditInterfaceControlelr(ILogger<TitoController> logger)
        {
            _logger = logger;
        }
    }



}
