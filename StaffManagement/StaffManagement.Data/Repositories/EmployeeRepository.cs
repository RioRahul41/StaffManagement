using Microsoft.EntityFrameworkCore;
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
            .Where(employee => !employee.EmpIsDeleted)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Employees
            .Include(employee => employee.IdentityUser)
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .FirstOrDefaultAsync(employee => employee.Id == id && !employee.EmpIsDeleted, cancellationToken);

    public Task<Employee?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default) =>
        dbContext.Employees
            .Include(employee => employee.IdentityUser)
            .Include(employee => employee.ProjectAssignments)
                .ThenInclude(assignment => assignment.Project)
            .FirstOrDefaultAsync(employee => employee.IdentityUserId == identityUserId && !employee.EmpIsDeleted, cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default) =>
        await dbContext.Employees.AddAsync(employee, cancellationToken);

    public void Update(Employee employee) => dbContext.Employees.Update(employee);

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
