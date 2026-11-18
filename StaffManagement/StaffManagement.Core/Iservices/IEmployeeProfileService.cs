using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeProfileService
{
    Task<IReadOnlyList<EmployeeProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeProfile?> GetByEmployeeIdAsync(int employeeId, bool asNoTracking = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeOtherDetail>> GetOtherDetailsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<int?> GetOwnEmployeeIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<bool> EmployeeHasProfileAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> IsActiveEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> CanEditAsync(int employeeId, string identityUserId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeProfile profile, CancellationToken cancellationToken = default);
    Task<bool> UpdateSectionAsync(int employeeId, EmployeeProfile submitted, string section, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
