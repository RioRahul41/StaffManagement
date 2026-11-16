using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.ExpenseModel;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeExpenseService
{
    Task<PagedResultDto<EmployeeExpenseGroupDto>> GetPagedGroupsAsync(
        bool canManageAll,
        string identityUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<EmployeeExpenseDto?> GetByIdAsync(
        int id,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<EmployeeExpenseGroupDto?> GetGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> UpdateGroupAsync(
        EditEmployeeExpenseGroupDto model,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> DeleteGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateEmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        string? documentPath = null,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error, int? FailedIndex)> CreateManyAsync(
        IReadOnlyList<CreateEmployeeExpenseDto> models,
        IReadOnlyList<string?> documentPaths,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> UpdateAsync(
        EmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> ReviewGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        string approvalNotes,
        StaffManagement.Model.Enums.ExpenseStatus status,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> DeleteAsync(
        int id,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default);
}
