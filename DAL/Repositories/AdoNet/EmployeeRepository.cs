using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL.Repositories.AdoNet
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AdoNetContext _context;

        public EmployeeRepository(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Employees WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapEmployee(reader);
            }
            return null!;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = new List<Employee>();
            var query = "SELECT * FROM Employees";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(MapEmployee(reader));
            }
            return employees;
        }

        public async Task<int> AddAsync(Employee entity)
        {
            var query = @"
                INSERT INTO Employees (Name, Department, Email, HireDate, Phone, Address, Salary, IsActive, CreatedAt)
                VALUES (@Name, @Department, @Email, @HireDate, @Phone, @Address, @Salary, @IsActive, @CreatedAt);
                SELECT SCOPE_IDENTITY()";

            entity.CreatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            AddEmployeeParameters(command, entity);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Employee entity)
        {
            var query = @"
                UPDATE Employees 
                SET Name = @Name, 
                    Department = @Department, 
                    Email = @Email, 
                    Phone = @Phone, 
                    Address = @Address, 
                    Salary = @Salary, 
                    IsActive = @IsActive, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            entity.UpdatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            AddEmployeeParameters(command, entity);
            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt ?? (object)DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Employees WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Employees";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Employee> GetByEmailAsync(string email)
        {
            var query = "SELECT * FROM Employees WHERE Email = @Email";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapEmployee(reader);
            }
            return null!;
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
        {
            var employees = new List<Employee>();
            var query = "SELECT * FROM Employees WHERE Department = @Department AND IsActive = 1";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Department", department);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(MapEmployee(reader));
            }
            return employees;
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            var employees = new List<Employee>();
            var query = "SELECT * FROM Employees WHERE IsActive = 1";

            using var connection = _context.CreateConnection();
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(MapEmployee(reader));
            }
            return employees;
        }

        private Employee MapEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Department = reader.GetString(reader.GetOrdinal("Department")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private void AddEmployeeParameters(SqlCommand command, Employee entity)
        {
            command.Parameters.AddWithValue("@Name", entity.Name);
            command.Parameters.AddWithValue("@Department", entity.Department);
            command.Parameters.AddWithValue("@Email", entity.Email);
            command.Parameters.AddWithValue("@HireDate", entity.HireDate);
            command.Parameters.AddWithValue("@Phone", (object)entity.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object)entity.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("@Salary", entity.Salary);
            command.Parameters.AddWithValue("@IsActive", entity.IsActive);
            command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
        }
    }
}