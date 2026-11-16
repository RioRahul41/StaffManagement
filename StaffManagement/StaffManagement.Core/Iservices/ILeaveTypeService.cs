using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface ILeaveTypeService
{
    Task<IReadOnlyList<LeaveTypeMaster>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<LeaveTypeMaster?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(LeaveTypeMaster leaveType, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> UpdateAsync(LeaveTypeMaster leaveType, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
