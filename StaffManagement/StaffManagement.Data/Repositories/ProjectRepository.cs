using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public class ProjectRepository(ApplicationDbContext context) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        return await context.Projects.AsNoTracking()
            .Where(project => project.IsActive)
            .OrderBy(project => project.ProjectName)
            .ToListAsync();
    }

    public Task<Project?> GetByIdAsync(int projectId)
    {
        return context.Projects.FirstOrDefaultAsync(project =>
            project.ProjectId == projectId && project.IsActive);
    }

    public async Task<IReadOnlyList<Project>> GetActiveByIdsAsync(IEnumerable<int> projectIds)
    {
        var ids = projectIds.Distinct().ToArray();
        return await context.Projects.AsNoTracking()
            .Where(project => project.IsActive && ids.Contains(project.ProjectId))
            .ToListAsync();
    }

    public Task<bool> ExistsByNameAsync(string projectName, int? excludedId = null)
    {
        var normalizedName = projectName.Trim().ToLower();
        return context.Projects.AnyAsync(project => project.IsActive &&
            project.ProjectName.ToLower() == normalizedName &&
            (!excludedId.HasValue || project.ProjectId != excludedId.Value));
    }

    public async Task AddAsync(Project project)
    {
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        project.IsActive = false;
        project.UpdatedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
