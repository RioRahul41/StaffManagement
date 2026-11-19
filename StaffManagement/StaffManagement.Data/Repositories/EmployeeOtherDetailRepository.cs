using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeOtherDetailRepository(ApplicationDbContext dbContext) : IEmployeeOtherDetailRepository
{
    public async Task AddAsync(EmployeeOtherDetail detail, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeOtherDetails.AddAsync(detail, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
