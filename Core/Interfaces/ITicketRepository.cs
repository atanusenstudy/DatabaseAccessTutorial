namespace Core.Interfaces
{
    public interface ITicketRepository : IGenericRepository<Core.Entities.Ticket>
    {
        Task<IEnumerable<Core.Entities.Ticket>> GetTicketsByStatusAsync(string status);
        Task<IEnumerable<Core.Entities.Ticket>> GetTicketsByEmployeeAsync(int employeeId);
        Task<IEnumerable<Core.Entities.Ticket>> GetTicketsByProjectAsync(int projectId);
    }
}