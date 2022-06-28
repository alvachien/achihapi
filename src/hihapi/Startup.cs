using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using hihapi.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text.Json.Serialization;

namespace hihapi
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;

            try
            {
                UploadFolder = Path.Combine(env.ContentRootPath, @"uploads");
                if (!Directory.Exists(UploadFolder))
                {
                    Directory.CreateDirectory(UploadFolder);
                }

                BlogFolder = Path.Combine(env.ContentRootPath, @"blogs");
                if (!Directory.Exists(BlogFolder))
                {
                    Directory.CreateDirectory(BlogFolder);
                }
            }
            catch(Exception)
            {
                // Do nothing
            }
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public string ConnectionString { get; private set; }
        internal static String UploadFolder { get; private set; }
        internal static String BlogFolder { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors();

#if DEBUG
            this.ConnectionString = Configuration["hihapi:ConnectionString"];
            if (this.ConnectionString.EndsWith(';'))
                this.ConnectionString += "Encrypt=False;";
            else
                this.ConnectionString += ";Encrypt=False;";
            System.Diagnostics.Debug.WriteLine(this.ConnectionString);
#else
#if USE_ALIYUN
            this.ConnectionString = Configuration.GetConnectionString("AliyunConnection");
#elif USE_AZURE
            this.ConnectionString = Configuration.GetConnectionString("AzureConnection");
#endif
#endif
            services.AddDbContext<hihDataContext>(options => options.UseSqlServer(this.ConnectionString));

            services.AddHttpContextAccessor();

            IEdmModel model = EdmModelBuilder.GetEdmModel();

            services.AddControllers()
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(100)
                .AddRouteComponents(model)
                .AddRouteComponents("v1", model)
                );

            if (Environment.IsDevelopment())
            {
                // accepts any access token issued by identity server
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = "https://localhost:44353";
                        options.RequireHttpsMetadata = false;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };
                    });

                services.AddCors(options =>
                {
                    options.AddPolicy(MyAllowSpecificOrigins, builder =>
                    {
                        builder.WithOrigins(
                            "https://localhost:29521",  // AC HIH
                            "https://localhost:29525"   // acblog
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });
            }
            else if (Environment.IsProduction())
            {
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
#if USE_ALIYUN
                        options.Authority = "https://www.alvachien.com/idserver";
#elif USE_AZURE
                        options.Authority = "https://acidsrv.azurewebsites.net";
#endif
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.IncludeErrorDetails = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };

                        options.Audience = "api.hih";
                    });
                services.AddCors(options =>
                {
                    options.AddPolicy(MyAllowSpecificOrigins, builder =>
                    {
                        builder.WithOrigins(
#if USE_ALIYUN
                            "https://www.alvachien.com/hih",
                            "https://www.alvachien.com/alvablog",
                            "https://www.alvachien.com/fishblog"
#elif USE_AZURE
#endif
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });
            }

            services.AddAuthorization();

            // Response Caching
            services.AddResponseCaching();
            // Memory cache
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(MyAllowSpecificOrigins);

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging(); // <-- Add this line

            app.UseODataBatching();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseResponseCaching();

            var cachePeriod = env.IsDevelopment() ? "10" : "30";
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(BlogFolder),
                RequestPath = "/blogs",
                OnPrepareResponse = ctx =>
                {
                    // Requires the following import:
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                }
            });
        }
    }
}
