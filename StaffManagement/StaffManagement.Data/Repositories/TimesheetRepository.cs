using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class TimesheetRepository(ApplicationDbContext dbContext) : ITimesheetRepository
{
    public async Task<IReadOnlyList<Timesheet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Timesheets
            .AsNoTracking()
            .Include(timesheet => timesheet.Employee)
            .Where(timesheet => !timesheet.Employee.EmpIsDeleted)
            .OrderByDescending(timesheet => timesheet.StartDate)
            .ThenBy(timesheet => timesheet.Employee.LastName)
            .ThenBy(timesheet => timesheet.Employee.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Timesheet>> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Timesheets
            .AsNoTracking()
            .Include(timesheet => timesheet.Employee)
            .Where(timesheet => timesheet.EmployeeId == employeeId && !timesheet.Employee.EmpIsDeleted)
            .OrderByDescending(timesheet => timesheet.StartDate)
            .ToListAsync(cancellationToken);

    public Task<Timesheet?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Timesheets
            .Include(timesheet => timesheet.Employee)
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
