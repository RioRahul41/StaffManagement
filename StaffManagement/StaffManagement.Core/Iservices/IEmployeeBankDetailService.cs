using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Core.DTOs.EmployeeModel;
namespace StaffManagement.Core.Iservices;
public interface IEmployeeBankDetailService
{
    Task<BankDetailsDto?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(BankDetailsDto detail, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> UpdateAsync(BankDetailsDto detail, CancellationToken cancellationToken = default);
}
