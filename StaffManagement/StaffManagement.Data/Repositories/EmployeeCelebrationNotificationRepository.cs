using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeCelebrationNotificationRepository(ApplicationDbContext context)
    : IEmployeeCelebrationNotificationRepository
{
    public Task<bool> ExistsAsync(
        int employeeId,
        EmployeeCelebrationType celebrationType,
        DateTime celebrationDate,
        CancellationToken cancellationToken = default) =>
        context.EmployeeCelebrationNotifications.AnyAsync(notification =>
            notification.EmployeeId == employeeId
            && notification.CelebrationType == celebrationType
            && notification.CelebrationDate == celebrationDate.Date,
            cancellationToken);

    public async Task AddAsync(
        EmployeeCelebrationNotification notification,
        CancellationToken cancellationToken = default)
    {
        await context.EmployeeCelebrationNotifications.AddAsync(notification, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
