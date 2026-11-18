using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IEmployeeProfileRepository
{
    Task<IReadOnlyList<EmployeeProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeProfile?> GetByEmployeeIdAsync(int employeeId, bool asNoTracking = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeOtherDetail>> GetOtherDetailsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeAddressInformation?> GetAddressInformationByEmployeeIdAsync(int employeeId, bool asNoTracking = false, CancellationToken cancellationToken = default);
    Task AddAddressInformationAsync(EmployeeAddressInformation address, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<bool> EmployeeHasProfileAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> IsActiveEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> EmployeeBelongsToIdentityUserAsync(int employeeId, string identityUserId, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeProfile profile, CancellationToken cancellationToken = default);
    void Remove(EmployeeProfile profile);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
