using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Iservices;
public interface IEmployeeExperienceService
{
    Task<IReadOnlyList<EmployeeExperience>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeExperience?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> CreateAsync(EmployeeExperience experience, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(EmployeeExperience experience, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
