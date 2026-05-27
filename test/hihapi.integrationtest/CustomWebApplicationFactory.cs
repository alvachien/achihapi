using hihapi.test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using hihapi.test.common;

namespace hihapi.integrationtest
{
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected SqliteConnection DBConnection { get; private set; }

        public CustomWebApplicationFactory() {
            DBConnection = new SqliteConnection("DataSource=:memory:");
            DBConnection.Open();

            try
            {
                // Create the schema in the database
                var context = GetCurrentDataContext();
                if (!context.Database.IsSqlite())
                {
                    throw new Exception("Expected SQLite database!");
                }

                // Create tables and views
                DataSetupUtility.CreateDatabaseTables(context.Database);
                DataSetupUtility.CreateDatabaseViews(context.Database);

                context.Database.EnsureCreated();

                // Setup the tables
                DataSetupUtility.InitializeSystemTables(context);
                DataSetupUtility.InitializeHomeDefineAndMemberTables(context);

                context.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (DBConnection != null)
            {
                DBConnection.Close();
                DBConnection = null;
            }
        }

        public hihDataContext GetCurrentDataContext()
        {
            var options = new DbContextOptionsBuilder<hihDataContext>()
                .UseSqlite(DBConnection, action =>
                {
                    action.UseRelationalNulls();
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new hihDataContext(options);
            return context;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls("http://localhost");
            builder.UseSetting("AllowedHosts", "*");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<hihDataContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<hihDataContext>(options =>
                {
                    options.UseSqlite(DBConnection, action =>
                    {
                         action.UseRelationalNulls();
                    })
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                    .EnableSensitiveDataLogging();
                });

                // Replace authentication with test handler
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                });
            });
        }
    }
}
