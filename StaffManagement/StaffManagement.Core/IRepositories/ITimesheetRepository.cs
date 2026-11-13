using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface ITimesheetRepository
{
    Task<IReadOnlyList<Timesheet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Timesheet>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Timesheet?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Timesheet timesheet, CancellationToken cancellationToken = default);
    void Update(Timesheet timesheet);
    void Remove(Timesheet timesheet);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
