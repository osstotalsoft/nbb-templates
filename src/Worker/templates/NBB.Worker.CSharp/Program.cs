﻿using System;
using System.IO;
using System.Threading.Tasks;
#if AspNetApp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.Serilog;
using Serilog;
using Serilog.Events;
using System.Threading;
#if SqlLogging
using Serilog.Sinks.MSSqlServer;
#endif
#if OpenTracing
using NBB.Tools.Serilog.OpenTracingSink;
#endif

namespace NBB.Worker
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static async Task<int> Main(string[] args)
        {
            try
            {
#if AspNetApp
                var host = BuildWebHost(args);
#else
                var host = BuildConsoleHost(args);
#endif
                Log.Information("Starting NBB.Worker");

                await host.RunAsync(CancellationToken.None);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

#if (AspNetApp)
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostBuilderContext, _) => ConfigureSerilog(hostBuilderContext.Configuration))
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
#else
        public static IHost BuildConsoleHost(string[] args) =>
            Host.CreateDefaultBuilder()
                .ConfigureLogging((hostBuilderContext, loggingBuilder) =>
                {
                    ConfigureSerilog(hostBuilderContext.Configuration);
                    loggingBuilder.AddSerilog();
                })
                .ConfigureServices((hostingContext, services) => services.AddWorkerServices(hostingContext.Configuration))
                .UseConsoleLifetime()
                .Build();
#endif

        private static void ConfigureSerilog(IConfiguration cofiguration)
        {
#if SqlLogging
            var connectionString = cofiguration.GetConnectionString("Log_Database");
            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Remove(StandardColumn.MessageTemplate);
            columnOptions.Store.Add(StandardColumn.LogEvent);
#endif
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationLogEventEnricher>()
                .WriteTo.Console()
#if OpenTracing
                .WriteTo.OpenTracing()
#endif
#if SqlLogging
                .WriteTo.MSSqlServer(connectionString, "__Logs", autoCreateSqlTable: true, columnOptions: columnOptions)
#endif
                .CreateLogger();
        }
    }
}
