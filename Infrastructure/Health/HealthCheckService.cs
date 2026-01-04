using Core.Interfaces;
using Core.Models.HealthCheck;
using Infrastructure.Health.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;

namespace Infrastructure.Health
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<IHealthChecker> _healthCheckers;

        public HealthCheckService(
            IConfiguration configuration,
            IEnumerable<IHealthChecker> healthCheckers)
        {
            _configuration = configuration;
            _healthCheckers = healthCheckers;
        }

        public async Task<HealthReport> CheckHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new List<HealthCheckResult>();

            // Run all health checks in parallel
            var tasks = _healthCheckers.Select(checker => checker.CheckHealthAsync());
            var healthResults = await Task.WhenAll(tasks);

            results.AddRange(healthResults);

            stopwatch.Stop();

            return new HealthReport
            {
                OverallStatus = CalculateOverallStatus(results),
                CheckTime = DateTime.UtcNow,
                TotalDuration = stopwatch.Elapsed,
                Results = results,
                DataAccessTechnology = _configuration["DataAccess:Technology"] ?? "EntityFramework"
            };
        }

        public async Task<bool> IsDatabaseConnectedAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                    return false;

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DatabaseInfo> GetDatabaseInfoAsync()
        {
            var info = new DatabaseInfo();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                info.Metadata = new Dictionary<string, object>
                {
                    ["Error"] = "Connection string is not configured"
                };
                return info;
            }

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Get basic info
                using var command = new SqlCommand(
                    "SELECT @@SERVERNAME as ServerName, DB_NAME() as DatabaseName",
                    connection);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    info.ServerName = reader["ServerName"]?.ToString();
                    info.DatabaseName = reader["DatabaseName"]?.ToString();
                }

                info.Provider = "SQL Server";

                // Get tables
                await reader.CloseAsync();
                using var tablesCommand = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                    connection);

                using var tablesReader = await tablesCommand.ExecuteReaderAsync();
                while (await tablesReader.ReadAsync())
                {
                    info.Tables.Add(tablesReader.GetString(0));
                }

                // Get last migration
                if (info.Tables.Contains("__EFMigrationsHistory"))
                {
                    await tablesReader.CloseAsync();
                    using var migrationCommand = new SqlCommand(
                        "SELECT MAX(CreatedOn) FROM [dbo].[__EFMigrationsHistory]",
                        connection);

                    var lastMigration = await migrationCommand.ExecuteScalarAsync();
                    if (lastMigration != DBNull.Value && lastMigration != null)
                    {
                        info.LastMigration = (DateTime)lastMigration;
                    }
                }

                // Sanitize connection string
                var builder = new SqlConnectionStringBuilder(connectionString);
                info.SanitizedConnectionString = $"Server={builder.DataSource};Database={builder.InitialCatalog}";
            }
            catch (Exception ex)
            {
                info.Metadata = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message,
                    ["ErrorType"] = ex.GetType().Name
                };
            }

            return info;
        }

        private HealthStatus CalculateOverallStatus(List<HealthCheckResult> results)
        {
            if (results.Any(r => r.Status == HealthStatus.Unhealthy))
                return HealthStatus.Unhealthy;

            if (results.Any(r => r.Status == HealthStatus.Degraded))
                return HealthStatus.Degraded;

            return HealthStatus.Healthy;
        }
    }
}