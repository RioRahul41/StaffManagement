using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;
namespace StaffManagement.Data.Repositories;
public sealed class EmployeeBankDetailRepository(ApplicationDbContext dbContext) : IEmployeeBankDetailRepository
{
    public Task<EmployeeBankDetail?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) => dbContext.EmployeeBankDetails.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);
    public Task<EmployeeBankDetail?> GetTrackedByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) => dbContext.EmployeeBankDetails.FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);
    public async Task AddAsync(EmployeeBankDetail detail, CancellationToken cancellationToken = default) => await dbContext.EmployeeBankDetails.AddAsync(detail, cancellationToken);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
