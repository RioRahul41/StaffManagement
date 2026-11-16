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
    public async Task<LeaveBalanceDto?> GetOwnBalanceAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (employee is null)
            return null;
        var totals = await leaveRepository.GetLeaveTotalsAsync(employee.Id, cancellationToken);
        return new LeaveBalanceDto
        {
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Email = employee.Email,
            SickAllowance = employee.SickLeaveAllowance,
            SickUsed = totals.SickUsed,
            SickPending = totals.SickPending,
            CasualAllowance = employee.CasualLeaveAllowance,
            CasualUsed = totals.CasualUsed,
            CasualPending = totals.CasualPending
        };
    }

    public async Task<IReadOnlyList<LeaveBalanceDto>> GetAllBalancesAsync(CancellationToken cancellationToken = default)
    {
        var employees = await employeeRepository.GetAllAsync(cancellationToken);
        var results = new List<LeaveBalanceDto>(employees.Count);
        foreach (var employee in employees)
        {
            var totals = await leaveRepository.GetLeaveTotalsAsync(employee.Id, cancellationToken);
            results.Add(new LeaveBalanceDto
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Email = employee.Email,
                SickAllowance = employee.SickLeaveAllowance,
                SickUsed = totals.SickUsed,
                SickPending = totals.SickPending,
                CasualAllowance = employee.CasualLeaveAllowance,
                CasualUsed = totals.CasualUsed,
                CasualPending = totals.CasualPending
            });
        }
        return results;
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAllowancesAsync(
        int employeeId, int sickAllowance, int casualAllowance, CancellationToken cancellationToken = default)
    {
        if (sickAllowance is < 0 or > 365 || casualAllowance is < 0 or > 365)
            return (false, "Leave allowances must be between 0 and 365 days.");
        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            return (false, "Employee was not found.");
        employee.SickLeaveAllowance = sickAllowance;
        employee.CasualLeaveAllowance = casualAllowance;
        employee.UpdatedTime = DateTime.UtcNow;
        employeeRepository.UpdateLeaveAllowances(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

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
                (canViewAllRequests || (request.EmployeeId != employee.Id && request.Employee.ReportingPersonId == employee.Id)) &&
                request.Status == LeaveStatus.Pending,
                request.Employee.IdentityUserId == identityUserId && request.Status == LeaveStatus.Pending)).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<(bool Succeeded, string? Error)> CancelAsync(int id, string identityUserId, CancellationToken cancellationToken = default)
    {
        var request = await leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request is null || request.Employee.IdentityUserId != identityUserId)
            return (false, "Leave request was not found.");
        if (request.Status != LeaveStatus.Pending)
            return (false, "Only pending leave requests can be cancelled.");
        request.Status = LeaveStatus.Cancelled;
        request.UpdatedTime = DateTime.UtcNow;
        leaveRepository.Update(request);
        await leaveRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
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

        var totals = await leaveRepository.GetLeaveTotalsAsync(employee.Id, cancellationToken);
        var allowance = model.LeaveType == LeaveType.Sick ? employee.SickLeaveAllowance :
            model.LeaveType == LeaveType.Casual ? employee.CasualLeaveAllowance : int.MaxValue;
        var used = model.LeaveType == LeaveType.Sick ? totals.SickUsed :
            model.LeaveType == LeaveType.Casual ? totals.CasualUsed : 0;
        var pending = model.LeaveType == LeaveType.Sick ? totals.SickPending :
            model.LeaveType == LeaveType.Casual ? totals.CasualPending : 0;
        if (appliedDays > allowance - used - pending)
            return (false, $"This request exceeds your available {model.LeaveType.ToString().ToLowerInvariant()} leave balance.");

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
        bool canReviewAllRequests,
        CancellationToken cancellationToken = default)
    {
        if (status is not LeaveStatus.Approved and not LeaveStatus.Rejected)
            return (false, "Choose approve or reject for this leave request.");

        var reportingPerson = canReviewAllRequests
            ? null
            : await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (!canReviewAllRequests && reportingPerson is null)
            return (false, "Your employee profile could not be found.");

        var request = await leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request is null)
            return (false, "Leave request was not found.");

        if (!canReviewAllRequests && (request.EmployeeId == reportingPerson!.Id || request.Employee.ReportingPersonId != reportingPerson.Id))
            return (false, "Only the employee's assigned reporting person can review this leave request.");

        if (request.Status != LeaveStatus.Pending)
            return (false, "This leave request has already been reviewed.");

        if (status == LeaveStatus.Approved && request.LeaveType is LeaveType.Sick or LeaveType.Casual)
        {
            var totals = await leaveRepository.GetLeaveTotalsAsync(request.EmployeeId, cancellationToken);
            var allowance = request.LeaveType == LeaveType.Sick ? request.Employee.SickLeaveAllowance : request.Employee.CasualLeaveAllowance;
            var used = request.LeaveType == LeaveType.Sick ? totals.SickUsed : totals.CasualUsed;
            var pending = request.LeaveType == LeaveType.Sick ? totals.SickPending : totals.CasualPending;
            var otherPending = Math.Max(0, pending - request.AppliedDays);
            if (request.AppliedDays > allowance - used - otherPending)
                return (false, "This employee no longer has enough leave balance to approve this request.");
        }

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
        bool canViewAllRequests,
        CancellationToken cancellationToken = default)
    {
        var request = await leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request is null || string.IsNullOrWhiteSpace(request.DocumentPath))
            return null;

        var isOwner = request.Employee.IdentityUserId == identityUserId;
        var isAssignedReviewer = canReviewAssignedRequests &&
            request.Employee.ReportingPersonId.HasValue &&
            await IsCurrentEmployeeAsync(request.Employee.ReportingPersonId.Value, identityUserId, cancellationToken);

        return isOwner || isAssignedReviewer || canViewAllRequests ? request.DocumentPath : null;
    }

    private async Task<bool> IsCurrentEmployeeAsync(
        int employeeId,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        return employee?.Id == employeeId;
    }

    private static LeaveRequestDto Map(LeaveRequest request, bool canReview, bool canCancel) => new()
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
        CanReview = canReview,
        CanCancel = canCancel
    };
}
