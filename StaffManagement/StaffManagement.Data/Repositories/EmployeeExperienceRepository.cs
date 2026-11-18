using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;
namespace StaffManagement.Data.Repositories;
public sealed class EmployeeExperienceRepository(ApplicationDbContext dbContext) : IEmployeeExperienceRepository
{
    public async Task<IReadOnlyList<EmployeeExperience>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) => await dbContext.EmployeeExperiences.AsNoTracking().Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.DateOfJoining).ToListAsync(cancellationToken);
    public Task<EmployeeExperience?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => dbContext.EmployeeExperiences.AsNoTracking().Include(x => x.Employee).FirstOrDefaultAsync(x => x.EmployeeExperienceId == id, cancellationToken);
    public Task<EmployeeExperience?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default) => dbContext.EmployeeExperiences.FirstOrDefaultAsync(x => x.EmployeeExperienceId == id, cancellationToken);
    public async Task AddAsync(EmployeeExperience experience, CancellationToken cancellationToken = default) => await dbContext.EmployeeExperiences.AddAsync(experience, cancellationToken);
    public void Remove(EmployeeExperience experience) => dbContext.EmployeeExperiences.Remove(experience);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => dbContext.SaveChangesAsync(cancellationToken);
}

