using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Iservices;

public interface ILeaveRequestService
{
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
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> ReviewAsync(
        int id,
        LeaveStatus status,
        string identityUserId,
        CancellationToken cancellationToken = default);
}
