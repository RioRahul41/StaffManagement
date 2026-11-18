using StaffManagement.Core.DTOs.SkillModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface ISkillService
{
    Task<IReadOnlyList<SkillSet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SkillSet>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<SkillCreateViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(SkillCreateViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> UpdateAsync(SkillCreateViewModel model, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
