using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealtyCheckAPI.Extensions
{
    public class SQLConnectionHealthCheck : IHealthCheck
    {

        private static readonly string DefaultTestQuery = "Select 1";

        public string ConnectionString { get; }

        public string TestQuery { get; }

        public SQLConnectionHealthCheck(string connectionString):this(connectionString, testQuery: DefaultTestQuery)
        {

        }

        public SQLConnectionHealthCheck(string connectionString, string testQuery)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TestQuery = testQuery;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                    if(TestQuery != null)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = TestQuery;
                        
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch(DbException ex)
                {
                    return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
                }

            }
            return HealthCheckResult.Healthy();
        }
    }
}
