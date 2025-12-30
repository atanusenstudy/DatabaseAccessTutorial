using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL.Repositories.AdoNet
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AdoNetContext _context;

        public TicketRepository(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Tickets WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapTicket(reader);
            }
            return null!;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            var tickets = new List<Ticket>();
            var query = "SELECT * FROM Tickets";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tickets.Add(MapTicket(reader));
            }
            return tickets;
        }

        public async Task<int> AddAsync(Ticket entity)
        {
            var query = @"
                INSERT INTO Tickets (Title, Description, Status, Priority, CreatedDate, DueDate, AssignedTo, ProjectId, Resolution, CreatedAt)
                VALUES (@Title, @Description, @Status, @Priority, @CreatedDate, @DueDate, @AssignedTo, @ProjectId, @Resolution, @CreatedAt);
                SELECT SCOPE_IDENTITY()";

            entity.CreatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            AddTicketParameters(command, entity);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
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
            using var command = new SqlCommand(query, connection);
            AddTicketParameters(command, entity);
            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Tickets WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Tickets";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status)
        {
            var tickets = new List<Ticket>();
            var query = "SELECT * FROM Tickets WHERE Status = @Status";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Status", status);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tickets.Add(MapTicket(reader));
            }
            return tickets;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEmployeeAsync(int employeeId)
        {
            var tickets = new List<Ticket>();
            var query = "SELECT * FROM Tickets WHERE AssignedTo = @EmployeeId";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", employeeId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tickets.Add(MapTicket(reader));
            }
            return tickets;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByProjectAsync(int projectId)
        {
            var tickets = new List<Ticket>();
            var query = "SELECT * FROM Tickets WHERE ProjectId = @ProjectId";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ProjectId", projectId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tickets.Add(MapTicket(reader));
            }
            return tickets;
        }

        private Ticket MapTicket(SqlDataReader reader)
        {
            return new Ticket
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Priority = reader.GetString(reader.GetOrdinal("Priority")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DueDate")),
                ResolvedDate = reader.IsDBNull(reader.GetOrdinal("ResolvedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ResolvedDate")),
                AssignedTo = reader.IsDBNull(reader.GetOrdinal("AssignedTo")) ? null : reader.GetInt32(reader.GetOrdinal("AssignedTo")),
                ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                Resolution = reader.IsDBNull(reader.GetOrdinal("Resolution")) ? null : reader.GetString(reader.GetOrdinal("Resolution")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddTicketParameters(SqlCommand command, Ticket entity)
        {
            command.Parameters.AddWithValue("@Title", entity.Title);
            command.Parameters.AddWithValue("@Description", (object)entity.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", entity.Status);
            command.Parameters.AddWithValue("@Priority", entity.Priority);
            command.Parameters.AddWithValue("@CreatedDate", entity.CreatedDate);
            command.Parameters.AddWithValue("@DueDate", (object)entity.DueDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@AssignedTo", (object)entity.AssignedTo ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProjectId", entity.ProjectId);
            command.Parameters.AddWithValue("@Resolution", (object)entity.Resolution ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
        }
    }
}