using StaffManagement.Core.DTOs;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Services;

public sealed class EmployeeCelebrationService(
    IEmployeeRepository employeeRepository,
    IEmployeeCelebrationNotificationRepository notificationRepository) : IEmployeeCelebrationService
{
    public async Task<EmployeeCelebrationGreetingDto?> GetAndRecordTodayGreetingAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (employee is null || !employee.EmpIsActive)
            return null;

        var today = DateTime.Today;
        var isBirthday = employee.DOB.HasValue
            && employee.DOB.Value.Month == today.Month
            && employee.DOB.Value.Day == today.Day;
        var anniversaryDay = Math.Min(
            employee.CreatedTime.Day,
            DateTime.DaysInMonth(today.Year, employee.CreatedTime.Month));
        var isWorkAnniversary = employee.CreatedTime.Year < today.Year
            && employee.CreatedTime.Month == today.Month
            && anniversaryDay == today.Day;

        var hasNewBirthdayGreeting = isBirthday
            && !await notificationRepository.ExistsAsync(
                employee.Id, EmployeeCelebrationType.Birthday, today, cancellationToken);
        var hasNewAnniversaryGreeting = isWorkAnniversary
            && !await notificationRepository.ExistsAsync(
                employee.Id, EmployeeCelebrationType.WorkAnniversary, today, cancellationToken);

        if (!hasNewBirthdayGreeting && !hasNewAnniversaryGreeting)
            return null;

        if (hasNewBirthdayGreeting)
        {
            await notificationRepository.AddAsync(new EmployeeCelebrationNotification
            {
                EmployeeId = employee.Id,
                CelebrationType = EmployeeCelebrationType.Birthday,
                CelebrationDate = today
            }, cancellationToken);
        }

        if (hasNewAnniversaryGreeting)
        {
            await notificationRepository.AddAsync(new EmployeeCelebrationNotification
            {
                EmployeeId = employee.Id,
                CelebrationType = EmployeeCelebrationType.WorkAnniversary,
                CelebrationDate = today
            }, cancellationToken);
        }

        return new EmployeeCelebrationGreetingDto
        {
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            Designation = employee.Designation,
            HasBirthday = hasNewBirthdayGreeting,
            HasWorkAnniversary = hasNewAnniversaryGreeting,
            CompletedYears = hasNewAnniversaryGreeting ? today.Year - employee.CreatedTime.Year : 0
        };
    }
}
