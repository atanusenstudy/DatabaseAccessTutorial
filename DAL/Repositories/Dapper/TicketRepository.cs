using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Dapper;
using System.Data;

namespace DAL.Repositories.Dapper
{
    public class TicketRepository : ITicketRepository
    {
        private readonly DapperContext _context;

        public TicketRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Tickets WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Ticket>(query, new { Id = id });
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            var query = "SELECT * FROM Tickets";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Ticket>(query);
        }

        public async Task<int> AddAsync(Ticket entity)
        {
            var query = @"
                INSERT INTO Tickets (Title, Description, Status, Priority, CreatedDate, DueDate, AssignedTo, ProjectId, Resolution, CreatedAt)
                VALUES (@Title, @Description, @Status, @Priority, @CreatedDate, @DueDate, @AssignedTo, @ProjectId, @Resolution, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            entity.CreatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, entity);
        }

        public async Task<bool> UpdateAsync(Ticket entity)
        {
            var query = @"
                UPDATE Tickets 
                SET Title = @Title, 
                    Description = @Description, 
                    Status = @Status, 
                    Priority = @Priority, 
                    DueDate = @DueDate, 
                    AssignedTo = @AssignedTo, 
                    ProjectId = @ProjectId, 
                    Resolution = @Resolution, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            entity.UpdatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Tickets WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Tickets";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status)
        {
            var query = "SELECT * FROM Tickets WHERE Status = @Status";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Ticket>(query, new { Status = status });
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEmployeeAsync(int employeeId)
        {
            var query = "SELECT * FROM Tickets WHERE AssignedTo = @EmployeeId";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Ticket>(query, new { EmployeeId = employeeId });
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByProjectAsync(int projectId)
        {
            var query = "SELECT * FROM Tickets WHERE ProjectId = @ProjectId";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Ticket>(query, new { ProjectId = projectId });
        }
    }
}