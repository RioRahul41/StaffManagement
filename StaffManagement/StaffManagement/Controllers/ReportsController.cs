using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement.Core.DTOs.TimesheetModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public sealed class ReportsController(
    IEmployeeService employeeService,
    UserManager<ApplicationIdentity> userManager,
    ITimesheetService timesheetService,
    ILeaveRequestService leaveRequestService) : Controller
{
    public async Task<IActionResult> LeaveReport(CancellationToken cancellationToken)
    {
        const int batchSize = 100;
        var identityUserId = userManager.GetUserId(User) ?? string.Empty;
        var firstPage = await leaveRequestService.GetPagedAsync(
            identityUserId,
            canReviewAssignedRequests: false,
            canViewAllRequests: true,
            pageNumber: 1,
            pageSize: batchSize,
            cancellationToken: cancellationToken);

        var leaveRequests = firstPage.Items.ToList();
        for (var pageNumber = 2; leaveRequests.Count < firstPage.TotalCount; pageNumber++)
        {
            var page = await leaveRequestService.GetPagedAsync(
                identityUserId,
                canReviewAssignedRequests: false,
                canViewAllRequests: true,
                pageNumber,
                batchSize,
                cancellationToken: cancellationToken);
            if (page.Items.Count == 0)
                break;
            leaveRequests.AddRange(page.Items);
        }

        var reportRows = leaveRequests
            .Select(request => new LeaveReportDto
            {
                EmployeeId = request.EmployeeId,
                EmployeeName = request.EmployeeName,
                LeaveType = request.LeaveType,
                FromDate = request.FromDate.Date,
                ToDate = request.ToDate.Date,
                AppliedDays = request.AppliedDays,
                Reason = request.Reason,
                Status = request.Status
            })
            .OrderByDescending(request => request.FromDate)
            .ThenBy(request => request.EmployeeName)
            .ToList();

        ViewBag.ReportEmployees = (await employeeService.GetAllAsync(cancellationToken))
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .ToList();
        ViewBag.ReportLeaveTypes = reportRows
            .Select(request => request.LeaveType)
            .Where(leaveType => !string.IsNullOrWhiteSpace(leaveType))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(leaveType => leaveType)
            .ToList();

        return View(reportRows);
    }

    public async Task<IActionResult> TimesheetLog(CancellationToken cancellationToken)
    {
        var employees = await employeeService.GetAllAsync(cancellationToken);
        var employeesById = employees.ToDictionary(employee => employee.Id);
        var timesheets = await timesheetService.GetAllAsync(
            canManage: true,
            identityUserId: userManager.GetUserId(User) ?? string.Empty,
            cancellationToken: cancellationToken);

        var reportRows = timesheets.Select(timesheet =>
        {
            employeesById.TryGetValue(timesheet.EmployeeId, out var employee);
            return new TimesheetLogReportDto
            {
                EmployeeId = timesheet.EmployeeId,
                EmployeeName = employee is null
                    ? timesheet.EmployeeName
                    : $"{employee.FirstName} {employee.LastName}",
                ProjectName = timesheet.ProjectName,
                TaskName = timesheet.TaskName,
                StartDate = timesheet.StartDate.Date,
                StartTime = timesheet.StartTime,
                EndTime = timesheet.EndTime,
                Status = timesheet.Status
            };
        }).OrderByDescending(row => row.StartDate).ThenBy(row => row.EmployeeName).ToList();

        ViewBag.ReportEmployees = employees
            .OrderBy(employee => employee.FirstName)
            .ThenBy(employee => employee.LastName)
            .ToList();
        return View(reportRows);
    }

    public async Task<IActionResult> Employees(CancellationToken cancellationToken)
    {
        var employees = await employeeService.GetAllAsync(cancellationToken);
        var identityUserIds = employees
            .Where(employee => !string.IsNullOrWhiteSpace(employee.IdentityUserId))
            .Select(employee => employee.IdentityUserId!)
            .Distinct()
            .ToArray();

        var usernamesByIdentityId = await userManager.Users
            .Where(user => identityUserIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => user.UserName ?? user.Email ?? string.Empty,
                cancellationToken: cancellationToken);

        ViewBag.UsernamesByEmployeeId = employees.ToDictionary(
            employee => employee.Id,
            employee => !string.IsNullOrWhiteSpace(employee.IdentityUserId) &&
                        usernamesByIdentityId.TryGetValue(employee.IdentityUserId, out var username)
                ? username
                : employee.Email);

        return View(employees);
    }
}
