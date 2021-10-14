namespace NBB.Worker.HealthChecks
#if HealthCheck

open System.Data.SqlClient
open System.Data.Common

type SqlConnectionHealthCheck(name: string, connectionString: string, ?testQuery: string) =
    inherit DbConnectionHealthCheck(name, connectionString, testQuery |> Option.defaultValue(SqlConnectionHealthCheck.DefaultTestQuery))
    
    static member DefaultTestQuery = "Select 1";
   
    override _.CreateConnection(connectionString: string) =
        new SqlConnection(connectionString) :> DbConnection

#endif

