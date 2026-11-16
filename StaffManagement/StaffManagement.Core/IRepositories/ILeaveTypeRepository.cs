using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface ILeaveTypeRepository
{
    Task<IReadOnlyList<LeaveTypeMaster>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<LeaveTypeMaster?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, int? exceptId = null, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveTypeMaster leaveType, CancellationToken cancellationToken = default);
    void Update(LeaveTypeMaster leaveType);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
