using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IEmployeeOtherDetailRepository
{
    Task<EmployeeOtherDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeOtherDetail?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeOtherDetail detail, CancellationToken cancellationToken = default);
    void Remove(EmployeeOtherDetail detail);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
