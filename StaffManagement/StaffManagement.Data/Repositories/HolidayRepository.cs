using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class HolidayRepository(ApplicationDbContext context) : IHolidayRepository
{
    public async Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Holidays
            .AsNoTracking()
            .OrderBy(holiday => holiday.HolidayDate)
            .ThenBy(holiday => holiday.HolidayName)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        await context.Holidays.AddAsync(holiday, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<Holiday?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Holidays.FirstOrDefaultAsync(holiday => holiday.Id == id, cancellationToken);

    public async Task UpdateAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        context.Holidays.Update(holiday);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        context.Holidays.Remove(holiday);
        await context.SaveChangesAsync(cancellationToken);
    }
}
