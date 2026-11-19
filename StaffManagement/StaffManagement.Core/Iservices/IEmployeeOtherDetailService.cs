using StaffManagement.Core.DTOs.EmployeeModel;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeOtherDetailService
{
    Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeOtherDetailDto detail, CancellationToken cancellationToken = default);
}
