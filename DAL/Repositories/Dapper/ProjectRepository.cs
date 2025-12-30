using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Dapper;
using System.Data;

namespace DAL.Repositories.Dapper
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DapperContext _context;

        public ProjectRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Project> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Projects WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Project>(query, new { Id = id });
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            var query = "SELECT * FROM Projects";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Project>(query);
        }

        public async Task<int> AddAsync(Project entity)
        {
            var query = @"
                INSERT INTO Projects (Name, Description, StartDate, EndDate, Status, Technology, Budget, ClientName, CreatedAt)
                VALUES (@Name, @Description, @StartDate, @EndDate, @Status, @Technology, @Budget, @ClientName, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            entity.CreatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, entity);
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
            var rowsAffected = await connection.ExecuteAsync(query, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Projects WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Projects";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query);
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
        {
            var query = "SELECT * FROM Projects WHERE Status = 'Active'";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Project>(query);
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status)
        {
            var query = "SELECT * FROM Projects WHERE Status = @Status";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Project>(query, new { Status = status });
        }
    }
}