using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class LeaveTypeRepository(ApplicationDbContext dbContext) : ILeaveTypeRepository
{
    public async Task<IReadOnlyList<LeaveTypeMaster>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.LeaveTypes.AsNoTracking();
        if (!includeInactive)
            query = query.Where(type => type.IsActive);
        return await query.OrderBy(type => !type.IsActive).ToListAsync(cancellationToken);
    }

    public Task<LeaveTypeMaster?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.LeaveTypes.FirstOrDefaultAsync(type => type.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, int? exceptId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        return dbContext.LeaveTypes.AnyAsync(type => type.IsActive && type.Name.ToLower() == normalized &&
            (!exceptId.HasValue || type.Id != exceptId.Value), cancellationToken);
    }

    public async Task AddAsync(LeaveTypeMaster leaveType, CancellationToken cancellationToken = default) =>
        await dbContext.LeaveTypes.AddAsync(leaveType, cancellationToken);

    public void Update(LeaveTypeMaster leaveType) => dbContext.LeaveTypes.Update(leaveType);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}
