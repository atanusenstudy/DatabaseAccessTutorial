using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Dapper;
using System.Data;

namespace DAL.Repositories.Dapper
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DapperContext _context;

        public EmployeeRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Employees WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Id = id });
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var query = "SELECT * FROM Employees";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Employee>(query);
        }

        public async Task<int> AddAsync(Employee entity)
        {
            var query = @"
                INSERT INTO Employees (Name, Department, Email, HireDate, Phone, Address, Salary, IsActive, CreatedAt)
                VALUES (@Name, @Department, @Email, @HireDate, @Phone, @Address, @Salary, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            entity.CreatedAt = DateTime.UtcNow;

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, entity);
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
            var rowsAffected = await connection.ExecuteAsync(query, entity);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Employees WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<int> CountAsync()
        {
            var query = "SELECT COUNT(*) FROM Employees";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query);
        }

        public async Task<Employee> GetByEmailAsync(string email)
        {
            var query = "SELECT * FROM Employees WHERE Email = @Email";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Email = email });
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
        {
            var query = "SELECT * FROM Employees WHERE Department = @Department AND IsActive = 1";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Employee>(query, new { Department = department });
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            var query = "SELECT * FROM Employees WHERE IsActive = 1";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Employee>(query);
        }
    }
}