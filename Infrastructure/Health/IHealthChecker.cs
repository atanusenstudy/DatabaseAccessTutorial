using Core.Models.HealthCheck;

namespace Infrastructure.Health
{
    public interface IHealthChecker
    {
        string ComponentName { get; }
        Task<HealthCheckResult> CheckHealthAsync();
    }
}