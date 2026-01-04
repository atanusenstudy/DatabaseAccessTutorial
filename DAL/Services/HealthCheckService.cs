using Core.Interfaces;
using Core.Models.HealthCheck;
using DAL.DataContext;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;

namespace DAL.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IConfiguration _configuration;
        private readonly EntityFrameworkDbContext _efContext;
        private readonly DapperContext _dapperContext;
        private readonly AdoNetContext _adoNetContext;
        private readonly string _dataAccessTechnology;

        public HealthCheckService(
            IConfiguration configuration,
            EntityFrameworkDbContext efContext,
            DapperContext dapperContext,
            AdoNetContext adoNetContext)
        {
            _configuration = configuration;
            _efContext = efContext;
            _dapperContext = dapperContext;
            _adoNetContext = adoNetContext;
            _dataAccessTechnology = _configuration["DataAccess:Technology"] ?? "EntityFramework";
        }

        public async Task<HealthReport> CheckHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new List<HealthCheckResult>();

            // 1. Check Entity Framework Connection
            var efResult = await CheckEntityFrameworkAsync();
            results.Add(efResult);

            // 2. Check Dapper Connection
            var dapperResult = await CheckDapperAsync();
            results.Add(dapperResult);

            // 3. Check ADO.NET Connection
            var adoNetResult = await CheckAdoNetAsync();
            results.Add(adoNetResult);

            // 4. Check Database Schema
            var schemaResult = await CheckDatabaseSchemaAsync();
            results.Add(schemaResult);

            // 5. Check Repository Health
            var repositoryResult = CheckRepositoryHealth();
            results.Add(repositoryResult);

            // 6. Get Database Info
            var databaseInfo = await GetDatabaseInfoAsync();

            stopwatch.Stop();

            return new HealthReport
            {
                OverallStatus = results.Any(r => r.Status == HealthStatus.Unhealthy)
                    ? HealthStatus.Unhealthy
                    : results.Any(r => r.Status == HealthStatus.Degraded)
                        ? HealthStatus.Degraded
                        : HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow,
                TotalDuration = stopwatch.Elapsed,
                Results = results,
                DataAccessTechnology = _dataAccessTechnology,
                DatabaseInfo = databaseInfo
            };
        }

        public async Task<bool> IsDatabaseConnectedAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
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

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                info.ServerName = builder.DataSource;
                info.DatabaseName = builder.InitialCatalog;
                info.Provider = "SQL Server";

                // Sanitize connection string (remove credentials)
                info.ConnectionString = $"Server={builder.DataSource};Database={builder.InitialCatalog}";

                // Get tables
                info.Tables = await GetDatabaseTablesAsync();

                // Get last migration
                info.LastMigration = await GetLastMigrationTimeAsync();
            }
            catch (Exception ex)
            {
                info.Data.Add("Error", ex.Message);
            }

            return info;
        }

        private async Task<HealthCheckResult> CheckEntityFrameworkAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                Component = "Entity Framework",
                Status = HealthStatus.Healthy
            };

            try
            {
                var canConnect = await _efContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    var connection = _efContext.Database.GetDbConnection();
                    result.Data.Add("Database", connection.Database);
                    result.Data.Add("DataSource", connection.DataSource);
                    result.Data.Add("State", connection.State.ToString());

                    // Test a simple query
                    var version = await _efContext.Database.ExecuteSqlRawAsync("SELECT @@VERSION");
                    result.Data.Add("VersionQuery", "Success");

                    // Count entities
                    var employeeCount = await _efContext.Employees.CountAsync();
                    result.Data.Add("EmployeeCount", employeeCount);

                    var projectCount = await _efContext.Projects.CountAsync();
                    result.Data.Add("ProjectCount", projectCount);

                    var ticketCount = await _efContext.Tickets.CountAsync();
                    result.Data.Add("TicketCount", ticketCount);

                    result.Description = "Entity Framework connection is healthy";
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.Description = "Cannot connect to database via Entity Framework";
                }
            }
            catch (Exception ex)
            {
                result.Status = HealthStatus.Unhealthy;
                result.Description = "Entity Framework connection failed";
                result.Error = ex.Message;
                result.Data.Add("ExceptionType", ex.GetType().Name);
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
            }

            return result;
        }

        private async Task<HealthCheckResult> CheckDapperAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                Component = "Dapper",
                Status = HealthStatus.Healthy
            };

            try
            {
                using var connection = _dapperContext.CreateConnection();
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    result.Data.Add("State", "Open");

                    // Test a simple query
                    var version = await connection.ExecuteScalarAsync<string>("SELECT @@VERSION");
                    result.Data.Add("ServerVersion", version?.Substring(0, Math.Min(50, version.Length)) + "...");

                    // Get database info
                    var dbName = await connection.ExecuteScalarAsync<string>("SELECT DB_NAME()");
                    result.Data.Add("Database", dbName);

                    result.Description = "Dapper connection is healthy";
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.Description = "Dapper connection is not open";
                }
            }
            catch (Exception ex)
            {
                result.Status = _dataAccessTechnology.ToLower() == "dapper" ? HealthStatus.Unhealthy : HealthStatus.Degraded;
                result.Description = _dataAccessTechnology.ToLower() == "dapper"
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

        private async Task<HealthCheckResult> CheckAdoNetAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                Component = "ADO.NET",
                Status = HealthStatus.Healthy
            };

            try
            {
                using var connection = _adoNetContext.CreateConnection();
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    result.Data.Add("State", "Open");

                    // Test a simple query
                    using var command = new SqlCommand("SELECT @@VERSION", connection);
                    var version = (await command.ExecuteScalarAsync())?.ToString();
                    result.Data.Add("ServerVersion", version?.Substring(0, Math.Min(50, version.Length)) + "...");

                    // Get server properties
                    using var command2 = new SqlCommand(
                        "SELECT SERVERPROPERTY('ProductVersion') as Version, " +
                        "SERVERPROPERTY('ProductLevel') as Level, " +
                        "SERVERPROPERTY('Edition') as Edition",
                        connection);

                    using var reader = await command2.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        result.Data.Add("ProductVersion", reader["Version"]);
                        result.Data.Add("ProductLevel", reader["Level"]);
                        result.Data.Add("Edition", reader["Edition"]);
                    }

                    result.Description = "ADO.NET connection is healthy";
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.Description = "ADO.NET connection is not open";
                }
            }
            catch (Exception ex)
            {
                result.Status = _dataAccessTechnology.ToLower() == "ado.net" ? HealthStatus.Unhealthy : HealthStatus.Degraded;
                result.Description = _dataAccessTechnology.ToLower() == "ado.net"
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

        private async Task<HealthCheckResult> CheckDatabaseSchemaAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                Component = "Database Schema",
                Status = HealthStatus.Healthy
            };

            try
            {
                var requiredTables = new[] { "Employees", "Projects", "Tickets", "__EFMigrationsHistory" };
                var existingTables = await GetDatabaseTablesAsync();

                result.Data.Add("RequiredTables", requiredTables);
                result.Data.Add("ExistingTables", existingTables);

                var missingTables = requiredTables.Except(existingTables).ToList();

                if (missingTables.Any())
                {
                    result.Status = HealthStatus.Degraded;
                    result.Description = $"Missing tables: {string.Join(", ", missingTables)}";
                    result.Data.Add("MissingTables", missingTables);
                }
                else
                {
                    result.Description = "All required tables exist";
                }

                // Check table row counts
                using var connection = _dapperContext.CreateConnection();

                foreach (var table in new[] { "Employees", "Projects", "Tickets" })
                {
                    if (existingTables.Contains(table))
                    {
                        var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {table}");
                        result.Data.Add($"{table}Count", count);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = HealthStatus.Degraded;
                result.Description = "Failed to check database schema";
                result.Error = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
            }

            return result;
        }

        private HealthCheckResult CheckRepositoryHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new HealthCheckResult
            {
                Component = "Repository Configuration",
                Status = HealthStatus.Healthy
            };

            try
            {
                result.Data.Add("CurrentTechnology", _dataAccessTechnology);
                result.Data.Add("ConnectionStringConfigured", !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")));

                // Check if all required repositories are registered
                var requiredRepositories = new[]
                {
                    "IEmployeeRepository",
                    "IProjectRepository",
                    "ITicketRepository"
                };

                result.Data.Add("RequiredRepositories", requiredRepositories);
                result.Description = $"Using {_dataAccessTechnology} data access technology";
            }
            catch (Exception ex)
            {
                result.Status = HealthStatus.Degraded;
                result.Description = "Failed to check repository configuration";
                result.Error = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
            }

            return result;
        }

        private async Task<List<string>> GetDatabaseTablesAsync()
        {
            var tables = new List<string>();

            try
            {
                using var connection = _dapperContext.CreateConnection();
                var result = await connection.QueryAsync<string>(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
                tables = result.ToList();
            }
            catch
            {
                // If Dapper fails, try ADO.NET
                try
                {
                    using var connection = _adoNetContext.CreateConnection();
                    await connection.OpenAsync();
                    using var command = new SqlCommand(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                        connection);

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
                catch
                {
                    // If all fails, return empty list
                }
            }

            return tables;
        }

        private async Task<DateTime?> GetLastMigrationTimeAsync()
        {
            try
            {
                using var connection = _dapperContext.CreateConnection();
                var lastMigration = await connection.QueryFirstOrDefaultAsync<DateTime?>(
                    "SELECT MAX(CREATED_ON) FROM [dbo].[__EFMigrationsHistory]");
                return lastMigration;
            }
            catch
            {
                return null;
            }
        }
    }
}