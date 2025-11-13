using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Pipes;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel;

namespace BitBossWebApiController.Controllers
{

    [ApiController]
    [Route("V0/Control")]
    public class ControlController : ControllerBase
    {
        private readonly ILogger<TitoController> _logger;

        public ControlController(ILogger<TitoController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Restart")]
        public async Task<ActionResult<object>> Restart()
        {
            return WebAPIController.Instance().Restart();
        }

        [HttpGet("Health")]
        public async Task<ActionResult<object>> Health()
        {
            return StatusCode(204);
        }
        [HttpGet("Version")]
        public async Task<ActionResult<object>> Version()
        {
            return new {
                device_id = Program.device_id,
                version = $"{Assembly.GetExecutingAssembly().GetName().Version}"
            };
        }

        [HttpGet("APICommHealth")]
        public async Task<ActionResult<object>> APICommHealth()
        {
            return WebAPIController.Instance().APICommHealth();
        }

        [HttpGet("LinksHealth")]
        public async Task<ActionResult<object>> LinksHealth()
        {
            Console.WriteLine("LinksHealth");
            return WebAPIController.Instance().LinksHealth();
        }

        [HttpGet("Test")]
        public async Task<ActionResult<object>> GetTest() {
            return "Test";
        }
        [HttpGet("ETest")]
        public async Task<ActionResult<object>> GetETest4() {
            _logger.LogInformation("Encryption.TestDecrypt()");
            string ephemKeyPub;
            string iv;
            string encryptedMessage;
            var key = Encryption.GetEncyptKeyAgreement(out ephemKeyPub);
            encryptedMessage = Encryption.encrypt4(key, "this is my test", out iv);
            return ephemKeyPub + " " + iv + " " + encryptedMessage;
        }
        [HttpGet("Sig")]
        public async Task<ActionResult<object>> GetSig() {
            _logger.LogInformation("EJwtHttpHandler.createSig");
            return JwtHttpHandler.createSig("test");
        }
        [HttpPost("Test")]
        public async Task<ActionResult<object>> PostTest(object body) {
            _logger.LogInformation("Encryption.TestDecrypt()");
            // Encryption.TestDecrypt();
            // _logger.LogInformation("Encryption.GenerateKeyPair()");
            // Encryption.GenerateKeyPair();
            return body;
        }
        [HttpGet("Start")]
        public async Task<ActionResult<object>> Start() {
            return "start";
        }

        public static void CopyTo(StreamReader input, BinaryWriter outputStream)
        {
            int bufferSize = 16 * 1024; // Fairly arbitrary size
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            int written = 0;
            while ((bytesRead = input.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                written += bytesRead;
                Console.WriteLine($"Writing {written}");
                outputStream.Write(buffer, 0, bytesRead);
            }
        }

        [HttpGet("ClearCache")]
        public async Task<ActionResult<object>> ClearCache() {
            var filename = "clearcache";
            using (System.IO.File.Create(filename)) {}
            return "clear started";
        }
        [DisableRequestSizeLimit]
        // [AllowSynchronousIO]
        [HttpPost("Upgrade/{filename}")]
        public async Task<ActionResult<object>> Upgrade(string fileName) {
            var dir = "upload";
            Directory.CreateDirectory(dir);
            fileName = $"{dir}/{fileName}";
            using (var sr = new StreamReader(Request.Body))
            using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName,FileMode.Create))) // /home/rock/
            {
                _logger.LogInformation("CopyTo(writer)");
                CopyTo(sr, writer);
            }
            return "Upgrade Initiated";
        }
        [HttpGet("Time")]
        public async Task<ActionResult<object>> Time() {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }
        [HttpGet("SetTime/{time}/{tolerance:int=5}")]
        public async Task<ActionResult<object>> SetTime(long time, int tolerance) {
            // _logger.LogInformation($"SetTime {time} {tolerance}");
            long now = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            long offset = time - now;
            if (Math.Abs(offset) <= tolerance) {
                // _logger.LogInformation($"ignoring {offset}");
                return "0";
            }
            string offsetStr = "" + offset;
            // _logger.LogInformation($"setting timeoffset {offset}");
            using (StreamWriter writer = new StreamWriter("timeoffset")) // /home/rock/
            {
               writer.Write(offsetStr);
            }
            return offsetStr;
        }
   }

}
