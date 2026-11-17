using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.ExpenseModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Models;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class ExpenseController(
    IEmployeeExpenseService expenseService,
    IEmployeeService employeeService,
    IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await expenseService.GetPagedGroupsAsync(
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            page,
            PageSize,
            cancellationToken);

        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetByIdAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        return expense is null ? NotFound() : View(expense);
    }

    public async Task<IActionResult> DetailsGroup(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var group = await GetExpenseGroupAsync(employeeId, batchId, title, fromDate, toDate, cancellationToken);
        return group is null ? NotFound() : View("DetailsGroup", group);
    }

    public async Task<IActionResult> EditGroup(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var group = await GetExpenseGroupAsync(employeeId, batchId, title, fromDate, toDate, cancellationToken);
        if (group is null)
            return NotFound();
        if (group.Expenses.Any(expense => expense.Status != StaffManagement.Model.Enums.ExpenseStatus.Pending))
        {
            TempData["Error"] = "Reviewed expense groups cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        return View("EditGroup", new EditEmployeeExpenseGroupDto
        {
            EmployeeId = group.EmployeeId,
            BatchId = group.BatchId,
            Title = group.Title,
            FromDate = group.FromDate,
            ToDate = group.ToDate,
            LookupTitle = group.Title,
            LookupFromDate = group.FromDate,
            LookupToDate = group.ToDate,
            Bills = group.Expenses.Select(expense => new EditEmployeeExpenseBillDto
            {
                Id = expense.Id,
                Category = expense.Category,
                Amount = expense.Amount,
                BillNumber = expense.BillNumber,
                Description = expense.Description
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGroup(EditEmployeeExpenseGroupDto model, CancellationToken cancellationToken)
    {
        model.Bills ??= [];
        var existingGroup = await GetExpenseGroupAsync(
            model.EmployeeId,
            model.BatchId,
            string.IsNullOrWhiteSpace(model.LookupTitle) ? model.Title : model.LookupTitle,
            model.LookupFromDate == default ? model.FromDate : model.LookupFromDate,
            model.LookupToDate == default ? model.ToDate : model.LookupToDate,
            cancellationToken);
        if (existingGroup is null)
            return NotFound();
        if (existingGroup.Expenses.Any(expense => expense.Status != StaffManagement.Model.Enums.ExpenseStatus.Pending))
        {
            TempData["Error"] = "Reviewed expense groups cannot be edited.";
            return RedirectToAction(nameof(Index));
        }
        if (!ModelState.IsValid)
            return View("EditGroup", model);

        for (var index = 0; index < model.Bills.Count; index++)
        {
            if (model.Bills[index].Id != 0)
                continue;

            var document = Request.Form.Files.FirstOrDefault(file =>
                string.Equals(file.Name, $"Bills[{index}].Document", StringComparison.OrdinalIgnoreCase));
            if (document is not { Length: > 0 })
                ModelState.AddModelError($"Bills[{index}].Document", "A document is required for each new bill.");
        }

        if (!ModelState.IsValid)
            return View("EditGroup", model);

        var uploadedDocuments = new List<string>();
        for (var index = 0; index < model.Bills.Count; index++)
        {
            var bill = model.Bills[index];
            if (bill.Id != 0)
                continue;

            var document = Request.Form.Files.First(file =>
                string.Equals(file.Name, $"Bills[{index}].Document", StringComparison.OrdinalIgnoreCase));
            var (documentPath, uploadError) = await SaveDocumentAsync(document, cancellationToken);
            if (uploadError is not null || documentPath is null)
            {
                foreach (var uploadedPath in uploadedDocuments)
                    DeleteDocument(uploadedPath);
                ModelState.AddModelError($"Bills[{index}].Document", uploadError ?? "The document could not be uploaded.");
                return View("EditGroup", model);
            }

            bill.DocumentPath = documentPath;
            uploadedDocuments.Add(documentPath);
        }

        var result = await expenseService.UpdateGroupAsync(
            model, CanApproveExpenses(), CurrentIdentityUserId(), cancellationToken);
        if (!result.Succeeded)
        {
            foreach (var uploadedPath in uploadedDocuments)
                DeleteDocument(uploadedPath);
            TempData["Error"] = result.Error;
        }
        else
            TempData["Success"] = "Expense group updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DeleteGroup(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var group = await GetExpenseGroupAsync(employeeId, batchId, title, fromDate, toDate, cancellationToken);
        if (group is null)
            return NotFound();
        if (group.Expenses.Any(expense => expense.Status != StaffManagement.Model.Enums.ExpenseStatus.Pending))
        {
            TempData["Error"] = "Reviewed expense groups cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }
        return View("DeleteGroup", group);
    }

    [HttpPost, ActionName("DeleteGroup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGroupConfirmed(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var group = await GetExpenseGroupAsync(employeeId, batchId, title, fromDate, toDate, cancellationToken);
        var result = await expenseService.DeleteGroupAsync(
            employeeId, batchId, title, fromDate, toDate,
            CanApproveExpenses(), CurrentIdentityUserId(), cancellationToken);

        if (!result.Succeeded)
            TempData["Error"] = result.Error;
        else
        {
            if (group is not null)
            {
                foreach (var expense in group.Expenses)
                    DeleteDocument(expense.DocumentPath);
            }
            TempData["Success"] = "Expense group deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Document(int id, CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetByIdAsync(
            id, CanApproveExpenses(), CurrentIdentityUserId(), cancellationToken);
        if (expense is null || string.IsNullOrWhiteSpace(expense.DocumentPath))
            return NotFound();

        var storagePath = Path.Combine(environment.ContentRootPath, "App_Data", "ExpenseDocuments");
        var fullPath = Path.GetFullPath(Path.Combine(storagePath, Path.GetFileName(expense.DocumentPath)));
        var rootPath = Path.GetFullPath(storagePath) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "application/octet-stream", Path.GetFileName(fullPath));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateExpenseBatchViewModel();
        await PopulateEmployeeIdAsync(model.Expenses[0], "Expenses[0].EmployeeId", cancellationToken);
        await LoadEmployeesAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateExpenseBatchViewModel model,
        CancellationToken cancellationToken)
    {
        model.Expenses ??= [];
        if (model.Expenses.Count == 0)
            model.Expenses.Add(new CreateEmployeeExpenseDto());

        for (var index = 0; index < model.Expenses.Count; index++)
        {
            await PopulateEmployeeIdAsync(model.Expenses[index], $"Expenses[{index}].EmployeeId", cancellationToken);
            model.Expenses[index].BillAmount = model.Expenses[index].Amount;
            ModelState.Remove($"Expenses[{index}].BillAmount");
        }

        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync(cancellationToken);
            return View(model);
        }

        var documentPaths = new List<string?>(model.Expenses.Count);
        for (var index = 0; index < model.Expenses.Count; index++)
        {
            var documentFieldName = $"Documents[{index}]";
            var document = Request.Form.Files.FirstOrDefault(file =>
                string.Equals(file.Name, documentFieldName, StringComparison.OrdinalIgnoreCase));
            var (documentPath, uploadError) = await SaveDocumentAsync(document, cancellationToken);
            if (uploadError is not null)
            {
                foreach (var savedPath in documentPaths)
                    DeleteDocument(savedPath);
                ModelState.AddModelError($"Documents[{index}]", uploadError);
                await LoadEmployeesAsync(cancellationToken);
                return View(model);
            }

            documentPaths.Add(documentPath);
        }

        var result = await expenseService.CreateManyAsync(
            model.Expenses,
            documentPaths,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var documentPath in documentPaths)
                DeleteDocument(documentPath);
            AddExpenseFieldError(result.Error!, result.FailedIndex ?? 0);
            await LoadEmployeesAsync(cancellationToken);
            return View(model);
        }

        TempData["Success"] = model.Expenses.Count == 1
            ? "Expense record created successfully."
            : $"{model.Expenses.Count} expense records created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetByIdAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (expense is null)
            return NotFound();
        if (expense.Status != StaffManagement.Model.Enums.ExpenseStatus.Pending)
        {
            TempData["Error"] = "Reviewed expense records cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        return View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EmployeeExpenseDto model,
        CancellationToken cancellationToken)
    {
                var existingExpense = await expenseService.GetByIdAsync(
            model.Id, CanApproveExpenses(), CurrentIdentityUserId(), cancellationToken);
        if (existingExpense is null)
            return NotFound();
        if (existingExpense.Status != StaffManagement.Model.Enums.ExpenseStatus.Pending)
        {
            TempData["Error"] = "Reviewed expense records cannot be edited.";
            return RedirectToAction(nameof(Index));
        }        if (!ModelState.IsValid)
            return View(model);

        var result = await expenseService.UpdateAsync(
            model,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Expense record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ApproveGroup(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        string approvalNotes,
        CancellationToken cancellationToken) =>
        ReviewGroupAsync(employeeId, batchId, title, fromDate, toDate, approvalNotes, StaffManagement.Model.Enums.ExpenseStatus.Approved, cancellationToken);

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> RejectGroup(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        string approvalNotes,
        CancellationToken cancellationToken) =>
        ReviewGroupAsync(employeeId, batchId, title, fromDate, toDate, approvalNotes, StaffManagement.Model.Enums.ExpenseStatus.Rejected, cancellationToken);

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetByIdAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (expense is null)
            return NotFound();
        if (expense.Status != StaffManagement.Model.Enums.ExpenseStatus.Pending)
        {
            TempData["Error"] = "Reviewed expense records cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }
        return View(expense);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var existingExpense = await expenseService.GetByIdAsync(
            id, CanApproveExpenses(), CurrentIdentityUserId(), cancellationToken);
        var result = await expenseService.DeleteAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
            TempData["Error"] = result.Error;
        else
        {
            DeleteDocument(existingExpense?.DocumentPath);
            TempData["Success"] = "Expense record deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEmployeesAsync(CancellationToken cancellationToken)
    {
        if (CanApproveExpenses())
            ViewBag.Employees = await employeeService.GetAllAsync(cancellationToken);
    }

    private Task<EmployeeExpenseGroupDto?> GetExpenseGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken) =>
        expenseService.GetGroupAsync(
            employeeId, batchId, title, fromDate, toDate,
            CanApproveExpenses(), CurrentIdentityUserId(), cancellationToken);

    private async Task PopulateEmployeeIdAsync(
        CreateEmployeeExpenseDto model,
        string modelStateKey,
        CancellationToken cancellationToken)
    {
        if (CanApproveExpenses())
            return;

        var employee = await employeeService.GetByIdentityUserIdAsync(CurrentIdentityUserId(), cancellationToken);

        if (employee is not null)
        {
            model.EmployeeId = employee.Id;
            ModelState.Remove(modelStateKey);
        }
    }

    private void AddExpenseFieldError(string error, int index)
    {
        var field = error.Contains("employee", StringComparison.OrdinalIgnoreCase)
            ? nameof(CreateEmployeeExpenseDto.EmployeeId)
            : error.Contains("to date", StringComparison.OrdinalIgnoreCase)
                ? nameof(CreateEmployeeExpenseDto.ToDate)
                : error.Contains("bill amount", StringComparison.OrdinalIgnoreCase)
                    ? nameof(CreateEmployeeExpenseDto.BillAmount)
                    : error.Contains("amount", StringComparison.OrdinalIgnoreCase)
                        ? nameof(CreateEmployeeExpenseDto.Amount)
                        : error.Contains("bill number", StringComparison.OrdinalIgnoreCase)
                            ? nameof(CreateEmployeeExpenseDto.BillNumber)
                            : nameof(CreateEmployeeExpenseDto.Category);

        ModelState.AddModelError($"Expenses[{index}].{field}", error);
    }

    private async Task<(string? Path, string? Error)> SaveDocumentAsync(
        IFormFile? document,
        CancellationToken cancellationToken)
    {
        if (document is null || document.Length == 0)
            return (null, "Upload the bill document.");

        const long maxFileSize = 10 * 1024 * 1024;
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return (null, "Only PDF, Word, Excel, PNG, and JPEG files are allowed.");
        if (document.Length > maxFileSize)
            return (null, "The document cannot be larger than 10 MB.");

        var uploadDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "ExpenseDocuments");
        Directory.CreateDirectory(uploadDirectory);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, storedFileName);
        await using var stream = System.IO.File.Create(filePath);
        await document.CopyToAsync(stream, cancellationToken);
        return (storedFileName, null);
    }

    private void DeleteDocument(string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
            return;

        var filePath = Path.Combine(
            environment.ContentRootPath, "App_Data", "ExpenseDocuments", Path.GetFileName(documentPath));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }

    private async Task<IActionResult> ReviewGroupAsync(
        int employeeId,
        Guid? batchId,
        string title,
        DateTime fromDate,
        DateTime toDate,
        string approvalNotes,
        StaffManagement.Model.Enums.ExpenseStatus status,
        CancellationToken cancellationToken)
    {
        var result = await expenseService.ReviewGroupAsync(
            employeeId, batchId, title, fromDate, toDate, approvalNotes, status, cancellationToken);
        if (result.Succeeded)
            TempData["Success"] = status == StaffManagement.Model.Enums.ExpenseStatus.Approved
                ? $"Expense approved successfully. Approval notes: {approvalNotes.Trim()}"
                : $"Expense rejected successfully. Approval notes: {approvalNotes.Trim()}";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    private bool CanApproveExpenses() =>
        User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr);

    private string CurrentIdentityUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}
