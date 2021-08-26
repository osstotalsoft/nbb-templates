namespace NBB.Worker.HealthChecks
#if HealthCheck

open System.Threading
open Microsoft.Extensions.Diagnostics.HealthChecks
open System.Threading.Tasks
open System

type GCInfoHealthCheck() =
    member _.Name = "GCInfo"

    interface IHealthCheck with
        member _.CheckHealthAsync(_context: HealthCheckContext, _cancellationToken: CancellationToken) =
            // This example will report degraded status if the application is using
            // more than 1gb of memory.
            //
            // Additionally we include some GC info in the reported diagnostics.
            let allocated =
                GC.GetTotalMemory(forceFullCollection = false)

            let data =
                [ "Allocated", box allocated
                  "Gen0Collections", box (GC.CollectionCount 0)
                  "Gen1Collections", box (GC.CollectionCount 1)
                  "Gen2Collections", box (GC.CollectionCount 2) ]
                |> readOnlyDict


            // Report degraded status if the allocated memory is >= 1gb (in bytes)
            let status =
                if allocated >= 1024L * 1024L * 1024L then
                    HealthStatus.Degraded
                else
                    HealthStatus.Healthy

            Task.FromResult(HealthCheckResult(status, "reports degraded status if allocated bytes >= 1gb", null, data))

#endif
