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
using achihapi.Models;

namespace achihapi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()                
                ;

            if (env.IsDevelopment())
            {
                //builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
#if DEBUG
        internal static String DebugConnectionString { get; private set; }
#endif
        internal static String DBConnectionString { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Add framework services.
            services.AddMvcCore()
                .AddJsonFormatters()
                .AddAuthorization(
                    options => {
                        options.AddPolicy("LearningAdmin", policy => policy.RequireClaim("LearningAdmin", "1"));
                        options.AddPolicy("KnowledgeAdmin", policy => policy.RequireClaim("KnowledgeAdmin", "1"));
                        options.AddPolicy("GalleryAdmin", policy => policy.RequireRole("GalleryAdmin"));
                        options.AddPolicy("GalleryPro", policy => policy.RequireRole("GalleryPro"));
                        options.AddPolicy("TodoAdmin", policy => policy.RequireClaim("TodoAdmin", "1"));
                    }
                );

            DBConnectionString = Configuration.GetConnectionString("DefaultConnection");
#if DEBUG
            DebugConnectionString = Configuration.GetConnectionString("DebugConnection");
#endif
            services.AddDbContext<achihdbContext>(options => options.UseSqlServer(DBConnectionString));
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
                    "http://localhost:29521",
                    "http://localhost:1601",
                    "https://localhost:29521",
                    "https://localhost:1601"
                    )
#else
                builder.WithOrigins(
                    "http://achihui.azurewebsites.net",
                    "http://acgallery.azurewebsites.net",
                    "https://achihui.azurewebsites.net",
                    "https://acgallery.azurewebsites.net"
                    )
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
                Authority = "http://acidserver.azurewebsites.net",
#endif
                RequireHttpsMetadata = false,

                ScopeName = "api.hihapi",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc();
        }
    }
}
