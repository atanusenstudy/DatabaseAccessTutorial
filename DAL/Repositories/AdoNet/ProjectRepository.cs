using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL.Repositories.AdoNet
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AdoNetContext _context;

        public ProjectRepository(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<Project> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Projects WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapProject(reader);
            }
            return null!;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            var projects = new List<Project>();
            var query = "SELECT * FROM Projects";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                projects.Add(MapProject(reader));
            }
            return projects;
        }

        public async Task<int> AddAsync(Project entity)
        {
            var query = @"
                INSERT INTO Projects (Name, Description, StartDate, EndDate, Status, Technology, Budget, ClientName, CreatedAt)
                VALUES (@Name, @Description, @StartDate, @EndDate, @Status, @Technology, @Budget, @ClientName, @CreatedAt);
                SELECT SCOPE_IDENTITY()";

            entity.CreatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            AddProjectParameters(command, entity);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Project entity)
        {
            var query = @"
                UPDATE Projects 
                SET Name = @Name, 
                    Description = @Description, 
                    StartDate = @StartDate, 
                    EndDate = @EndDate, 
                    Status = @Status, 
                    Technology = @Technology, 
                    Budget = @Budget, 
                    ClientName = @ClientName, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            entity.UpdatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            AddProjectParameters(command, entity);
            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Projects WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Projects";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
        {
            var projects = new List<Project>();
            var query = "SELECT * FROM Projects WHERE Status = 'Active'";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                projects.Add(MapProject(reader));
            }
            return projects;
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status)
        {
            var projects = new List<Project>();
            var query = "SELECT * FROM Projects WHERE Status = @Status";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", status);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                projects.Add(MapProject(reader));
            }
            return projects;
        }

        private Project MapProject(SqlDataReader reader)
        {
            return new Project
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Technology = reader.IsDBNull(reader.GetOrdinal("Technology")) ? null : reader.GetString(reader.GetOrdinal("Technology")),
                Budget = reader.GetDecimal(reader.GetOrdinal("Budget")),
                ClientName = reader.IsDBNull(reader.GetOrdinal("ClientName")) ? null : reader.GetString(reader.GetOrdinal("ClientName")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddProjectParameters(SqlCommand command, Project entity)
        {
            command.Parameters.AddWithValue("@Name", entity.Name);
            command.Parameters.AddWithValue("@Description", (object)entity.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", entity.StartDate);
            command.Parameters.AddWithValue("@EndDate", (object)entity.EndDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", entity.Status);
            command.Parameters.AddWithValue("@Technology", (object)entity.Technology ?? DBNull.Value);
            command.Parameters.AddWithValue("@Budget", entity.Budget);
            command.Parameters.AddWithValue("@ClientName", (object)entity.ClientName ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
        }
    }
}