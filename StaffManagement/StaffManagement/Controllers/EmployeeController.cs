using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;
using StaffManagement.Models;

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
    public async Task<IActionResult> Index(
        string? searchTerm,
        int? departmentId,
        string? designation,
        bool? isActive,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 10;
        page = Math.Max(1, page);

        var result = await employeeService.GetPageAsync(
            searchTerm, departmentId, designation, isActive, page, pageSize, cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)pageSize));
        if (page > totalPages)
        {
            page = totalPages;
            result = await employeeService.GetPageAsync(
                searchTerm, departmentId, designation, isActive, page, pageSize, cancellationToken);
        }

        return View(new EmployeeListViewModel
        {
            Employees = result.Items,
            Departments = await departmentService.GetAllAsync(),
            Designations = (await designationService.GetAllAsync())
                .Where(item => item.IsActive)
                .OrderBy(item => item.DesignationName)
                .ToList(),
            SearchTerm = searchTerm?.Trim(),
            DepartmentId = departmentId,
            Designation = designation,
            IsActive = isActive,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        });
    }

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
        await LoadReportingPersonsAsync();
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

        var reportingPersons = await LoadReportingPersonsAsync(model.ReportingPersonId);
        if (string.Equals(model.Role, RoleNames.User, StringComparison.OrdinalIgnoreCase) && !model.ReportingPersonId.HasValue)
            ModelState.AddModelError(nameof(model.ReportingPersonId), "Assign a reporting person to this employee.");
        else if (model.ReportingPersonId.HasValue && !reportingPersons.Any(person => person.Id == model.ReportingPersonId.Value))
            ModelState.AddModelError(nameof(model.ReportingPersonId), "Select an active employee with the Admin, HR, or Manager role.");

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
            await LoadReportingPersonsAsync(model.ReportingPersonId);
            return View("~/Views/Account/Create.cshtml", model);
        }

        TempData["Success"] = $"{model.FirstName} {model.LastName} was created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        await LoadEditChoicesAsync(employee?.ProjectIds);
        await LoadReportingPersonsAsync(employee?.ReportingPersonId);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeDto model, CancellationToken cancellationToken)
    {
        var existingEmployee = await employeeService.GetByIdAsync(model.Id, cancellationToken);
        if (existingEmployee is null)
            return NotFound();

        var canAssignReportingPerson = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr);
        if (!canAssignReportingPerson)
            model.ReportingPersonId = existingEmployee.ReportingPersonId;
        else
        {
            var reportingPersons = await LoadReportingPersonsAsync(model.ReportingPersonId);
            if (model.ReportingPersonId.HasValue &&
                (!reportingPersons.Any(person => person.Id == model.ReportingPersonId.Value) || model.ReportingPersonId == model.Id))
            {
                ModelState.AddModelError(nameof(model.ReportingPersonId), "Select a different active employee with the Admin, HR, or Manager role.");
            }
        }

        if (model.DepartmentId.HasValue)
        {
            var department = await departmentService.GetByIdAsync(model.DepartmentId.Value);
            if (department is null)
                ModelState.AddModelError(nameof(model.DepartmentId), "Select a valid department.");
            else
                model.Department = department.DepartmentName;
        }

        if (!CanAssignProjects())
        {
            var existing = await employeeService.GetByIdAsync(model.Id, cancellationToken);
            if (existing is null)
                return NotFound();

            model.ProjectIds = existing.ProjectIds;
        }

        if (!ModelState.IsValid)
        {
            await LoadReportingPersonsAsync(model.ReportingPersonId);
            await LoadEditChoicesAsync(model.ProjectIds);
            return View(model);
        }

        var result = await employeeService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await LoadEditChoicesAsync(model.ProjectIds);
            await LoadReportingPersonsAsync(model.ReportingPersonId);
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

    private async Task LoadEditChoicesAsync(IReadOnlyCollection<int>? selectedProjectIds = null)
    {
        await LoadProjectChoicesAsync(selectedProjectIds);
        ViewBag.Departments = await departmentService.GetAllForSelectionAsync();
        ViewBag.Designations = (await designationService.GetAllAsync())
            .Where(designation => designation.IsActive)
            .OrderBy(designation => designation.DesignationName)
            .ToList();
    }

    private async Task<IReadOnlyList<EmployeeDto>> LoadReportingPersonsAsync(int? selectedReportingPersonId = null)
    {
        var reportingRoles = new[] { RoleNames.Admin, RoleNames.Hr, RoleNames.Manager };
        var reportingUserIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in reportingRoles)
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            reportingUserIds.UnionWith(users.Select(user => user.Id));
        }

        var reportingPersons = (await employeeService.GetAllAsync())
            .Where(employee => employee.EmpIsActive &&
                employee.IdentityUserId is not null &&
                reportingUserIds.Contains(employee.IdentityUserId))
            .ToList();

        ViewBag.Managers = reportingPersons;
        ViewBag.SelectedReportingPersonId = selectedReportingPersonId;
        return reportingPersons;
    }
}
