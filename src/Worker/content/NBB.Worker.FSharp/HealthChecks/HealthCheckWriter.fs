namespace NBB.Worker.HealthChecks
#if HealthCheck

open Microsoft.AspNetCore.Http
open System.Linq
open Microsoft.Extensions.Diagnostics.HealthChecks
open Newtonsoft.Json.Linq
open Newtonsoft.Json

type HealthCheckWriter =
    member _.Name = "GCInfo"

    member _.WriteResponse(httpContext: HttpContext, result: HealthReport) =
        httpContext.Response.ContentType <- "application/json"

        let json =
            JObject(
                JProperty("status", result.Status.ToString()),
                JProperty(
                    "results",
                    JObject(
                        result.Entries.Select
                            (fun pair ->
                                JProperty(
                                    pair.Key,
                                    JObject(
                                        JProperty("status", pair.Value.Status.ToString()),
                                        JProperty("description", pair.Value.Description),
                                        JProperty(
                                            "data",
                                            JObject(pair.Value.Data.Select(fun p -> JProperty(p.Key, p.Value)))
                                        )
                                    )
                                ))
                        |> Seq.toArray
                    )
                )
            )

        httpContext.Response.WriteAsync(json.ToString(Formatting.Indented))


#endif
