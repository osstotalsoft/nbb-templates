namespace NBB.Worker
#if AspNetApp

#if HealthCheck 
open NBB.Worker.HealthChecks
open Microsoft.AspNetCore.Diagnostics.HealthChecks;
open HealthChecks.UI.Client;
open Microsoft.AspNetCore.Http
#endif
open Microsoft.AspNetCore.Builder;
open Microsoft.AspNetCore.Hosting;
open Microsoft.Extensions.Configuration;
open Microsoft.Extensions.DependencyInjection;
open System
open Microsoft.Extensions.Diagnostics.HealthChecks
open System.Threading.Tasks

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddWorkerServices(this.Configuration) |> ignore

#if HealthCheck
        services.AddHealthChecks() // Registers health checks services
            // Add a health check for a SQL database
            .AddCheck("SQL database",
                SqlConnectionHealthCheck("Log_Database", this.Configuration.["ConnectionStrings:Log_Database"]))
            .AddCheck<GCInfoHealthCheck>("GC")
            |> ignore

#endif

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
#if HealthCheck
        let writeResponse context report = UIResponseWriter.WriteHealthCheckUIResponse(context, report)
        app.UseHealthChecks(PathString "/health", options = 
            HealthCheckOptions(ResponseWriter = Func<HttpContext, HealthReport, Task>(writeResponse))) |> ignore
#endif
#endif