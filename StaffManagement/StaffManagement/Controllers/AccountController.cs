using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

public sealed class AccountController(
    SignInManager<ApplicationIdentity> signInManager,
    UserManager<ApplicationIdentity> userManager,
    IAccountService accountService,
    IDesignationService designationService,
    IDepartmentService departmentService,
    IEmployeeService employeeService) : Controller
{
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginDto { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.UserIsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return RedirectToLocal(returnUrl ?? model.ReturnUrl);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        return employee is null ? NotFound() : View(employee);
    }

    [Authorize]
    public async Task<IActionResult> EditProfile(CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        return employee is null ? NotFound() : View(employee);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EmployeeDto model, CancellationToken cancellationToken)
    {
        var currentEmployee = await GetCurrentEmployeeAsync(cancellationToken);
        if (currentEmployee is null)
            return NotFound();

        model.Id = currentEmployee.Id;
        model.IdentityUserId = currentEmployee.IdentityUserId;
        model.DepartmentId = currentEmployee.DepartmentId;
        model.Department = currentEmployee.Department;
        model.Designation = currentEmployee.Designation;
        model.Salary = currentEmployee.Salary;
        model.ProjectIds = currentEmployee.ProjectIds;
        model.EmpIsActive = currentEmployee.EmpIsActive;

        // These employee-management fields are intentionally not editable in the
        // self-service profile form. Their values are restored above, so remove
        // binder validation entries for fields that were not posted by the form.
        ModelState.Remove(nameof(EmployeeDto.DepartmentId));
        ModelState.Remove(nameof(EmployeeDto.Department));
        ModelState.Remove(nameof(EmployeeDto.Designation));
        ModelState.Remove(nameof(EmployeeDto.Salary));
        ModelState.Remove(nameof(EmployeeDto.ProjectIds));
        ModelState.Remove(nameof(EmployeeDto.EmpIsActive));

        if (!ModelState.IsValid)
            return View(model);

        var result = await employeeService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user is not null)
        {
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.Phone;
            user.Location = model.Location;

            var identityResult = await userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            await signInManager.RefreshSignInAsync(user);
        }

        TempData["Success"] = "Your profile was updated.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordDto());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    public async Task<IActionResult> Create()
    {
        var roles = GetCreatorRoles();
        ViewBag.Roles = accountService.GetCreatableRoles(roles);
        ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
        await LoadDepartmentsAsync();
        await LoadReportingPersonsAsync();
        return View(new CreateAccountDto());
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
            await LoadReportingPersonsAsync(model.ReportingPersonId);
            return View(model);
        }

        var result = await accountService.CreateAsync(model, creatorRoles, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Roles = allowedRoles;
            ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
            await LoadDepartmentsAsync(model.DepartmentId);
            await LoadReportingPersonsAsync(model.ReportingPersonId);
            return View(model);
        }

        TempData["Success"] = $"{model.FirstName} {model.LastName} was created with the {model.Role} role.";
        return RedirectToAction("Index", "Employee");
    }

    private async Task LoadReportingPersonsAsync(int? selectedReportingPersonId = null)
    {
        var reportingUsers = await userManager.GetUsersInRoleAsync(RoleNames.Manager);
        var reportingUserIds = reportingUsers.Select(user => user.Id).ToHashSet(StringComparer.Ordinal);

        ViewBag.Managers = (await employeeService.GetAllAsync())
            .Where(employee => employee.EmpIsActive &&
                employee.IdentityUserId is not null &&
                reportingUserIds.Contains(employee.IdentityUserId))
            .ToList();
        ViewBag.SelectedReportingPersonId = selectedReportingPersonId;
    }

    private async Task LoadDepartmentsAsync(int? selectedDepartmentId = null)
    {
        ViewBag.Departments = await departmentService.GetAllForSelectionAsync()
            ?? Array.Empty<Department>();
        ViewBag.SelectedDepartmentId = selectedDepartmentId;
    }

    private IReadOnlyCollection<string> GetCreatorRoles() =>
        RoleNames.All.Where(role => User.IsInRole(role)).ToArray();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home")!;

    private Task<EmployeeDto?> GetCurrentEmployeeAsync(CancellationToken cancellationToken) =>
        employeeService.GetByIdentityUserIdAsync(userManager.GetUserId(User)!, cancellationToken);
}
