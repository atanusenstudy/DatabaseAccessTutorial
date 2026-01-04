using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.HealthCheck
{
    public class HealthCheckResult
    {
        public string Component { get; set; } = string.Empty;
        public HealthStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public string? Error { get; set; }
    }
}
