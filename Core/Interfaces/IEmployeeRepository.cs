namespace Core.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Core.Entities.Employee>
    {
        Task<Core.Entities.Employee> GetByEmailAsync(string email);
        Task<IEnumerable<Core.Entities.Employee>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Core.Entities.Employee>> GetActiveEmployeesAsync();
    }
}