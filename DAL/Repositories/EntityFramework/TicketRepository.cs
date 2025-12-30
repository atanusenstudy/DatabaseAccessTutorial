using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.EntityFramework
{
    public class TicketRepository : ITicketRepository
    {
        private readonly EntityFrameworkDbContext _context;

        public TicketRepository(EntityFrameworkDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            return await _context.Tickets.FindAsync(id);
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets.ToListAsync();
        }

        public async Task<int> AddAsync(Ticket entity)
        {
            await _context.Tickets.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Ticket entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Tickets.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await GetByIdAsync(id);
            if (ticket == null) return false;

            _context.Tickets.Remove(ticket);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Tickets.CountAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status)
        {
            return await _context.Tickets
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByEmployeeAsync(int employeeId)
        {
            return await _context.Tickets
                .Where(t => t.AssignedTo == employeeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByProjectAsync(int projectId)
        {
            return await _context.Tickets
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }
    }
}