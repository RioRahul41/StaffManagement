using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.ExpenseModel;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeExpenseService
{
    Task<PagedResultDto<EmployeeExpenseDto>> GetPagedAsync(
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

    Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateEmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> UpdateAsync(
        EmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> DeleteAsync(
        int id,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default);
}
