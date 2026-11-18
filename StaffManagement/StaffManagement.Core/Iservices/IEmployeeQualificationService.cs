using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeQualificationService
{
    Task<IReadOnlyList<EmployeeQualification>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeQualification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeQualification qualification, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(EmployeeQualification qualification, CancellationToken cancellationToken = default);
}
