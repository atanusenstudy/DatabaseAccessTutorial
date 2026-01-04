using Core.Models.HealthCheck;
using DAL.DataContext;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;

namespace Infrastructure.Health.Models.HealthCheckers
{
    public class DapperHealthChecker : IHealthChecker
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _configuration;

        public string ComponentName => "Dapper";

        public DapperHealthChecker(DapperContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                Component = ComponentName,
                Status = HealthStatus.Healthy
            };

            try
            {
                using var connection = _context.CreateConnection();
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    result.Data = new Dictionary<string, object>
                    {
                        ["State"] = "Open",
                        ["Provider"] = "Dapper"
                    };

                    // Get database info
                    var dbName = await connection.ExecuteScalarAsync<string>("SELECT DB_NAME()");
                    var serverName = await connection.ExecuteScalarAsync<string>("SELECT @@SERVERNAME");

                    result.Data["Database"] = dbName;
                    result.Data["Server"] = serverName;
                    result.Data["TestQuery"] = "Success";

                    // Check if this is the current technology
                    var currentTech = _configuration["DataAccess:Technology"] ?? "EntityFramework";
                    if (!currentTech.Equals("Dapper", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Status = HealthStatus.Degraded;
                        result.Description = "Dapper is not the current data access technology";
                    }
                    else
                    {
                        result.Description = "Dapper connection is healthy";
                    }
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.Description = "Connection not open";
                }
            }
            catch (Exception ex)
            {
                var currentTech = _configuration["DataAccess:Technology"] ?? "EntityFramework";
                result.Status = currentTech.Equals("Dapper", StringComparison.OrdinalIgnoreCase)
                    ? HealthStatus.Unhealthy
                    : HealthStatus.Degraded;
                result.Description = currentTech.Equals("Dapper", StringComparison.OrdinalIgnoreCase)
                    ? "Dapper connection failed (Current Technology)"
                    : "Dapper connection failed (Not in use)";
                result.Error = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
            }

            return result;
        }
    }
}