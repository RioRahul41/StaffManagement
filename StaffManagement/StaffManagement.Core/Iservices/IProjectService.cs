using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(int projectId);
    Task<(bool Succeeded, string? Error)> CreateAsync(Project project);
    Task<(bool Succeeded, string? Error)> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int projectId);
}
