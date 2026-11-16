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
    public async Task<PagedResultDto<EmployeeExpenseGroupDto>> GetPagedGroupsAsync(
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
                return new PagedResultDto<EmployeeExpenseGroupDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

            employeeId = employee.Id;
        }

        var result = await expenseRepository.GetPagedGroupsAsync(
            employeeId,
            pageNumber,
            pageSize,
            cancellationToken);

        return new PagedResultDto<EmployeeExpenseGroupDto>
        {
            Items = result.Groups.Select(group => new EmployeeExpenseGroupDto
            {
                EmployeeId = group.EmployeeId,
                Title = group.Title,
                FromDate = group.FromDate,
                ToDate = group.ToDate,
                Categories = group.Expenses.Select(expense => expense.Category).Distinct().Order().ToList(),
                BillAmount = group.BillAmount,
                ApprovalNotes = group.ApprovalNotes,
                Status = group.Expenses.Select(expense => expense.Status).Distinct().Count() == 1
                    ? group.Status.ToString()
                    : "Mixed",
                Expenses = group.Expenses.Select(Map).ToList()
            }).ToList(),
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
        string? documentPath = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(
            model.FromDate, model.ToDate, model.Amount, model.BillAmount,
            model.Category, model.BillNumber, model.Status);
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
            Category = model.Category.Trim(),
            Amount = model.Amount.GetValueOrDefault(),
            BillNumber = model.BillNumber.Trim(),
            Description = model.Description?.Trim(),
            BillAmount = model.BillAmount.GetValueOrDefault(),
            DocumentPath = documentPath,
            ApprovalNotes = canApprove ? model.ApprovalNotes?.Trim() : null,
            Status = canApprove ? model.Status : ExpenseStatus.Pending,
            CreatedTime = DateTime.UtcNow
        };

        await expenseRepository.AddAsync(expense, cancellationToken);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error, int? FailedIndex)> CreateManyAsync(
        IReadOnlyList<CreateEmployeeExpenseDto> models,
        IReadOnlyList<string?> documentPaths,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        if (models.Count == 0 || models.Count != documentPaths.Count)
            return (false, "Add at least one expense and upload its document.", 0);

        var expenses = new List<EmployeeExpense>(models.Count);
        for (var index = 0; index < models.Count; index++)
        {
            var model = models[index];
            model.BillAmount = model.Amount;
            var validationError = Validate(
                model.FromDate, model.ToDate, model.Amount, model.BillAmount,
                model.Category, model.BillNumber, model.Status);
            if (validationError is not null)
                return (false, validationError, index);

            if (string.IsNullOrWhiteSpace(documentPaths[index]))
                return (false, "Upload the bill document.", index);

            var employee = await GetTargetEmployeeAsync(
                model.EmployeeId, canApprove, identityUserId, cancellationToken);
            if (employee is null)
                return (false, "The selected employee was not found or you are not allowed to use that employee.", index);

            expenses.Add(new EmployeeExpense
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                Title = model.Title.Trim(),
                FromDate = model.FromDate.Date,
                ToDate = model.ToDate.Date,
                Category = model.Category.Trim(),
                Amount = model.Amount.GetValueOrDefault(),
                BillNumber = model.BillNumber.Trim(),
                Description = model.Description?.Trim(),
                BillAmount = model.Amount.GetValueOrDefault(),
                DocumentPath = documentPaths[index],
                Status = canApprove ? model.Status : ExpenseStatus.Pending,
                CreatedTime = DateTime.UtcNow
            });
        }

        foreach (var expense in expenses)
            await expenseRepository.AddAsync(expense, cancellationToken);

        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(
        EmployeeExpenseDto model,
        bool canApprove,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(
            model.FromDate, model.ToDate, model.Amount, model.BillAmount,
            model.Category, model.BillNumber, model.Status);
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
        expense.Category = model.Category.Trim();
        expense.BillNumber = model.BillNumber.Trim();
        expense.Description = model.Description?.Trim();
        expense.BillAmount = model.BillAmount;
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
        decimal? amount,
        decimal? billAmount,
        string category,
        string billNumber,
        ExpenseStatus status)
    {
        if (toDate.Date < fromDate.Date)
            return "To date cannot be earlier than From date.";

        if (!amount.HasValue || amount.Value <= 0)
            return "Amount must be greater than zero.";

        if (!billAmount.HasValue || billAmount.Value <= 0)
            return "Total amount must be greater than zero.";

        if (string.IsNullOrWhiteSpace(category))
            return "Category is required.";

        if (string.IsNullOrWhiteSpace(billNumber))
            return "Bill number is required.";

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
        Category = expense.Category,
        BillNumber = expense.BillNumber,
        Description = expense.Description,
        BillAmount = expense.BillAmount,
        DocumentPath = expense.DocumentPath,
        Amount = expense.Amount,
        ApprovalNotes = expense.ApprovalNotes,
        Status = expense.Status
    };
}
