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
using Microsoft.AspNetCore.Cors;

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
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                //builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            //services.A<IConfigurationRoot>(Configuration);

            services.AddCors();

            // Add framework services.
            services.AddMvc();

            //var strConn = @"Server=(LocalDB)\MSSQLLocalDB;Initial Catalog=achihdb;User ID=actest;Password=actest;MultipleActiveResultSets=True";
            ////var strConn = Configuration.GetSection("ConnectionStrings")["DefaultConnection"];
            var strConn = Configuration.GetConnectionString("DefaultConnection");

            if (String.IsNullOrEmpty(strConn))
            {
                // Do nothing!
            }

            services.AddDbContext<achihdbContext>(options => options.UseSqlServer(strConn));
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
                //builder.WithOrigins("http://localhost:29521")
                builder.WithOrigins("http://achihui.azurewebsites.net")
                .AllowAnyHeader()
                .AllowAnyMethod()
                );

            app.UseMvc();
        }
    }
}
