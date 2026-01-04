using Core.Models.HealthCheck;
using DAL.DataContext;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;

namespace Infrastructure.Health.Models.HealthCheckers
{
    public class AdoNetHealthChecker : IHealthChecker
    {
        private readonly AdoNetContext _context;
        private readonly IConfiguration _configuration;

        public string ComponentName => "ADO.NET";

        public AdoNetHealthChecker(AdoNetContext context, IConfiguration configuration)
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
                        ["Provider"] = "ADO.NET"
                    };

                    // Get server info
                    using var command = new SqlCommand(
                        "SELECT @@SERVERNAME as ServerName, DB_NAME() as DatabaseName",
                        connection);

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        result.Data["Server"] = reader["ServerName"];
                        result.Data["Database"] = reader["DatabaseName"];
                        result.Data["TestQuery"] = "Success";
                    }

                    // Check if this is the current technology
                    var currentTech = _configuration["DataAccess:Technology"] ?? "EntityFramework";
                    if (!currentTech.Equals("AdoNet", StringComparison.OrdinalIgnoreCase) &&
                        !currentTech.Equals("ADO.NET", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Status = HealthStatus.Degraded;
                        result.Description = "ADO.NET is not the current data access technology";
                    }
                    else
                    {
                        result.Description = "ADO.NET connection is healthy";
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
                var isCurrent = currentTech.Equals("AdoNet", StringComparison.OrdinalIgnoreCase) ||
                               currentTech.Equals("ADO.NET", StringComparison.OrdinalIgnoreCase);

                result.Status = isCurrent ? HealthStatus.Unhealthy : HealthStatus.Degraded;
                result.Description = isCurrent
                    ? "ADO.NET connection failed (Current Technology)"
                    : "ADO.NET connection failed (Not in use)";
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