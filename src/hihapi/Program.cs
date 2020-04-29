using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace hihapi
{
    public class Program
    {
        public static int Main(string[] args)
        {
#if DEBUG
#else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                //.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                .WriteTo.File(
                    @"C:\WebApps\Logs\HIHAPI\log.txt",
                    fileSizeLimitBytes: 1_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                //.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();
#endif

            try
            {
                var host = CreateWebHostBuilder(args).Build();
#if DEBUG
#else
                Log.Information("Starting host...");
#endif
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
#if DEBUG
#else
                Log.Fatal(ex, "Host terminated unexpectedly.");
#endif
                return 1;
            }
            finally
            {
#if DEBUG
#else
                Log.CloseAndFlush();
#endif
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
