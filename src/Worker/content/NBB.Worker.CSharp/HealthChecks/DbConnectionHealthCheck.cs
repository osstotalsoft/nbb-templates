﻿#if HealthCheck
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NBB.Worker.HealthChecks
{
    public abstract class DbConnectionHealthCheck : IHealthCheck
    {
        protected DbConnectionHealthCheck(string name, string connectionString)
            : this(name, connectionString, testQuery: null)
        {
        }

        protected DbConnectionHealthCheck(string name, string connectionString, string testQuery)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TestQuery = testQuery;
        }

        public string Name { get; }

        protected string ConnectionString { get; }

        // This sample supports specifying a query to run as a boolean test of whether the database
        // is responding. It is important to choose a query that will return quickly or you risk
        // overloading the database.
        //
        // In most cases this is not necessary, but if you find it necessary, choose a simple query such as 'SELECT 1'.
        protected string TestQuery { get; }

        protected abstract DbConnection CreateConnection(string connectionString);



        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);

                    if (TestQuery != null)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = TestQuery;

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch (DbException ex)
                {
                    return HealthCheckResult.Unhealthy(null, ex);
                }
            }

            return HealthCheckResult.Healthy();
        }
    }
}
#endif