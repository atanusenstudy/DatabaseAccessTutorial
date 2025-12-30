using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.EntityFramework
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EntityFrameworkDbContext _context;

        public EmployeeRepository(EntityFrameworkDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<int> AddAsync(Employee entity)
        {
            await _context.Employees.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Employee entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Employees.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await GetByIdAsync(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Employees.CountAsync();
        }

        public async Task<Employee> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
        {
            return await _context.Employees
                .Where(e => e.Department == department && e.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .ToListAsync();
        }
    }
}