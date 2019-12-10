using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using hihapi.Models;

namespace hihapi.test.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public CustomWebApplicationFactory(): base()
        {
            hihDataContext.TestingMode = true;

            // Open connections
            DBConnection = new SqliteConnection("DataSource=:memory:");
            DBConnection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<hihDataContext>()
                    .UseSqlite(DBConnection)
                    .Options;

                // Create the schema in the database
                CurrentDataContext = new hihDataContext(options);
                CurrentDataContext.Database.EnsureCreated();

                // Setup the tables
                DataSetupUtility.InitializeSystemTables(CurrentDataContext);
                DataSetupUtility.InitializeHomeDefineAndMemberTables(CurrentDataContext);
            }
            catch
            {
                // Error occurred
            }
            finally
            {
            }
        }

        public SqliteConnection DBConnection { get; private set; }
        public hihDataContext CurrentDataContext { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTest");

            builder.ConfigureServices(services =>
            {
                services.AddDbContext<hihDataContext>((options, context) =>
                    {
                        context.UseSqlite(DBConnection);
                    });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                }
            });

            //builder.ConfigureServices(services =>
            //{
            //    // In-memory database only exists while the connection is open
            //    var connection = new SqliteConnection("DataSource=:memory:");
            //    connection.Open();

            //    var descriptor = services.SingleOrDefault(
            //        d => d.ServiceType == typeof(Microsoft.AspNetCore.Authentication.AuthenticationOptions));
            //    if (descriptor != null)
            //    {
            //        services.Remove(descriptor);
            //    }

            //    // Add the Jwt bear back
            //    services.AddAuthentication("Bearer")
            //        .AddJwtBearer("Bearer", options =>
            //        {
            //            options.Authority = DataSetupUtility.IntegrationTestIdentityServerUrl;
            //            options.RequireHttpsMetadata = false;

            //            options.Audience = DataSetupUtility.IntegrationTestAPIScope;
            //        });

            //    // Remove the app's ApplicationDbContext registration.
            //    descriptor = services.SingleOrDefault(
            //        d => d.ServiceType == typeof(DbContextOptions<hihDataContext>));

            //    if (descriptor != null)
            //    {
            //        services.Remove(descriptor);
            //    }

            //    // Add ApplicationDbContext using an in-memory database for testing.
            //    services.AddDbContext<hihDataContext>((options, context) =>
            //        {
            //            context.UseSqlite(connection);
            //        });

            //    // Build the service provider.
            //    var sp = services.BuildServiceProvider();

            //    // Create a scope to obtain a reference to the database
            //    // context (kbdataContext).
            //    using (var scope = sp.CreateScope())
            //    {
            //        var scopedServices = scope.ServiceProvider;
            //        var db = scopedServices.GetRequiredService<hihDataContext>();
            //        var logger = scopedServices
            //            .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            //        // Ensure the database is created.
            //        db.Database.EnsureCreated();

            //        try
            //        {
            //            DataSetupUtility.InitializeSystemTables(db);
            //            DataSetupUtility.InitializeHomeDefineAndMemberTables(db);
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.LogError(ex, "An error occurred seeding the " +
            //                "database with test messages. Error: {Message}", ex.Message);
            //        }
            //    }
            //});
        }
    }
}
