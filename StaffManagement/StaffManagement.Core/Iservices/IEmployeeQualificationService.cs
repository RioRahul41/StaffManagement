using StaffManagement.Core.DTOs.EmployeeModel;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeQualificationService
{
    Task<IReadOnlyList<EmployeeQualificationDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeQualificationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeQualificationDto qualification, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(EmployeeQualificationDto qualification, CancellationToken cancellationToken = default);
}
