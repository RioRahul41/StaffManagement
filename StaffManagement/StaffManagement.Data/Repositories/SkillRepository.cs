using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class SkillRepository(ApplicationDbContext context) : ISkillRepository
{
    public async Task<IReadOnlyList<SkillSet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.SkillSets
            .AsNoTracking()
            .Where(skill => !skill.IsDeleted)
            .OrderBy(skill => !skill.IsActive)
            .ThenBy(skill => skill.SkillName)
            .ToListAsync(cancellationToken);

    public Task<SkillSet?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default) =>
        context.SkillSets.FirstOrDefaultAsync(
            skill => skill.Id == skillId && !skill.IsDeleted,
            cancellationToken);

    public Task<bool> ExistsByNameAsync(string skillName, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = skillName.Trim().ToUpper();
        return context.SkillSets.AnyAsync(skill =>
            !skill.IsDeleted
            && skill.SkillName.ToUpper() == normalizedName
            && (!excludedId.HasValue || skill.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task AddAsync(SkillSet skill, CancellationToken cancellationToken = default)
    {
        await context.SkillSets.AddAsync(skill, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SkillSet skill, CancellationToken cancellationToken = default)
    {
        context.SkillSets.Update(skill);
        await context.SaveChangesAsync(cancellationToken);
    }
}
