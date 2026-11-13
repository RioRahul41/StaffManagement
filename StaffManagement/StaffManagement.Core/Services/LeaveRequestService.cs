using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Services;

public sealed class LeaveRequestService(
    ILeaveRequestRepository leaveRepository,
    IEmployeeRepository employeeRepository) : ILeaveRequestService
{
    public async Task<PagedResultDto<LeaveRequestDto>> GetPagedAsync(
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
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (employee is null && !canViewAllRequests)
            return new PagedResultDto<LeaveRequestDto> { PageNumber = pageNumber, PageSize = pageSize };

        var result = await leaveRepository.GetPagedAsync(
            employee?.Id,
            canReviewAssignedRequests,
            canViewAllRequests,
            pageNumber,
            pageSize,
            search,
            status,
            leaveType,
            fromDate,
            toDate,
            cancellationToken);

        return new PagedResultDto<LeaveRequestDto>
        {
            Items = result.Items.Select(request => Map(
                request,
                canReviewAssignedRequests && employee is not null &&
                request.EmployeeId != employee.Id &&
                request.Employee.ReportingPersonId == employee.Id &&
                request.Status == LeaveStatus.Pending)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateLeaveRequestDto model,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(model.LeaveType))
            return (false, "Select a valid leave type.");

        var fromDate = model.FromDate.Date;
        var toDate = model.ToDate.Date;
        if (model.LeaveType == LeaveType.Sick && (fromDate > DateTime.Today || toDate > DateTime.Today))
            return (false, "Sick leave can only be requested for today or a past date.");

        if (model.LeaveType != LeaveType.Sick && (fromDate < DateTime.Today || toDate < DateTime.Today))
            return (false, "Leave can only be applied for today or a future date.");

        if (toDate < fromDate)
            return (false, "To date must be on or after from date.");

        var appliedDays = checked((toDate - fromDate).Days + 1);
        var requiresDocument = model.LeaveType == LeaveType.Sick && appliedDays >= 2;
        if (requiresDocument && string.IsNullOrWhiteSpace(documentPath))
            return (false, "A supporting document is required for sick leave applications of 2 or more days.");

        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (employee is null)
            return (false, "Your employee profile could not be found.");

        if (!employee.ReportingPersonId.HasValue)
            return (false, "No reporting person is assigned to your employee profile. Ask Admin or HR to assign an Admin, HR, or Manager as your reporting person.");

        var request = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = model.LeaveType,
            FromDate = fromDate,
            ToDate = toDate,
            AppliedDays = appliedDays,
            Reason = model.Reason.Trim(),
            Status = LeaveStatus.Pending,
            DocumentPath = requiresDocument ? documentPath : null,
            CreatedTime = DateTime.UtcNow
        };

        await leaveRepository.AddAsync(request, cancellationToken);
        await leaveRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> ReviewAsync(
        int id,
        LeaveStatus status,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        if (status is not LeaveStatus.Approved and not LeaveStatus.Rejected)
            return (false, "Choose approve or reject for this leave request.");

        var reportingPerson = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (reportingPerson is null)
            return (false, "Your employee profile could not be found.");

        var request = await leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request is null)
            return (false, "Leave request was not found.");

        if (request.EmployeeId == reportingPerson.Id || request.Employee.ReportingPersonId != reportingPerson.Id)
            return (false, "Only the employee's assigned reporting person can review this leave request.");

        if (request.Status != LeaveStatus.Pending)
            return (false, "This leave request has already been reviewed.");

        request.Status = status;
        request.UpdatedTime = DateTime.UtcNow;
        leaveRepository.Update(request);
        await leaveRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<string?> GetDocumentPathAsync(
        int id,
        string identityUserId,
        bool canReviewAssignedRequests,
        CancellationToken cancellationToken = default)
    {
        var request = await leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request is null || string.IsNullOrWhiteSpace(request.DocumentPath))
            return null;

        var isOwner = request.Employee.IdentityUserId == identityUserId;
        var isAssignedReviewer = canReviewAssignedRequests &&
            request.Employee.ReportingPersonId.HasValue &&
            await IsCurrentEmployeeAsync(request.Employee.ReportingPersonId.Value, identityUserId, cancellationToken);

        return isOwner || isAssignedReviewer ? request.DocumentPath : null;
    }

    private async Task<bool> IsCurrentEmployeeAsync(
        int employeeId,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        return employee?.Id == employeeId;
    }

    private static LeaveRequestDto Map(LeaveRequest request, bool canReview) => new()
    {
        Id = request.Id,
        EmployeeId = request.EmployeeId,
        EmployeeName = $"{request.Employee.FirstName} {request.Employee.LastName}",
        LeaveType = request.LeaveType,
        FromDate = request.FromDate,
        ToDate = request.ToDate,
        AppliedDays = request.AppliedDays,
        Reason = request.Reason,
        Status = request.Status,
        DocumentPath = request.DocumentPath,
        CanReview = canReview
    };
}
