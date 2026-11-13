using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.IRepositories;

public interface ILeaveRequestRepository
{
    Task<(IReadOnlyList<LeaveRequest> Items, int TotalCount)> GetPagedAsync(
        int? employeeId,
        bool includeAssignedRequests,
        bool includeAllRequests,
        int pageNumber,
        int pageSize,
        string? search = null,
        LeaveStatus? status = null,
        LeaveType? leaveType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default);
    void Update(LeaveRequest request);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
