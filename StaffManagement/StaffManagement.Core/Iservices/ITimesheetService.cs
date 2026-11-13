using StaffManagement.Core.DTOs.TimesheetModel;

namespace StaffManagement.Core.Iservices;

public interface ITimesheetService
{
    Task<IReadOnlyList<TimesheetDto>> GetAllAsync(
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<TimesheetDto?> GetByIdAsync(
        int id,
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateTimesheetDto model,
        bool canManage,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> UpdateAsync(
        TimesheetDto model,
        bool canManage,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> DeleteAsync(
        int id,
        bool canManage,
        string identityUserId,
        CancellationToken cancellationToken = default);

}
