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
                FirstName = group.Expenses.FirstOrDefault()?.FirstName ?? string.Empty,
                BatchId = group.BatchId,
                Title = group.Title,
                FromDate = group.FromDate,
                ToDate = group.ToDate,
                Categories = group.Expenses.Select(expense => expense.Category).Distinct().Order().ToList(),
                BillAmount = group.BillAmount,
                ApprovalNotes = group.Expenses
                    .OrderByDescending(expense => expense.UpdatedTime ?? expense.CreatedTime)
                    .First().ApprovalNotes,
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

    public async Task<EmployeeExpenseGroupDto?> GetGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var expenses = await expenseRepository.GetByGroupAsync(
            employeeId, batchId, title, fromDate, toDate, cancellationToken);
        if (expenses.Count == 0)
            return null;

        foreach (var expense in expenses)
        {
            if (!await CanAccessAsync(expense, canManageAll, identityUserId, cancellationToken))
                return null;
        }

        var firstExpense = expenses[0];
        var statuses = expenses.Select(expense => expense.Status).Distinct().ToList();
        return new EmployeeExpenseGroupDto
        {
            EmployeeId = firstExpense.EmployeeId,
            FirstName = firstExpense.FirstName,
            BatchId = firstExpense.BatchId,
            Title = firstExpense.Title,
            FromDate = firstExpense.FromDate,
            ToDate = firstExpense.ToDate,
            Categories = expenses.Select(expense => expense.Category).Distinct().Order().ToList(),
            BillAmount = expenses.Sum(expense => expense.BillAmount),
            ApprovalNotes = expenses
                .OrderByDescending(expense => expense.UpdatedTime ?? expense.CreatedTime)
                .First().ApprovalNotes,
            Status = statuses.Count == 1 ? statuses[0].ToString() : "Mixed",
            Expenses = expenses.OrderBy(expense => expense.Id).Select(Map).ToList()
        };
    }

    public async Task<(bool Succeeded, string? Error)> UpdateGroupAsync(
        EditEmployeeExpenseGroupDto model,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
            return (false, "Title is required.");
        if (model.Title.Length > 200)
            return (false, "Title cannot exceed 200 characters.");
        if (model.ToDate.Date < model.FromDate.Date)
            return (false, "To date cannot be earlier than From date.");
        if (model.Bills is null || model.Bills.Count == 0)
            return (false, "The expense group must contain at least one bill.");

        var expenses = await expenseRepository.GetByGroupAsync(
            model.EmployeeId,
            model.BatchId,
            string.IsNullOrWhiteSpace(model.LookupTitle) ? model.Title : model.LookupTitle,
            model.LookupFromDate == default ? model.FromDate : model.LookupFromDate,
            model.LookupToDate == default ? model.ToDate : model.LookupToDate,
            cancellationToken);
        if (expenses.Count == 0)
            return (false, "Expense group was not found.");
        if (expenses.Any(expense => expense.Status != ExpenseStatus.Pending))
            return (false, "Reviewed expense records cannot be edited.");
        foreach (var expense in expenses)
        {
            if (!await CanAccessAsync(expense, canManageAll, identityUserId, cancellationToken))
                return (false, "You are not allowed to manage this expense group.");
        }

        var existingBillIds = model.Bills.Where(bill => bill.Id > 0).Select(bill => bill.Id).ToList();
        if (model.Bills.Any(bill => bill.Id < 0) || existingBillIds.Distinct().Count() != existingBillIds.Count
            || existingBillIds.Count != expenses.Count || expenses.Any(expense => !existingBillIds.Contains(expense.Id)))
            return (false, "The bill list changed. Refresh the page and try again.");

        foreach (var bill in model.Bills)
        {
            if (string.IsNullOrWhiteSpace(bill.Category) || bill.Category.Length > 100)
                return (false, "Each bill needs a category of 100 characters or fewer.");
            if (bill.Amount <= 0)
                return (false, "Each bill amount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(bill.BillNumber) || bill.BillNumber.Length > 100)
                return (false, "Each bill needs a bill number of 100 characters or fewer.");
            if (bill.Description?.Length > 2000)
                return (false, "A bill description cannot exceed 2,000 characters.");

            if (bill.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(bill.DocumentPath))
                    return (false, "A document is required for each new bill.");

                await expenseRepository.AddAsync(new EmployeeExpense
                {
                    EmployeeId = model.EmployeeId,
                    BatchId = model.BatchId,
                    FirstName = expenses[0].FirstName,
                    Title = model.Title.Trim(),
                    FromDate = model.FromDate.Date,
                    ToDate = model.ToDate.Date,
                    Category = bill.Category.Trim(),
                    BillNumber = bill.BillNumber.Trim(),
                    Description = bill.Description?.Trim(),
                    Amount = bill.Amount,
                    BillAmount = bill.Amount,
                    DocumentPath = bill.DocumentPath,
                    Status = ExpenseStatus.Pending,
                    CreatedTime = DateTime.UtcNow
                }, cancellationToken);
                continue;
            }

            var expense = expenses.Single(item => item.Id == bill.Id);
            expense.Title = model.Title.Trim();
            expense.FromDate = model.FromDate.Date;
            expense.ToDate = model.ToDate.Date;
            expense.Category = bill.Category.Trim();
            expense.Amount = bill.Amount;
            expense.BillAmount = bill.Amount;
            expense.BillNumber = bill.BillNumber.Trim();
            expense.Description = bill.Description?.Trim();
            if (!string.IsNullOrWhiteSpace(bill.DocumentPath))
                expense.DocumentPath = bill.DocumentPath;
            expense.UpdatedTime = DateTime.UtcNow;
            expenseRepository.Update(expense);
        }

        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        bool canManageAll,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var expenses = await expenseRepository.GetByGroupAsync(
            employeeId, batchId, title, fromDate, toDate, cancellationToken);
        if (expenses.Count == 0)
            return (false, "Expense group was not found.");
        if (expenses.Any(expense => expense.Status != ExpenseStatus.Pending))
            return (false, "Reviewed expense groups cannot be deleted.");
        foreach (var expense in expenses)
        {
            if (!await CanAccessAsync(expense, canManageAll, identityUserId, cancellationToken))
                return (false, "You are not allowed to delete this expense group.");
        }

        foreach (var expense in expenses)
            expenseRepository.Remove(expense);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
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
            BatchId = Guid.NewGuid(),
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
        var batchId = Guid.NewGuid();
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
                BatchId = batchId,
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

        if (expense.Status != ExpenseStatus.Pending)
            return (false, "Reviewed expense records cannot be edited.");

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

    public async Task<(bool Succeeded, string? Error)> ReviewGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        string approvalNotes,
        ExpenseStatus status,
        CancellationToken cancellationToken = default)
    {
        if (status is not ExpenseStatus.Approved and not ExpenseStatus.Rejected)
            return (false, "Choose approve or reject for this expense.");
        if (string.IsNullOrWhiteSpace(approvalNotes))
            return (false, "Approval notes are required.");
        if (approvalNotes.Trim().Length > 1000)
            return (false, "Approval notes cannot exceed 1,000 characters.");

        var expenses = await expenseRepository.GetByGroupAsync(
            employeeId, batchId, title, fromDate, toDate, cancellationToken);
        if (expenses.Count == 0)
            return (false, "Expense record was not found.");
        var pendingExpenses = expenses.Where(expense => expense.Status == ExpenseStatus.Pending).ToList();
        if (pendingExpenses.Count == 0)
            return (false, "This expense has already been reviewed.");

        foreach (var expense in pendingExpenses)
        {
            expense.Status = status;
            expense.ApprovalNotes = approvalNotes.Trim();
            expense.UpdatedTime = DateTime.UtcNow;
            expenseRepository.Update(expense);
        }
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
        if (expense.Status != ExpenseStatus.Pending)
            return (false, "Reviewed expense records cannot be deleted.");

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
