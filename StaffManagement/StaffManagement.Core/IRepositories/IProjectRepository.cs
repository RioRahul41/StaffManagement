using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(int projectId);
    Task<IReadOnlyList<Project>> GetActiveByIdsAsync(IEnumerable<int> projectIds);
    Task<bool> ExistsByNameAsync(string projectName, int? excludedId = null);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
}
