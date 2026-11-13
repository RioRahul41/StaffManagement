using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.ExpenseModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Services;

public sealed class EmployeeExpenseService(
    IEmployeeExpenseRepository expenseRepository,
    IEmployeeRepository employeeRepository) : IEmployeeExpenseService
{
    public async Task<PagedResultDto<EmployeeExpenseDto>> GetPagedAsync(
        bool canManageAll,
        string identityUserId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        int? employeeId = null;
        if (!canManageAll)
        {
            var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
            if (employee is null)
                return new PagedResultDto<EmployeeExpenseDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

            employeeId = employee.Id;
        }

        var result = await expenseRepository.GetPagedAsync(
            employeeId,
            pageNumber,
            pageSize,
            cancellationToken);

        return new PagedResultDto<EmployeeExpenseDto>
        {
            Items = result.Items.Select(Map).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<EmployeeExpenseDto?> GetByIdAsync(
        int id,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense is null || !await CanAccessAsync(expense, canManageAll, identityUserId, cancellationToken))
            return null;

        return Map(expense);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateEmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(model.FromDate, model.ToDate, model.Amount, model.Status);
        if (validationError is not null)
            return (false, validationError);

        var employee = await GetTargetEmployeeAsync(
            model.EmployeeId,
            canApprove,
            identityUserId,
            cancellationToken);

        if (employee is null)
            return (false, "The selected employee was not found or you are not allowed to use that employee.");

        var expense = new EmployeeExpense
        {
            EmployeeId = employee.Id,
            FirstName = employee.FirstName,
            Title = model.Title.Trim(),
            FromDate = model.FromDate.Date,
            ToDate = model.ToDate.Date,
            Amount = model.Amount,
            ApprovalNotes = canApprove ? model.ApprovalNotes?.Trim() : null,
            Status = canApprove ? model.Status : ExpenseStatus.Pending,
            CreatedTime = DateTime.UtcNow
        };

        await expenseRepository.AddAsync(expense, cancellationToken);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(
        EmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(model.FromDate, model.ToDate, model.Amount, model.Status);
        if (validationError is not null)
            return (false, validationError);

        var expense = await expenseRepository.GetByIdAsync(model.Id, cancellationToken);
        if (expense is null)
            return (false, "Expense record was not found.");

        if (!await CanAccessAsync(expense, canApprove, identityUserId, cancellationToken))
            return (false, "You are not allowed to manage this expense record.");

        expense.Title = model.Title.Trim();
        expense.FromDate = model.FromDate.Date;
        expense.ToDate = model.ToDate.Date;
        expense.Amount = model.Amount;
        if (canApprove)
        {
            expense.Status = model.Status;
            expense.ApprovalNotes = model.ApprovalNotes?.Trim();
        }
        expense.UpdatedTime = DateTime.UtcNow;

        expenseRepository.Update(expense);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteAsync(
        int id,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense is null)
            return (false, "Expense record was not found.");

        if (!await CanAccessAsync(expense, canManageAll, identityUserId, cancellationToken))
            return (false, "You are not allowed to delete this expense record.");

        expenseRepository.Remove(expense);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<Employee?> GetTargetEmployeeAsync(
        int? requestedEmployeeId,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        if (canApprove && requestedEmployeeId.HasValue)
            return await employeeRepository.GetByIdAsync(requestedEmployeeId.Value, cancellationToken);

        return await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
    }

    private async Task<bool> CanAccessAsync(
        EmployeeExpense expense,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        if (canManageAll)
            return true;

        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        return employee is not null && employee.Id == expense.EmployeeId;
    }

    private static string? Validate(
        DateTime fromDate,
        DateTime toDate,
        decimal amount,
        ExpenseStatus status)
    {
        if (toDate.Date < fromDate.Date)
            return "To date cannot be earlier than From date.";

        if (amount <= 0)
            return "Amount must be greater than zero.";

        if (!Enum.IsDefined(status))
            return "The selected status is invalid.";

        return null;
    }

    private static EmployeeExpenseDto Map(EmployeeExpense expense) => new()
    {
        Id = expense.Id,
        EmployeeId = expense.EmployeeId,
        FirstName = expense.FirstName,
        Title = expense.Title,
        FromDate = expense.FromDate,
        ToDate = expense.ToDate,
        Amount = expense.Amount,
        ApprovalNotes = expense.ApprovalNotes,
        Status = expense.Status
    };
}
