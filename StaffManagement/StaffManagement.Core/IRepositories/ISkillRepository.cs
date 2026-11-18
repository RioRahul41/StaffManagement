using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface ISkillRepository
{
    Task<IReadOnlyList<SkillSet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SkillSet?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string skillName, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddAsync(SkillSet skill, CancellationToken cancellationToken = default);
    Task UpdateAsync(SkillSet skill, CancellationToken cancellationToken = default);
}
