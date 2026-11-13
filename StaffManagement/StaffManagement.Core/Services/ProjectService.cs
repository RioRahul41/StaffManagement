using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    public Task<IReadOnlyList<Project>> GetAllAsync() => projectRepository.GetAllAsync();

    public Task<Project?> GetByIdAsync(int projectId) => projectRepository.GetByIdAsync(projectId);

    public async Task<(bool Succeeded, string? Error)> CreateAsync(Project project)
    {
        project.ProjectName = project.ProjectName.Trim();
        if (await projectRepository.ExistsByNameAsync(project.ProjectName))
        {
            return (false, "A project with this name already exists.");
        }

        if (project.StartDate.HasValue && project.EndDate.HasValue && project.EndDate < project.StartDate)
        {
            return (false, "End date must be on or after the start date.");
        }

        project.CreatedDate = DateTime.UtcNow;
        project.UpdatedDate = null;
        project.IsActive = true;
        await projectRepository.AddAsync(project);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(Project project)
    {
        project.ProjectName = project.ProjectName.Trim();
        if (await projectRepository.ExistsByNameAsync(project.ProjectName, project.ProjectId))
        {
            return (false, "A project with this name already exists.");
        }

        if (project.StartDate.HasValue && project.EndDate.HasValue && project.EndDate < project.StartDate)
        {
            return (false, "End date must be on or after the start date.");
        }

        project.UpdatedDate = DateTime.UtcNow;
        await projectRepository.UpdateAsync(project);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int projectId)
    {
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project is null) return false;
        await projectRepository.DeleteAsync(project);
        return true;
    }
}
