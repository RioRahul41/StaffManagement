using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeProfileRepository(ApplicationDbContext dbContext) : IEmployeeProfileRepository
{
    public async Task<IReadOnlyList<EmployeeProfile>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeProfiles.AsNoTracking()
            .Include(profile => profile.Employee)
            .Where(profile => !profile.Employee.EmpIsDeleted)
            .OrderBy(profile => profile.Employee.LastName)
            .ThenBy(profile => profile.Employee.FirstName)
            .ToListAsync(cancellationToken);

    public Task<EmployeeProfile?> GetByEmployeeIdAsync(int employeeId, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<EmployeeProfile> query = dbContext.EmployeeProfiles.Include(profile => profile.Employee);
        if (asNoTracking)
            query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(profile => profile.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeOtherDetail>> GetOtherDetailsAsync(int employeeId, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeOtherDetails.AsNoTracking()
            .Where(detail => detail.EmployeeId == employeeId)
            .OrderBy(detail => detail.DocumentType)
            .ToListAsync(cancellationToken);

    public Task<EmployeeAddressInformation?> GetAddressInformationByEmployeeIdAsync(int employeeId, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<EmployeeAddressInformation> query = dbContext.EmployeeAddressInformations;
        if (asNoTracking)
            query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(address => address.EmployeeId == employeeId, cancellationToken);
    }

    public async Task AddAddressInformationAsync(EmployeeAddressInformation address, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeAddressInformations.AddAsync(address, cancellationToken);
    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Employees.AsNoTracking()
            .Where(employee => !employee.EmpIsDeleted)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        dbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == employeeId && !employee.EmpIsDeleted, cancellationToken);

    public Task<Employee?> GetEmployeeByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default) =>
        dbContext.Employees.FirstOrDefaultAsync(employee => employee.IdentityUserId == identityUserId && !employee.EmpIsDeleted, cancellationToken);

    public Task<bool> EmployeeHasProfileAsync(int employeeId, CancellationToken cancellationToken = default) =>
        dbContext.EmployeeProfiles.AnyAsync(profile => profile.EmployeeId == employeeId, cancellationToken);

    public Task<bool> IsActiveEmployeeAsync(int employeeId, CancellationToken cancellationToken = default) =>
        dbContext.Employees.AnyAsync(employee => employee.Id == employeeId && !employee.EmpIsDeleted, cancellationToken);

    public Task<bool> EmployeeBelongsToIdentityUserAsync(int employeeId, string identityUserId, CancellationToken cancellationToken = default) =>
        dbContext.Employees.AnyAsync(employee => employee.Id == employeeId && employee.IdentityUserId == identityUserId && !employee.EmpIsDeleted, cancellationToken);

    public async Task AddAsync(EmployeeProfile profile, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeProfiles.AddAsync(profile, cancellationToken);

    public void Remove(EmployeeProfile profile) => dbContext.EmployeeProfiles.Remove(profile);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
