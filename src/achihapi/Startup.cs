//#define USINGAZURE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace achihapi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#else
#if USINGAZURE
                .AddJsonFile("appsettings.Azure.json", optional: true, reloadOnChange: true)
#else
                .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
#endif
#endif
                .AddEnvironmentVariables()                
                ;

            if (env.IsDevelopment())
            {
                //builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        internal static String DBConnectionString { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            services.AddMvcCore()
                .AddJsonFormatters()
                .AddAuthorization();

            DBConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();

            app.UseCors(builder =>
#if DEBUG
                builder.WithOrigins(
                    "http://localhost:29521", // AC HIH
                    "https://localhost:29521"
                    )
#else
#if USINGAZURE
                builder.WithOrigins(
                    "http://achihui.azurewebsites.net",
                    "https://achihui.azurewebsites.net"
                    )
#else
                builder.WithOrigins(
                    "http://118.178.58.187:5200",
                    "https://118.178.58.187:5200"
                    )
#endif
#endif
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                );

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
#if DEBUG
                Authority = "http://localhost:41016",
#else
#if USINGAZURE
                Authority = "http://acidserver.azurewebsites.net",
#else
                Authority = "http://118.178.58.187:5100/",
#endif
#endif
                RequireHttpsMetadata = false,

                AllowedScopes = { "api.hihapi" },
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc();
        }
    }
}
