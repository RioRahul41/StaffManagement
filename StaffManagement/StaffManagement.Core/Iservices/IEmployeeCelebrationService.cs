using StaffManagement.Core.DTOs;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeCelebrationService
{
    Task<EmployeeCelebrationGreetingDto?> GetAndRecordTodayGreetingAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);
}
