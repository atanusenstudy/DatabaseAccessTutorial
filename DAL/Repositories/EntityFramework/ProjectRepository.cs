using Core.Interfaces;
using Core.Entities;
using DAL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.EntityFramework
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly EntityFrameworkDbContext _context;

        public ProjectRepository(EntityFrameworkDbContext context)
        {
            _context = context;
        }

        public async Task<Project> GetByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<int> AddAsync(Project entity)
        {
            await _context.Projects.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Project entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Projects.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await GetByIdAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Projects.CountAsync();
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
        {
            return await _context.Projects
                .Where(p => p.Status == "Active")
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status)
        {
            return await _context.Projects
                .Where(p => p.Status == status)
                .ToListAsync();
        }
    }
}