namespace NBB.Worker.HealthChecks
#if HealthCheck

open System.Data.Common
open System.Threading
open Microsoft.Extensions.Diagnostics.HealthChecks
open FSharp.Control.Tasks

[<AbstractClass>]
type DbConnectionHealthCheck(name: string, connectionString: string, ?testQuery: string) =
    member _.ConnectionString = connectionString
    member _.Name = name

    // This sample supports specifying a query to run as a boolean test of whether the database
    // is responding. It is important to choose a query that will return quickly or you risk
    // overloading the database.
    //
    // In most cases this is not necessary, but if you find it necessary, choose a simple query such as 'SELECT 1'.
    member _.TestQuery = testQuery

    abstract member CreateConnection : string -> DbConnection

    interface IHealthCheck with
        member this.CheckHealthAsync(_context: HealthCheckContext, cancellationToken: CancellationToken) =
            task {
                use connection =
                    this.CreateConnection(this.ConnectionString)

                do! connection.OpenAsync(cancellationToken)

                try
                    if (this.TestQuery.IsSome) then
                        let command = connection.CreateCommand()
                        command.CommandText <- this.TestQuery.Value

                        let! _ = command.ExecuteNonQueryAsync(cancellationToken)
                        ()

                    return HealthCheckResult.Healthy()

                with :? DbException as ex -> return HealthCheckResult.Unhealthy(null, ex)
            }

#endif