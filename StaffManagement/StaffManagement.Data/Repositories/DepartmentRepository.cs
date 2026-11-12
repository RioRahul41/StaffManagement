using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public class DepartmentRepository(ApplicationDbContext context) : IDepartmentRepository
{
    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        return await context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
    }
    public Task<Department?> GetByIdAsync(int departmentId)
    {
        return context.Departments
            .FirstOrDefaultAsync(department =>
                department.DepartmentId == departmentId && department.IsActive);
    }

    public Task<bool> ExistsByNameAsync(string departmentName, int? excludedId = null)
    {
        var normalizedName = departmentName.Trim().ToLower();

        return context.Departments.AnyAsync(department =>
            department.IsActive &&
            department.DepartmentName.ToLower() == normalizedName &&
            (!excludedId.HasValue || department.DepartmentId != excludedId.Value));
    }

    public async Task AddAsync(Department department)
    {
        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Department department)
    {
        context.Departments.Update(department);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Department department)
    {
        department.IsActive = false;
        department.UpdatedDate = DateTime.Now;
        await context.SaveChangesAsync();
    }
}
