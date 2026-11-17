using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeRepository(ApplicationDbContext dbContext) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .Include(employee => employee.ReportingPerson)
            .Where(employee => !employee.EmpIsDeleted)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPageAsync(
        string? searchTerm,
        int? departmentId,
        string? designation,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var employees = dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .Include(employee => employee.ReportingPerson)
            .Where(employee => !employee.EmpIsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            employees = employees.Where(employee =>
                employee.FirstName.Contains(term) ||
                employee.LastName.Contains(term) ||
                (employee.FirstName + " " + employee.LastName).Contains(term) ||
                employee.Email.Contains(term) ||
                (employee.Phone != null && employee.Phone.Contains(term)) ||
                employee.Location.Contains(term) ||
                (employee.Department != null && employee.Department.Contains(term)) ||
                (employee.Designation != null && employee.Designation.Contains(term)));
        }

        if (departmentId.HasValue)
            employees = employees.Where(employee => employee.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(designation))
            employees = employees.Where(employee => employee.Designation == designation);

        if (isActive.HasValue)
            employees = employees.Where(employee => employee.EmpIsActive == isActive.Value);

        var totalCount = await employees.CountAsync(cancellationToken);
        var items = await employees
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount, int FilteredCount)> GetDataTablePageAsync(
        EmployeeDataTableQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var employees = dbContext.Employees
            .AsNoTracking()
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .Include(employee => employee.ReportingPerson)
            .Where(employee => !employee.EmpIsDeleted);

        var totalCount = await employees.CountAsync(cancellationToken);

        if (query.DepartmentId.HasValue)
            employees = employees.Where(employee => employee.DepartmentId == query.DepartmentId.Value);

        if (!string.IsNullOrWhiteSpace(query.Designation))
            employees = employees.Where(employee => employee.Designation == query.Designation);

        if (query.IsActive.HasValue)
            employees = employees.Where(employee => employee.EmpIsActive == query.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            employees = employees.Where(employee =>
                employee.FirstName.Contains(term) ||
                employee.LastName.Contains(term) ||
                (employee.FirstName + " " + employee.LastName).Contains(term) ||
                employee.Email.Contains(term) ||
                (employee.Phone != null && employee.Phone.Contains(term)) ||
                employee.Location.Contains(term) ||
                (employee.Department != null && employee.Department.Contains(term)) ||
                (employee.Designation != null && employee.Designation.Contains(term)) ||
                (employee.ReportingPerson != null &&
                    (employee.ReportingPerson.FirstName + " " + employee.ReportingPerson.LastName).Contains(term)));
        }

        var filteredCount = await employees.CountAsync(cancellationToken);
        IOrderedQueryable<Employee> ordered = (query.SortColumn, query.SortAscending) switch
        {
            ("name", true) => employees.OrderBy(employee => employee.LastName).ThenBy(employee => employee.FirstName),
            ("name", false) => employees.OrderByDescending(employee => employee.LastName).ThenByDescending(employee => employee.FirstName),
            ("email", true) => employees.OrderBy(employee => employee.Email),
            ("email", false) => employees.OrderByDescending(employee => employee.Email),
            ("department", true) => employees.OrderBy(employee => employee.Department),
            ("department", false) => employees.OrderByDescending(employee => employee.Department),
            ("reportingPerson", true) => employees.OrderBy(employee => employee.ReportingPerson!.LastName)
                .ThenBy(employee => employee.ReportingPerson!.FirstName),
            ("reportingPerson", false) => employees.OrderByDescending(employee => employee.ReportingPerson!.LastName)
                .ThenByDescending(employee => employee.ReportingPerson!.FirstName),
            ("status", true) => employees.OrderBy(employee => employee.EmpIsActive),
            ("status", false) => employees.OrderByDescending(employee => employee.EmpIsActive),
            _ => employees.OrderBy(employee => employee.LastName).ThenBy(employee => employee.FirstName)
        };

        var items = await ordered
            .ThenBy(employee => employee.Id)
            .Skip(query.Start)
            .Take(query.Length)
            .ToListAsync(cancellationToken);

        return (items, totalCount, filteredCount);
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Employees
            .Include(employee => employee.IdentityUser)
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .Include(employee => employee.ReportingPerson)
            .FirstOrDefaultAsync(employee => employee.Id == id && !employee.EmpIsDeleted, cancellationToken);

    public Task<Employee?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default) =>
        dbContext.Employees
            .Include(employee => employee.IdentityUser)
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .Include(employee => employee.ReportingPerson)
            .FirstOrDefaultAsync(employee => employee.IdentityUserId == identityUserId && !employee.EmpIsDeleted, cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default) =>
        await dbContext.Employees.AddAsync(employee, cancellationToken);

    public void Update(Employee employee) => dbContext.Employees.Update(employee);

    public void UpdateLeaveAllowances(Employee employee)
    {
        dbContext.Entry(employee).Property(item => item.SickLeaveAllowance).IsModified = true;
        dbContext.Entry(employee).Property(item => item.SickLeaveRemaining).IsModified = true;
        dbContext.Entry(employee).Property(item => item.CasualLeaveAllowance).IsModified = true;
        dbContext.Entry(employee).Property(item => item.CasualLeaveRemaining).IsModified = true;
        dbContext.Entry(employee).Property(item => item.UpdatedTime).IsModified = true;
    }

    public void Remove(Employee employee)
    {
        employee.EmpIsDeleted = true;
        employee.EmpIsActive = false;
        employee.UpdatedTime = DateTime.UtcNow;
        dbContext.Employees.Update(employee);
    }

    public Task<bool> EmailExistsAsync(string email, int? excludedId = null, CancellationToken cancellationToken = default) =>
        dbContext.Employees.AnyAsync(
            employee => !employee.EmpIsDeleted &&
                        employee.Email == email &&
                        (!excludedId.HasValue || employee.Id != excludedId.Value),
            cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
