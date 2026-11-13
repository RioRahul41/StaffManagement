using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.ExpenseModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class ExpenseController(
    IEmployeeExpenseService expenseService,
    IEmployeeService employeeService) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await expenseService.GetPagedAsync(
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

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateEmployeeExpenseDto();
        await PopulateFirstNameAsync(model, cancellationToken);
        await LoadEmployeesAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateEmployeeExpenseDto model,
        CancellationToken cancellationToken)
    {
        await PopulateFirstNameAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync(cancellationToken);
            return View(model);
        }

        var result = await expenseService.CreateAsync(
            model,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await LoadEmployeesAsync(cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Expense record created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetByIdAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        return expense is null ? NotFound() : View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EmployeeExpenseDto model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await expenseService.UpdateAsync(
            model,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = "Expense record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var expense = await expenseService.GetByIdAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        return expense is null ? NotFound() : View(expense);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await expenseService.DeleteAsync(
            id,
            CanApproveExpenses(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
            TempData["Error"] = result.Error;
        else
            TempData["Success"] = "Expense record deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEmployeesAsync(CancellationToken cancellationToken)
    {
        if (CanApproveExpenses())
            ViewBag.Employees = await employeeService.GetAllAsync(cancellationToken);
    }

    private async Task PopulateFirstNameAsync(
        CreateEmployeeExpenseDto model,
        CancellationToken cancellationToken)
    {
        var employee = CanApproveExpenses() && model.EmployeeId.HasValue
            ? await employeeService.GetByIdAsync(model.EmployeeId.Value, cancellationToken)
            : await employeeService.GetByIdentityUserIdAsync(CurrentIdentityUserId(), cancellationToken);

        if (employee is not null)
        {
            model.FirstName = employee.FirstName;
            ModelState.Remove(nameof(model.FirstName));
        }
    }

    private bool CanApproveExpenses() =>
        User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr);

    private string CurrentIdentityUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}
