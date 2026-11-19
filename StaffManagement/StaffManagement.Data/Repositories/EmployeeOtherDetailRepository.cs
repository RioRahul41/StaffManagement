using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeOtherDetailRepository(ApplicationDbContext dbContext) : IEmployeeOtherDetailRepository
{
    public Task<EmployeeOtherDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.EmployeeOtherDetails.AsNoTracking().FirstOrDefaultAsync(detail => detail.EmployeeOtherDetailId == id, cancellationToken);

    public Task<EmployeeOtherDetail?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.EmployeeOtherDetails.FirstOrDefaultAsync(detail => detail.EmployeeOtherDetailId == id, cancellationToken);

    public async Task AddAsync(EmployeeOtherDetail detail, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeOtherDetails.AddAsync(detail, cancellationToken);

    public void Remove(EmployeeOtherDetail detail) => dbContext.EmployeeOtherDetails.Remove(detail);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
