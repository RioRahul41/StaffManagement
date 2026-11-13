using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class EmployeeController(
    IEmployeeService employeeService,
    IAccountService accountService,
    IDesignationService designationService,
    IDepartmentService departmentService,
    IProjectService projectService,
    UserManager<ApplicationIdentity> userManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await employeeService.GetAllAsync(cancellationToken));

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : View(employee);
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = accountService.GetCreatableRoles(GetCreatorRoles());
        ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
        await LoadDepartmentsAsync();
        return View("~/Views/Account/Create.cshtml", new CreateAccountDto());
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto model, CancellationToken cancellationToken)
    {
        var creatorRoles = GetCreatorRoles();
        var allowedRoles = accountService.GetCreatableRoles(creatorRoles);
        if (!allowedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Role), "You cannot create an account with this role.");

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = allowedRoles;
            ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
            await LoadDepartmentsAsync(model.DepartmentId);
            return View("~/Views/Account/Create.cshtml", model);
        }

        var result = await accountService.CreateAsync(model, creatorRoles, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Roles = allowedRoles;
            ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
            await LoadDepartmentsAsync(model.DepartmentId);
            return View("~/Views/Account/Create.cshtml", model);
        }

        TempData["Success"] = $"{model.FirstName} {model.LastName} was created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        await LoadProjectChoicesAsync(employee?.ProjectIds);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeDto model, CancellationToken cancellationToken)
    {
        if (!CanAssignProjects())
        {
            var existing = await employeeService.GetByIdAsync(model.Id, cancellationToken);
            if (existing is null)
                return NotFound();

            model.ProjectIds = existing.ProjectIds;
        }

        if (!ModelState.IsValid)
        {
            await LoadProjectChoicesAsync(model.ProjectIds);
            return View(model);
        }

        var result = await employeeService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await LoadProjectChoicesAsync(model.ProjectIds);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.IdentityUserId))
        {
            var user = await userManager.FindByIdAsync(model.IdentityUserId);
            if (user is not null)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.Phone;
                user.Location = model.Location;
                await userManager.UpdateAsync(user);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    [HttpGet]
    public async Task<IActionResult> AssignProjects(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        if (employee is null)
            return NotFound();

        ViewBag.Projects = await projectService.GetAllAsync();
        return View(employee);
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignProjects(
        int id,
        List<int> projectIds,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        if (employee is null)
            return NotFound();

        employee.ProjectIds = projectIds.Distinct().ToList();
        var result = await employeeService.UpdateAsync(employee, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Projects = await projectService.GetAllAsync();
            return View(employee);
        }

        TempData["Success"] = $"Projects assigned to {employee.FirstName} {employee.LastName}.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await employeeService.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    private IReadOnlyCollection<string> GetCreatorRoles() =>
        RoleNames.All.Where(role => User.IsInRole(role)).ToArray();

    private async Task LoadDepartmentsAsync(int? selectedDepartmentId = null)
    {
        ViewBag.Departments = await departmentService.GetAllForSelectionAsync()
            ?? Array.Empty<Department>();
        ViewBag.SelectedDepartmentId = selectedDepartmentId;
    }

    private bool CanAssignProjects() =>
        User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Manager);

    private async Task LoadProjectChoicesAsync(IReadOnlyCollection<int>? selectedProjectIds)
    {
        if (!CanAssignProjects())
            return;

        ViewBag.Projects = await projectService.GetAllAsync();
        ViewBag.SelectedProjectIds = selectedProjectIds ?? Array.Empty<int>();
    }
}
