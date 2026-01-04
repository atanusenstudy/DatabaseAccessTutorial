using Core.Models.HealthCheck;

namespace Core.Interfaces
{
    public interface IHealthCheckService
    {
        Task<HealthReport> CheckHealthAsync();
        Task<bool> IsDatabaseConnectedAsync();
    }
}