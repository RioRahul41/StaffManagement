using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class DesignationRepository(ApplicationDbContext context) : IDesignationRepository
{
    public async Task<IReadOnlyList<Designation>> GetAllAsync()
    {
        return await context.Designations
            .AsNoTracking()
            .Where(designation => designation.IsActive)
            .OrderBy(designation => designation.DesignationName)
            .ToListAsync();
    }

    public Task<Designation?> GetByIdAsync(int designationId)
    {
        return context.Designations
            .FirstOrDefaultAsync(designation =>
                designation.DesignationId == designationId && designation.IsActive);
    }

    public Task<bool> ExistsByNameAsync(string designationName, int? excludedId = null)
    {
        var normalizedName = designationName.Trim().ToLower();

        return context.Designations.AnyAsync(designation =>
            designation.IsActive &&
            designation.DesignationName.ToLower() == normalizedName &&
            (!excludedId.HasValue || designation.DesignationId != excludedId.Value));
    }

    public async Task AddAsync(Designation designation)
    {
        await context.Designations.AddAsync(designation);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Designation designation)
    {
        context.Designations.Update(designation);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Designation designation)
    {
        designation.IsActive = false;
        designation.UpdatedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
