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

    public class AFTSettingsParameters
    {
        public int? AssetNumber { get; set; }
        public bool? ForceDummyRegistrationOnStartup {get; set;}
    }


    [ApiController]
    [Route("V0/Settings")]
    public class SettingsInterfacingController : ControllerBase
    {
       
        private readonly ILogger<SettingsInterfacingController> _logger;

        public SettingsInterfacingController(ILogger<SettingsInterfacingController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Interfacing")]
        public async Task<ActionResult<object>> InterfacingSettings()
        {
            return WebAPIController.Instance().GetInterfacingSettings();
        }

        [HttpGet("PhysicalEGMSettings")]
        public async Task<ActionResult<object>> PhysicalEGMSettings()
        {
            return WebAPIController.Instance().GetPhysicalEGMSettings();
        }

        [HttpPost("AFTSettings")]
        public async Task<ActionResult<object>> AFTSettings(AFTSettingsParameters aftsettingsparameters)
        {
            return WebAPIController.Instance().AFTSettings(aftsettingsparameters);
        }


    }

    

}
