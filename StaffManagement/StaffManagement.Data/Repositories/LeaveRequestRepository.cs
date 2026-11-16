using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Data.Repositories;

public sealed class LeaveRequestRepository(ApplicationDbContext dbContext) : ILeaveRequestRepository
{
    public async Task<(IReadOnlyList<LeaveRequest> Items, int TotalCount)> GetPagedAsync(
        int? employeeId,
        bool includeAssignedRequests,
        bool includeAllRequests,
        int pageNumber,
        int pageSize,
        string? search = null,
        LeaveStatus? status = null,
        string? leaveType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.LeaveRequests
            .AsNoTracking()
            .Include(request => request.Employee)
            .Where(request => !request.Employee.EmpIsDeleted);

        if (!includeAllRequests)
        {
            if (!employeeId.HasValue)
                return (Array.Empty<LeaveRequest>(), 0);

            var currentEmployeeId = employeeId.Value;
            query = query.Where(request =>
                request.EmployeeId == currentEmployeeId ||
                (includeAssignedRequests && request.Employee.ReportingPersonId == currentEmployeeId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(request =>
                request.Employee.FirstName.Contains(searchTerm) ||
                request.Employee.LastName.Contains(searchTerm) ||
                (request.Employee.FirstName + " " + request.Employee.LastName).Contains(searchTerm) ||
                request.Reason.Contains(searchTerm));
        }

        if (status.HasValue)
            query = query.Where(request => request.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(leaveType))
        {
            var legacyLeaveType = Enum.TryParse<LeaveType>(leaveType, true, out var parsedLeaveType)
                ? parsedLeaveType
                : (LeaveType?)null;
            query = legacyLeaveType.HasValue
                ? query.Where(request => request.LeaveTypeName == leaveType || (request.LeaveTypeName == null && request.LeaveType == legacyLeaveType.Value))
                : query.Where(request => request.LeaveTypeName == leaveType);
        }

        if (fromDate.HasValue)
            query = query.Where(request => request.ToDate >= fromDate.Value.Date);

        if (toDate.HasValue)
        {
            var endDate = toDate.Value.Date;
            query = query.Where(request => request.FromDate <= endDate);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(request => request.Status)
            .ThenByDescending(request => request.FromDate)
            .ThenByDescending(request => request.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.LeaveRequests
            .Include(request => request.Employee)
            .FirstOrDefaultAsync(
                request => request.Id == id && !request.Employee.EmpIsDeleted,
                cancellationToken);

    public async Task<(int SickUsed, int SickPending, int CasualUsed, int CasualPending)> GetLeaveTotalsAsync(
        int employeeId, CancellationToken cancellationToken = default)
    {
        var totals = await dbContext.LeaveRequests.AsNoTracking()
            .Where(request => request.EmployeeId == employeeId &&
                (request.LeaveType == LeaveType.Sick || request.LeaveType == LeaveType.Casual))
            .GroupBy(request => new { request.LeaveType, request.Status })
            .Select(group => new { group.Key.LeaveType, group.Key.Status, Days = group.Sum(request => request.AppliedDays) })
            .ToListAsync(cancellationToken);

        int Sum(LeaveType type, LeaveStatus status) => totals
            .Where(item => item.LeaveType == type && item.Status == status)
            .Sum(item => item.Days);

        return (Sum(LeaveType.Sick, LeaveStatus.Approved), Sum(LeaveType.Sick, LeaveStatus.Pending),
            Sum(LeaveType.Casual, LeaveStatus.Approved), Sum(LeaveType.Casual, LeaveStatus.Pending));
    }

    public async Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default) =>
        await dbContext.LeaveRequests.AddAsync(request, cancellationToken);

    public void Update(LeaveRequest request) => dbContext.LeaveRequests.Update(request);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
