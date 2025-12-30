namespace Core.Interfaces
{
    public interface IProjectRepository : IGenericRepository<Core.Entities.Project>
    {
        Task<IEnumerable<Core.Entities.Project>> GetActiveProjectsAsync();
        Task<IEnumerable<Core.Entities.Project>> GetProjectsByStatusAsync(string status);
    }
}