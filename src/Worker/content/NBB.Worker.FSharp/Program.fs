namespace NBB.Worker

#if AspNetApp
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
#endif
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open NBB.Correlation.Serilog
open Serilog
open Serilog.Events
#if SqlLogging
open Serilog.Sinks.MSSqlServer
#endif
#if OpenTracing
open NBB.Tools.Serilog.OpenTracingSink
#endif

module Program =
    let ConfigureSerilog (cofiguration: IConfiguration) =

#if SqlLogging
        let connectionString =
            cofiguration.GetConnectionString("Log_Database")

        let columnOptions = ColumnOptions()
        columnOptions.Store.Remove(StandardColumn.Properties) |> ignore
        columnOptions.Store.Remove(StandardColumn.MessageTemplate) |> ignore
        columnOptions.Store.Add(StandardColumn.LogEvent) |> ignore
#endif

        Log.Logger <-
            LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With<CorrelationLogEventEnricher>()
                .WriteTo.Console()
#if OpenTracing
                .WriteTo.OpenTracing()
#endif
#if SqlLogging
                .WriteTo.MSSqlServer(
                    connectionString,
                    MSSqlServerSinkOptions(TableName = "__Logs", AutoCreateSqlTable = true),
                    columnOptions = columnOptions
                )
#endif
                .CreateLogger()

        ()


#if (AspNetApp)
    let createHostBuilder args =
        Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webHostBuilder ->
                webHostBuilder
                    .ConfigureLogging(fun hostingContext _-> ConfigureSerilog(hostingContext.Configuration) |> ignore)
                    .UseStartup<Startup>()
                    .UseSerilog()
                |> ignore)
#else
    let createHostBuilder args =
        Host
            .CreateDefaultBuilder(args)
            .ConfigureServices(fun hostingContext services -> services.AddWorkerServices(hostingContext.Configuration))
            .ConfigureLogging(fun hostingContext loggingBuilder ->
                ConfigureSerilog(hostingContext.Configuration)
                loggingBuilder.AddSerilog() |> ignore)
#endif


    [<EntryPoint>]
    let main args =
        try
            try
                let host = createHostBuilder args

                Log.Information("Starting NBB.Worker") |> ignore
                
                host.Build().Run()
                0
            with ex -> 
                Log.Error(ex, "Application start-up failed")
                1
        finally
            Log.CloseAndFlush()