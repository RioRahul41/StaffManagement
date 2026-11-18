using StaffManagement.Model.Entities;
namespace StaffManagement.Core.IRepositories;
public interface IEmployeeExperienceRepository
{
    Task<IReadOnlyList<EmployeeExperience>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeExperience?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeExperience?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeExperience experience, CancellationToken cancellationToken = default);
    void Remove(EmployeeExperience experience);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
