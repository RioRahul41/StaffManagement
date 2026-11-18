using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IEmployeeQualificationRepository
{
    Task<IReadOnlyList<EmployeeQualification>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeQualification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeQualification?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeQualification qualification, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
