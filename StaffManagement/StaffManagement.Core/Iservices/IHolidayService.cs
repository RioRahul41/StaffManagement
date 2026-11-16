using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IHolidayService
{
    Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(string holidayName, DateTime holidayDate, string? description, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
