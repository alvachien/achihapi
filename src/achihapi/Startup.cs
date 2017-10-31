#if DEBUG
#else
//#define USE_MICROSOFTAZURE
#define USE_ALIYUN
#undef USE_MICROSOFTAZURE
#endif

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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        internal static String DBConnectionString { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters()
                .AddAuthorization();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
#if DEBUG
                    options.Authority = "http://localhost:41016";
#else
#if USE_MICROSOFTAZURE
                    options.Authority = "http://acidserver.azurewebsites.net";
#elif USE_ALIYUN
                    options.Authority = "http://118.178.58.187:5100";
#endif
#endif
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api.hihapi";
                });

            // DB connection string
#if DEBUG
            DBConnectionString = Configuration["ConnectionStrings:DebugConnection"];
#else
#if USE_ALIYUN
            DBConnectionString = Configuration["ConnectionStrings:AliyunConnection"];
#elif USE_MICROSOFTAZURE
            DBConnectionString = Configuration["ConnectionStrings:AzureConnection"];
#endif
#endif
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseCors(builder =>
#if DEBUG
                builder.WithOrigins(
                    "http://localhost:29521", // AC HIH
                    "https://localhost:29521"
                    )
#else
#if USE_MICROSOFTAZURE
                builder.WithOrigins(
                    "http://achihui.azurewebsites.net",
                    "https://achihui.azurewebsites.net"
                    )
#elif USE_ALIYUN
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

            app.UseMvc();
        }
    }
}
