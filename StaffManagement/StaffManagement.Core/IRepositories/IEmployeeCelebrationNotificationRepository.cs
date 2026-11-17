using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.IRepositories;

public interface IEmployeeCelebrationNotificationRepository
{
    Task<bool> ExistsAsync(
        int employeeId,
        EmployeeCelebrationType celebrationType,
        DateTime celebrationDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        EmployeeCelebrationNotification notification,
        CancellationToken cancellationToken = default);
}
