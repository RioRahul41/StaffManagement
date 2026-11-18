using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Iservices;
public interface IEmployeeBankDetailService
{
    Task<EmployeeBankDetail?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> SaveAsync(EmployeeBankDetail detail, CancellationToken cancellationToken = default);
}
