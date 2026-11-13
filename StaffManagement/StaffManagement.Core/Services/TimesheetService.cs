using StaffManagement.Core.DTOs.TimesheetModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Services;

public sealed class TimesheetService(
    ITimesheetRepository timesheetRepository,
    IEmployeeRepository employeeRepository,
    IProjectRepository projectRepository) : ITimesheetService
{
    public async Task<IReadOnlyList<TimesheetDto>> GetAllAsync(
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var timesheets = canManage
            ? await timesheetRepository.GetAllAsync(cancellationToken)
            : await GetOwnTimesheetsAsync(identityUserId, cancellationToken);

        return timesheets.Select(Map).ToList();
    }

    public async Task<TimesheetDto?> GetByIdAsync(
        int id,
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var timesheet = await timesheetRepository.GetByIdAsync(id, cancellationToken);
        if (timesheet is null || !await CanAccessAsync(timesheet, canManage, identityUserId, cancellationToken))
            return null;

        return Map(timesheet);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateTimesheetDto model,
        bool canManage,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateSchedule(model.StartDate, model.StartTime, model.EndTime);
        if (validationError is not null)
            return (false, validationError);

        var employee = await GetTargetEmployeeAsync(
            model.EmployeeId,
            canManage,
            identityUserId,
            cancellationToken);

        if (employee is null)
            return (false, "The selected employee was not found or you are not allowed to use that employee.");

        var project = model.ProjectId.HasValue
            ? await projectRepository.GetByIdAsync(model.ProjectId.Value)
            : null;
        if (project is null)
            return (false, "Select an active project.");
        if (!canManage && !employee.ProjectAssignments.Any(assignment => assignment.ProjectId == project.ProjectId))
            return (false, "You can only record time for a project assigned to you.");

        var timesheet = new Timesheet
        {
            EmployeeId = employee.Id,
            ProjectId = project.ProjectId,
            ProjectName = project.ProjectName,
            TaskName = model.TaskName.Trim(),
            Description = model.Description.Trim(),
            StartDate = model.StartDate.Date,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Status = model.Status,
            DocumentPath = documentPath,
            CreatedTime = DateTime.UtcNow
        };

        await timesheetRepository.AddAsync(timesheet, cancellationToken);
        await timesheetRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(
        TimesheetDto model,
        bool canManage,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateSchedule(model.StartDate, model.StartTime, model.EndTime);
        if (validationError is not null)
            return (false, validationError);

        var timesheet = await timesheetRepository.GetByIdAsync(model.Id, cancellationToken);
        if (timesheet is null)
            return (false, "Timesheet was not found.");

        if (!await CanAccessAsync(timesheet, canManage, identityUserId, cancellationToken))
            return (false, "You are not allowed to manage this timesheet.");

        var project = model.ProjectId.HasValue
            ? await projectRepository.GetByIdAsync(model.ProjectId.Value)
            : null;
        if (project is null)
            return (false, "Select an active project.");
        if (!canManage && !timesheet.Employee.ProjectAssignments.Any(assignment => assignment.ProjectId == project.ProjectId))
            return (false, "You can only record time for a project assigned to you.");

        timesheet.ProjectId = project.ProjectId;
        timesheet.ProjectName = project.ProjectName;
        timesheet.TaskName = model.TaskName.Trim();
        timesheet.Description = model.Description.Trim();
        timesheet.StartDate = model.StartDate.Date;
        timesheet.StartTime = model.StartTime;
        timesheet.EndTime = model.EndTime;
        timesheet.Status = model.Status;
        if (documentPath is not null)
            timesheet.DocumentPath = documentPath;
        timesheet.UpdatedTime = DateTime.UtcNow;

        timesheetRepository.Update(timesheet);
        await timesheetRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteAsync(
        int id,
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var timesheet = await timesheetRepository.GetByIdAsync(id, cancellationToken);
        if (timesheet is null)
            return (false, "Timesheet was not found.");

        if (!await CanAccessAsync(timesheet, canManage, identityUserId, cancellationToken))
            return (false, "You are not allowed to manage this timesheet.");

        timesheetRepository.Remove(timesheet);
        await timesheetRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<IReadOnlyList<Timesheet>> GetOwnTimesheetsAsync(
        string identityUserId,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        return employee is null
            ? Array.Empty<Timesheet>()
            : await timesheetRepository.GetByEmployeeIdAsync(employee.Id, cancellationToken);
    }

    private async Task<Employee?> GetTargetEmployeeAsync(
        int? requestedEmployeeId,
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        if (canManage && requestedEmployeeId.HasValue)
            return await employeeRepository.GetByIdAsync(requestedEmployeeId.Value, cancellationToken);

        return await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
    }

    private async Task<bool> CanAccessAsync(
        Timesheet timesheet,
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        if (canManage)
            return true;

        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        return employee is not null && employee.Id == timesheet.EmployeeId;
    }

    private static TimesheetDto Map(Timesheet timesheet)
    {
        var model = new TimesheetDto
        {
            Id = timesheet.Id,
            EmployeeId = timesheet.EmployeeId,
            EmployeeName = $"{timesheet.Employee.FirstName} {timesheet.Employee.LastName}",
            ProjectId = timesheet.ProjectId,
            ProjectName = timesheet.Project?.ProjectName ?? timesheet.ProjectName,
            TaskName = timesheet.TaskName,
            Description = timesheet.Description,
            StartDate = timesheet.StartDate,
            StartTime = timesheet.StartTime,
            EndTime = timesheet.EndTime,
            Status = timesheet.Status,
            DocumentPath = timesheet.DocumentPath
        };
        model.SetTimeParts();
        return model;
    }

    private static string? ValidateSchedule(DateTime startDate, TimeSpan startTime, TimeSpan endTime)
    {
        if (startDate.Date > DateTime.Today)
            return "Start date cannot be in the future.";

        if (endTime <= startTime)
            return "End time must be later than start time.";

        return null;
    }
}
