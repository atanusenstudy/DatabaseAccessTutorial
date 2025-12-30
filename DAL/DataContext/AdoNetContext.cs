using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace DAL.DataContext
{
    public class AdoNetContext
    {
        private readonly string _connectionString;

        public AdoNetContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}