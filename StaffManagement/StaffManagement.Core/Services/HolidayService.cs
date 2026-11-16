using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class HolidayService(IHolidayRepository repository) : IHolidayService
{
    public Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public Task AddAsync(
        string holidayName,
        DateTime holidayDate,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var holiday = new Holiday
        {
            HolidayName = holidayName.Trim(),
            HolidayDate = holidayDate.Date,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        return repository.AddAsync(holiday, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var holiday = await repository.GetByIdAsync(id, cancellationToken);
        if (holiday is null)
            return false;

        await repository.DeleteAsync(holiday, cancellationToken);
        return true;
    }
}
