using Microsoft.EntityFrameworkCore;
using System.Globalization;
using StaffManagement.Core.DTOs.TimesheetModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Data.Repositories;

public sealed class TimesheetRepository(ApplicationDbContext dbContext) : ITimesheetRepository
{
    public async Task<IReadOnlyList<Timesheet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Timesheets
            .AsNoTracking()
            .Include(timesheet => timesheet.Employee)
                .ThenInclude(employee => employee.ProjectAssignments)
            .Include(timesheet => timesheet.Project)
            .Where(timesheet => !timesheet.Employee.EmpIsDeleted)
            .OrderByDescending(timesheet => timesheet.StartDate)
            .ThenBy(timesheet => timesheet.Employee.LastName)
            .ThenBy(timesheet => timesheet.Employee.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Timesheet> Items, int TotalCount, int FilteredCount)> GetDataTablePageAsync(
        int? employeeId,
        TimesheetDataTableQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var timesheets = dbContext.Timesheets
            .AsNoTracking()
            .Include(timesheet => timesheet.Employee)
            .Include(timesheet => timesheet.Project)
            .Where(timesheet => !timesheet.Employee.EmpIsDeleted);

        if (employeeId.HasValue)
            timesheets = timesheets.Where(timesheet => timesheet.EmployeeId == employeeId.Value);

        var totalCount = await timesheets.CountAsync(cancellationToken);

        if (query.Status.HasValue)
            timesheets = timesheets.Where(timesheet => timesheet.Status == query.Status.Value);

        if (query.DateFrom.HasValue)
            timesheets = timesheets.Where(timesheet => timesheet.StartDate >= query.DateFrom.Value.Date);

        if (query.DateTo.HasValue)
        {
            var dayAfterDateTo = query.DateTo.Value.Date.AddDays(1);
            timesheets = timesheets.Where(timesheet => timesheet.StartDate < dayAfterDateTo);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            var statusSearchTerm = term.Replace(" ", string.Empty, StringComparison.Ordinal);
            var matchesStatus = Enum.TryParse<TimesheetStatus>(statusSearchTerm, true, out var parsedStatus) &&
                Enum.IsDefined(parsedStatus);
            var matchesDate = DateTime.TryParseExact(
                term,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate);
            timesheets = timesheets.Where(timesheet =>
                timesheet.Employee.FirstName.Contains(term) ||
                timesheet.Employee.LastName.Contains(term) ||
                (timesheet.Employee.FirstName + " " + timesheet.Employee.LastName).Contains(term) ||
                timesheet.ProjectName.Contains(term) ||
                (timesheet.Project != null && timesheet.Project.ProjectName.Contains(term)) ||
                timesheet.TaskName.Contains(term) ||
                timesheet.Description.Contains(term) ||
                (matchesStatus && timesheet.Status == parsedStatus) ||
                (matchesDate && timesheet.StartDate.Date == parsedDate.Date));
        }

        var filteredCount = await timesheets.CountAsync(cancellationToken);
        IOrderedQueryable<Timesheet> ordered = (query.SortColumn, query.SortAscending) switch
        {
            ("employeeName", true) => timesheets.OrderBy(timesheet => timesheet.Employee.LastName)
                .ThenBy(timesheet => timesheet.Employee.FirstName),
            ("employeeName", false) => timesheets.OrderByDescending(timesheet => timesheet.Employee.LastName)
                .ThenByDescending(timesheet => timesheet.Employee.FirstName),
            ("projectName", true) => timesheets.OrderBy(timesheet => timesheet.Project != null
                ? timesheet.Project.ProjectName : timesheet.ProjectName),
            ("projectName", false) => timesheets.OrderByDescending(timesheet => timesheet.Project != null
                ? timesheet.Project.ProjectName : timesheet.ProjectName),
            ("taskName", true) => timesheets.OrderBy(timesheet => timesheet.TaskName),
            ("taskName", false) => timesheets.OrderByDescending(timesheet => timesheet.TaskName),
            ("description", true) => timesheets.OrderBy(timesheet => timesheet.Description),
            ("description", false) => timesheets.OrderByDescending(timesheet => timesheet.Description),
            ("startDate", true) => timesheets.OrderBy(timesheet => timesheet.StartDate),
            ("startDate", false) => timesheets.OrderByDescending(timesheet => timesheet.StartDate),
            ("status", true) => timesheets.OrderBy(timesheet => timesheet.Status),
            ("status", false) => timesheets.OrderByDescending(timesheet => timesheet.Status),
            ("startTime", true) => timesheets.OrderBy(timesheet => timesheet.StartTime),
            ("startTime", false) => timesheets.OrderByDescending(timesheet => timesheet.StartTime),
            _ => timesheets.OrderByDescending(timesheet => timesheet.StartDate)
        };

        var items = await ordered
            .ThenBy(timesheet => timesheet.Id)
            .Skip(query.Start)
            .Take(query.Length)
            .ToListAsync(cancellationToken);

        return (items, totalCount, filteredCount);
    }

    public async Task<IReadOnlyList<Timesheet>> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Timesheets
            .AsNoTracking()
            .Include(timesheet => timesheet.Employee)
                .ThenInclude(employee => employee.ProjectAssignments)
            .Include(timesheet => timesheet.Project)
            .Where(timesheet => timesheet.EmployeeId == employeeId && !timesheet.Employee.EmpIsDeleted)
            .OrderByDescending(timesheet => timesheet.StartDate)
            .ToListAsync(cancellationToken);

    public Task<Timesheet?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Timesheets
            .Include(timesheet => timesheet.Employee)
                .ThenInclude(employee => employee.ProjectAssignments)
            .Include(timesheet => timesheet.Project)
            .FirstOrDefaultAsync(
                timesheet => timesheet.Id == id && !timesheet.Employee.EmpIsDeleted,
                cancellationToken);

    public async Task AddAsync(Timesheet timesheet, CancellationToken cancellationToken = default) =>
        await dbContext.Timesheets.AddAsync(timesheet, cancellationToken);

    public void Update(Timesheet timesheet) => dbContext.Timesheets.Update(timesheet);

    public void Remove(Timesheet timesheet) => dbContext.Timesheets.Remove(timesheet);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
