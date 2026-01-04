using Core.Models.HealthCheck;
using DAL.DataContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Infrastructure.Health.Models.HealthCheckers
{
    public class EntityFrameworkHealthChecker : IHealthChecker
    {
        private readonly EntityFrameworkDbContext _context;

        public string ComponentName => "Entity Framework";

        public EntityFrameworkHealthChecker(EntityFrameworkDbContext context)
        {
            _context = context;
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
                var canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    var connection = _context.Database.GetDbConnection();
                    result.Data = new Dictionary<string, object>
                    {
                        ["Database"] = connection.Database,
                        ["DataSource"] = connection.DataSource,
                        ["State"] = connection.State.ToString(),
                        ["Provider"] = "Entity Framework Core"
                    };

                    // Test query
                    await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                    result.Data["TestQuery"] = "Success";

                    // Get entity counts
                    result.Data["EmployeeCount"] = await _context.Employees.CountAsync();
                    result.Data["ProjectCount"] = await _context.Projects.CountAsync();
                    result.Data["TicketCount"] = await _context.Tickets.CountAsync();

                    result.Description = "Entity Framework connection is healthy";
                }
                else
                {
                    result.Status = HealthStatus.Unhealthy;
                    result.Description = "Cannot connect to database";
                }
            }
            catch (Exception ex)
            {
                result.Status = HealthStatus.Unhealthy;
                result.Description = "Connection failed";
                result.Error = ex.Message;
                result.Data = new Dictionary<string, object>
                {
                    ["ExceptionType"] = ex.GetType().Name
                };
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