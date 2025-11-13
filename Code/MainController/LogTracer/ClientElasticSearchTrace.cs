using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;

namespace MainController
{
    // Elastic Search Trace
    public class ClientElasticSearchTrace : LoggerInterface
    {
        // Configuración // Configuration
        private static void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            // Carga la configuración de la appsettings.json y buildea // Load the configuration of the appsettings.json and build 
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .Build();
            // Crea el Logger, con especial énfasis en WriteTo.Elasticsearch // Creates the Logger, with special emphasis on WriteTo.Elasticsearch 
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                .Enrich.WithProperty("Environment", environment)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        // ConfigureElasticSink
        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                // AutoRegisterTemplate en true
                AutoRegisterTemplate = true,
                // IndexFormat: En Bitboss-MainController- 
                IndexFormat = $"Bitboss-MainController-Client-{DateTime.UtcNow:yyyy-MM}" // Bitboss-MainController-*
            };
        }

        // Configuración // Configuration
        private void Configure()
        {
            //configure logging first
            ConfigureLogging();
        }

        public override void GetTrace(ref object trace) // se obtiene el sastrace // sastrace is obtained 
        {
            /* En esta instancia, no se devuelve el SASTrace, 
               no se utiliza debido a que solamente enviamos los logs a un servicio externo*/

            /* In this instance, the SASTrace is not returned., 
               is not used because we only send the logs to an external service.*/
        }

        public override void AddTrace(string message, string type, bool crc, bool isRetry) // Se añade una line nueva // A new line is added 
        {
            Log.Information($"TimeStamp: {DateTime.Now} Message: {message} Direction: {type}, CRC: {crc}, IsRetry: {isRetry}");
        }

        public override void Init()  // Inicialización // Initialization
        {
            Configure();
        }
    }

}

