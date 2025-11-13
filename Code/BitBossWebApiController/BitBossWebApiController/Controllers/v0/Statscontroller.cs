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
    [Route("V0/Stats/PhysicalEGM")]
    public class PhysicalEGMController : ControllerBase
    {
       
        private readonly ILogger<AFTController> _logger;

        public PhysicalEGMController(ILogger<AFTController> logger)
        {
            _logger = logger;
        }

        [HttpGet("0/Info")]
        public async Task<ActionResult<object>> GetInfo()
        {
            return WebAPIController.Instance().getInfoFromPhysicalEGM();
        }
        [HttpGet("0/Meters")]
        public async Task<ActionResult<object>> GetMeters()
        {
            return WebAPIController.Instance().getMetersFromPhysicalEGM();
        }

    }

    [ApiController]
    [Route("V0/Stats/VirtualEGM")]
    public class VirtualEGMController : ControllerBase
    {
        private readonly ILogger<TitoController> _logger;

        public VirtualEGMController(ILogger<TitoController> logger)
        {
            _logger = logger;
        }
    }

}
