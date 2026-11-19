using StaffManagement.Core.DTOs.EmployeeModel;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeOtherDetailService
{
    Task<EmployeeOtherDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeOtherDetailDto detail, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
