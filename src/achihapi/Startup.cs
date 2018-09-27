using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace achihapi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            HostingEnvironment = env;
            Configuration = config;

            UnitTestMode = HostingEnvironment.EnvironmentName == "Test";
        }

        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        internal static String DBConnectionString { get; private set; }
        internal static Boolean UnitTestMode { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            if (UnitTestMode)
            {
                services.AddMvcCore()
                    .AddJsonFormatters();
            }
            else
            {
                services.AddMvcCore()
                    .AddAuthorization()
                    .AddJsonFormatters();

                services.AddAuthentication("Bearer")
                    .AddIdentityServerAuthentication(options =>
                    {
#if DEBUG
#if USE_SSL
                        options.Authority = "https://localhost:44397";
#else
                        options.Authority = "http://localhost:41016";
#endif
#else
#if USE_AZURE
#if USE_SSL
                        options.Authority = "https://acidserver.azurewebsites.net";
#else
                        options.Authority = "http://acidserver.azurewebsites.net";
#endif
#elif USE_ALIYUN
#if USE_SSL
                        options.Authority = "https://118.178.58.187:5100";
#else
                        options.Authority = "http://118.178.58.187:5100";
#endif
#endif
#endif
                        options.RequireHttpsMetadata = false;
                        options.ApiName = "api.hihapi";
                    });
            }

            // DB connection string
#if DEBUG
            DBConnectionString = Configuration.GetConnectionString("DebugConnection");
            if (UnitTestMode)
                DBConnectionString = Configuration.GetConnectionString("UnitTestConnection");
#else
#if USE_ALIYUN
            DBConnectionString = Configuration.GetConnectionString("AliyunConnection");
#elif USE_AZURE
            DBConnectionString = Configuration.GetConnectionString("AzureConnection");
#endif
#endif
            // Response Caching
            services.AddResponseCaching();
            // Memory cache
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (UnitTestMode)
            {
            }
            else
            {
                app.UseAuthentication();
            }

            app.UseCors(builder =>
#if DEBUG
                builder.WithOrigins(
#if USE_SSL
                    "https://localhost:29521"
#else
                    "http://localhost:29521" // AC HIH
#endif
                    )
#else
#if USE_AZURE
                builder.WithOrigins(
#if USE_SSL
                    "https://achihui.azurewebsites.net"                    
#else
                    "http://achihui.azurewebsites.net"
#endif
                    )
#elif USE_ALIYUN
                builder.WithOrigins(
#if USE_SSL
                    "https://118.178.58.187:5200"
#else
                    "http://118.178.58.187:5200"
#endif
                    )
#endif
#endif
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                );

            app.UseMvc();

            app.UseResponseCaching();
        }
    }
}
