using Core.Interfaces;
using Core.Models.HealthCheck;
using Infrastructure.Health.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            IHealthCheckService healthCheckService,
            ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetHealth()
        {
            try
            {
                var report = await _healthCheckService.CheckHealthAsync();

                var response = new
                {
                    Success = report.OverallStatus == HealthStatus.Healthy,
                    Message = GetHealthMessage(report.OverallStatus),
                    Data = report,
                    Timestamp = DateTime.UtcNow
                };

                return report.OverallStatus switch
                {
                    HealthStatus.Healthy => Ok(response),
                    HealthStatus.Degraded => StatusCode(206, response), // Partial Content
                    HealthStatus.Unhealthy => StatusCode(503, response), // Service Unavailable
                    _ => StatusCode(500, response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Health check failed",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("database")]
        public async Task<ActionResult> CheckDatabase()
        {
            try
            {
                var isConnected = await _healthCheckService.IsDatabaseConnectedAsync();

                return Ok(new
                {
                    Success = true,
                    Message = isConnected ? "Database is connected" : "Database is not connected",
                    Data = isConnected,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database check failed");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Database check failed",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                Status = "OK",
                Message = "API is running",
                Timestamp = DateTime.UtcNow,
                Uptime = Environment.TickCount64 / 1000
            });
        }

        private static string GetHealthMessage(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => "All systems operational",
                HealthStatus.Degraded => "System is degraded",
                HealthStatus.Unhealthy => "System is unhealthy",
                _ => "Unknown health status"
            };
        }
    }
}