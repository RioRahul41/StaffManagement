using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeQualificationRepository(ApplicationDbContext dbContext) : IEmployeeQualificationRepository
{
    public async Task<IReadOnlyList<EmployeeQualification>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeQualifications.AsNoTracking()
            .Where(item => item.EmployeeId == employeeId)
            .OrderByDescending(item => item.YearOfPassing)
            .ThenBy(item => item.Qualification)
            .ToListAsync(cancellationToken);

    public Task<EmployeeQualification?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.EmployeeQualifications.AsNoTracking()
            .FirstOrDefaultAsync(item => item.EmployeeQualificationId == id, cancellationToken);

    public Task<EmployeeQualification?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.EmployeeQualifications.FirstOrDefaultAsync(item => item.EmployeeQualificationId == id, cancellationToken);
    public async Task AddAsync(EmployeeQualification qualification, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeQualifications.AddAsync(qualification, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
