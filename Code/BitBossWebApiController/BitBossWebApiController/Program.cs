using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.IO;

namespace BitBossWebApiController
{
    public class Program
    {
        public static string device_id = "unknown";
        public static string listening_port = "5002";
        public static void Main(string[] args)
        {
            Console.WriteLine($"{Assembly.GetExecutingAssembly().GetName().Version}");
            if (args.Length == 1 && args[0] == "-v") return;

            try {
                device_id = File.ReadAllText("device_id").Trim();
                listening_port = File.ReadAllText("api_port").Trim();
                Console.WriteLine($"device_id: {device_id}");
            } catch (Exception e) {
                Console.WriteLine("Exception");
                Console.WriteLine(e);
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://*:{listening_port}");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
