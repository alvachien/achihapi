using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Batch;
using hihapi.Models;
using Microsoft.AspNetCore.Routing;
using IdentityServer4.AccessTokenValidation;

namespace hihapi
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public string ConnectionString { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.ConnectionString = Configuration["hihapi:ConnectionString"];
            System.Diagnostics.Debug.WriteLine(this.ConnectionString);

            if (Environment.EnvironmentName != "IntegrationTest")
            {
                services.AddDbContext<hihDataContext>(options =>
                    options.UseSqlServer(this.ConnectionString));
            }

            services.AddMvc(x => x.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthorization();

            if (Environment.EnvironmentName == "IntegrationTest")
            {
            }
            else if (Environment.IsDevelopment())
            {                
                // services.AddAuthentication("Bearer")
                //     .AddJwtBearer("Bearer", options =>
                //     {
                //         options.Authority = "http://localhost:41016";
                //         options.RequireHttpsMetadata = false;                        

                //         options.Audience = "api.hih";
                //     });
                services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme,
                        jwtOptions =>
                        {
                            // jwt bearer options
                            jwtOptions.Authority = "http://localhost:41016";
                            jwtOptions.Audience = "api.hih";
                            jwtOptions.RequireHttpsMetadata = false;
                            jwtOptions.SaveToken = true;
                        },
                        referenceOptions =>
                        {
                            // oauth2 introspection options
                        });

                services.AddCors(options =>
                {
                    options.AddPolicy(MyAllowSpecificOrigins, builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:29521" // AC HIH
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
                        options.Authority = "http://118.178.58.187:5100";
                        options.RequireHttpsMetadata = false;

                        options.Audience = "api.hih";
                    });
            }

            services.AddOData();

            // Response Caching
            services.AddResponseCaching();
            // Memory cache
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            // app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors(MyAllowSpecificOrigins);

            ODataModelBuilder modelBuilder = new ODataConventionModelBuilder(app.ApplicationServices);
            modelBuilder.EntitySet<DBVersion>("DBVersions");
            modelBuilder.EntitySet<Currency>("Currencies");
            modelBuilder.EntitySet<Language>("Languages");
            modelBuilder.EntitySet<HomeDefine>("HomeDefines");
            modelBuilder.EntitySet<HomeMember>("HomeMembers");
            modelBuilder.EntitySet<FinanceAccountCategory>("FinanceAccountCategories");
            modelBuilder.EntitySet<FinanceAssetCategory>("FinanceAssetCategories");
            modelBuilder.EntitySet<FinanceDocumentType>("FinanceDocumentTypes");
            modelBuilder.EntitySet<FinanceTransactionType>("FinanceTransactionTypes");
            modelBuilder.Namespace = typeof(Currency).Namespace;

            var model = modelBuilder.GetEdmModel();
            app.UseODataBatching();
            
            app.UseMvc(routeBuilder =>
                {
                    // and this line to enable OData query option, for example $filter
                    routeBuilder.Select().Expand().Filter().OrderBy().MaxTop(100).Count();

                    routeBuilder.MapODataServiceRoute("ODataRoute", "api", model);
                });

            app.UseResponseCaching();
        }
    }
}
