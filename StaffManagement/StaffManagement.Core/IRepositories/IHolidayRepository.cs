using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IHolidayRepository
{
    Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Holiday holiday, CancellationToken cancellationToken = default);
    Task<Holiday?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Holiday holiday, CancellationToken cancellationToken = default);
    Task DeleteAsync(Holiday holiday, CancellationToken cancellationToken = default);
}
