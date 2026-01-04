using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.HealthCheck
{
    public class HealthReport
    {
        public HealthStatus OverallStatus { get; set; }
        public DateTime CheckTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public List<HealthCheckResult> Results { get; set; } = new();
        public string DataAccessTechnology { get; set; } = string.Empty;
    }
}
