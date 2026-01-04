namespace Infrastructure.Health.Models
{
    public class DatabaseInfo
    {
        public string? ServerName { get; set; }
        public string? DatabaseName { get; set; }
        public string? Provider { get; set; }
        public DateTime? LastMigration { get; set; }
        public List<string> Tables { get; set; } = new();
        public Dictionary<string, object>? Metadata { get; set; }
        public string? SanitizedConnectionString { get; set; }
    }
}