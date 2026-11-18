using StaffManagement.Model.Entities;
namespace StaffManagement.Core.IRepositories;
public interface IEmployeeBankDetailRepository
{
    Task<EmployeeBankDetail?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeBankDetail?> GetTrackedByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeBankDetail detail, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
