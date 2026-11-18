using StaffManagement.Core.DTOs.SkillModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class SkillService(ISkillRepository skillRepository) : ISkillService
{
    public Task<IReadOnlyList<SkillSet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        skillRepository.GetAllAsync(cancellationToken);

    public async Task<IReadOnlyList<SkillSet>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        (await skillRepository.GetAllAsync(cancellationToken))
            .Where(skill => skill.IsActive)
            .ToList();

    public async Task<SkillCreateViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var skill = await skillRepository.GetByIdAsync(id, cancellationToken);
        return skill is null ? null : new SkillCreateViewModel
        {
            Id = skill.Id,
            SkillName = skill.SkillName,
            IsActive = skill.IsActive
        };
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(SkillCreateViewModel model, CancellationToken cancellationToken = default)
    {
        var name = model.SkillName.Trim();
        if (await skillRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
            return (false, "A skill with this name already exists.");

        await skillRepository.AddAsync(new SkillSet
        {
            SkillName = name,
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(SkillCreateViewModel model, CancellationToken cancellationToken = default)
    {
        var skill = await skillRepository.GetByIdAsync(model.Id, cancellationToken);
        if (skill is null)
            return (false, "Skill was not found.");

        var name = model.SkillName.Trim();
        if (await skillRepository.ExistsByNameAsync(name, model.Id, cancellationToken))
            return (false, "A skill with this name already exists.");

        skill.SkillName = name;
        skill.IsActive = model.IsActive;
        skill.UpdatedAtUtc = DateTime.UtcNow;
        await skillRepository.UpdateAsync(skill, cancellationToken);
        return (true, null);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var skill = await skillRepository.GetByIdAsync(id, cancellationToken);
        if (skill is null)
            return false;

        skill.IsActive = false;
        skill.UpdatedAtUtc = DateTime.UtcNow;
        await skillRepository.UpdateAsync(skill, cancellationToken);
        return true;
    }
}
