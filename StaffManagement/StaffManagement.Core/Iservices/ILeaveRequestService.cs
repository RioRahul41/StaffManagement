using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Iservices;

public interface ILeaveRequestService
{
    Task<LeaveBalanceDto?> GetOwnBalanceAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveBalanceDto>> GetAllBalancesAsync(CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> UpdateAllowancesAsync(int employeeId, int sickAllowance, int casualAllowance, CancellationToken cancellationToken = default);
    Task<PagedResultDto<LeaveRequestDto>> GetPagedAsync(
        string identityUserId,
        bool canReviewAssignedRequests,
        bool canViewAllRequests,
        int pageNumber,
        int pageSize,
        string? search = null,
        LeaveStatus? status = null,
        LeaveType? leaveType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateLeaveRequestDto model,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default);

    Task<string?> GetDocumentPathAsync(
        int id,
        string identityUserId,
        bool canReviewAssignedRequests,
        bool canViewAllRequests,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> ReviewAsync(
        int id,
        LeaveStatus status,
        string identityUserId,
        bool canReviewAllRequests,
        CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CancelAsync(int id, string identityUserId, CancellationToken cancellationToken = default);
}
