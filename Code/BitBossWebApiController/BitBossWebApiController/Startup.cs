using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
// using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Microsoft.AspNetCore.Server.Kestrel;

namespace BitBossWebApiController
{
    public class Startup
    {
        // int nonce;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        static public IConfiguration Configuration { get; set;  }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                // options.MaxRequestBodySize = int.MaxValue;
            });
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                // options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });
            // Agregamos autenticación con JWT Token authentication
            // services.AddAuthentication(x =>
            // {
            //     x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //     x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            // })
            // .AddJwtBearer(x =>
            //     {
            //         x.RequireHttpsMetadata = false;
            //         x.SaveToken = false;
            //         x.TokenValidationParameters = new TokenValidationParameters
            //         {
            //             ValidateIssuerSigningKey = false,
            //             IssuerSigningKey =  new BouncyCastleEcdsaSecurityKey(),
            //             ValidateIssuer = false,
            //             ValidateAudience = false,
            //         };
            // });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            // JwtHttpHandler.TestList();
            try {
                JwtHttpHandler.ParseJWT("eyJ0eXAiOiJKV1QiLCJhbGciOiJFUzI1NksifQ.eyJpc3MiOiIwMzU5M2M5NGQ1OWRkYjI2ZWEzMmY3ZDkzNjIwMWMwMTM0ZTkyYTBiMGFiYTY1ZmJhNzRjOGZiMjc4OGE5NTRiNzYiLCJpYXQiOjE2ODQ4MDA3NjAsImV4cCI6MTY4NzM5Mjc2MH0.3ZfRIXQ6G7Q7BAV0xyNTYNxFMAgtnH5_BuS4Id-hjEGXB_ieAi1oqswgNMsOnv2Yz8atJwE-jWv72E-TucAHFw");
            } catch (Exception e) {

            }

            app.Use(async (context, next) => {
                await JwtHttpHandler.Check(context, next);
            });

            app.UseRouting();

            // app.UseAuthentication();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

        }
    }
}
