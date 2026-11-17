using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

public sealed class AccountController(
    SignInManager<ApplicationIdentity> signInManager,
    UserManager<ApplicationIdentity> userManager,
    IAccountService accountService,
    IDesignationService designationService,
    IDepartmentService departmentService,
    IEmployeeService employeeService,
    IWebHostEnvironment environment,
    ApplicationDbContext dbContext) : Controller
{
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginDto { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

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
        {
            var celebration = await employeeCelebrationService.GetAndRecordTodayGreetingAsync(
                user.Id,
                HttpContext.RequestAborted);
            if (celebration is not null)
                return View("~/Views/Celebration/Index.cshtml", celebration);

            return RedirectToLocal(returnUrl ?? model.ReturnUrl);
        }

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
        if (employee is null)
            return NotFound();
        ViewBag.ProfileInformation = await dbContext.EmployeeProfiles.AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.EmployeeId == employee.Id, cancellationToken);
        ViewBag.OtherDetails = await dbContext.EmployeeOtherDetails.AsNoTracking()
            .Where(detail => detail.EmployeeId == employee.Id)
            .OrderBy(detail => detail.DocumentType)
            .ToListAsync(cancellationToken);
        return View(employee);
    }

    [Authorize]
    public async Task<IActionResult> ProfilePhoto(CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee is null || string.IsNullOrWhiteSpace(employee.ProfilePhotoPath))
            return NotFound();

        var fileName = Path.GetFileName(employee.ProfilePhotoPath);
        var directory = Path.Combine(environment.ContentRootPath, "App_Data", "ProfilePhotos");
        var fullPath = Path.GetFullPath(Path.Combine(directory, fileName));
        var rootPath = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };
        if (contentType is null)
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(fullPath, contentType);
    }

    [Authorize]
    public async Task<IActionResult> EditProfile(CancellationToken cancellationToken)
    {
        var employee = await GetCurrentEmployeeAsync(cancellationToken);
        if (employee is null)
            return NotFound();
        return View(employee);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EmployeeDto model, IFormFile? profilePhoto, CancellationToken cancellationToken)
    {
        var currentEmployee = await GetCurrentEmployeeAsync(cancellationToken);
        if (currentEmployee is null)
            return NotFound();

        model.Id = currentEmployee.Id;
        model.IdentityUserId = currentEmployee.IdentityUserId;
        model.Email = currentEmployee.Email;
        model.ProfilePhotoPath = currentEmployee.ProfilePhotoPath;
        model.Gender = currentEmployee.Gender;
        model.DOB = currentEmployee.DOB;
        model.MaritalStatus = currentEmployee.MaritalStatus;
        model.BloodGroup = currentEmployee.BloodGroup;
        model.EmergencyContactName = currentEmployee.EmergencyContactName;
        model.EmergencyContactRelationship = currentEmployee.EmergencyContactRelationship;
        model.EmergencyContactPhone = currentEmployee.EmergencyContactPhone;
        model.DepartmentId = currentEmployee.DepartmentId;
        model.Department = currentEmployee.Department;
        model.Designation = currentEmployee.Designation;
        model.Salary = currentEmployee.Salary;
        model.ProjectIds = currentEmployee.ProjectIds;
        model.EmpIsActive = currentEmployee.EmpIsActive;
        model.SickLeaveAllowance = currentEmployee.SickLeaveAllowance;
        model.CasualLeaveAllowance = currentEmployee.CasualLeaveAllowance;

        // These employee-management fields are intentionally not editable in the
        // self-service profile form. Their values are restored above, so remove
        // binder validation entries for fields that were not posted by the form.
        ModelState.Remove(nameof(EmployeeDto.Email));
        ModelState.Remove(nameof(EmployeeDto.ProfilePhotoPath));
        ModelState.Remove(nameof(EmployeeDto.Gender));
        ModelState.Remove(nameof(EmployeeDto.DOB));
        ModelState.Remove(nameof(EmployeeDto.MaritalStatus));
        ModelState.Remove(nameof(EmployeeDto.BloodGroup));
        ModelState.Remove(nameof(EmployeeDto.EmergencyContactName));
        ModelState.Remove(nameof(EmployeeDto.EmergencyContactRelationship));
        ModelState.Remove(nameof(EmployeeDto.EmergencyContactPhone));
        ModelState.Remove(nameof(EmployeeDto.DepartmentId));
        ModelState.Remove(nameof(EmployeeDto.Department));
        ModelState.Remove(nameof(EmployeeDto.Designation));
        ModelState.Remove(nameof(EmployeeDto.Salary));
        ModelState.Remove(nameof(EmployeeDto.ProjectIds));
        ModelState.Remove(nameof(EmployeeDto.EmpIsActive));
        ModelState.Remove(nameof(EmployeeDto.SickLeaveAllowance));
        ModelState.Remove(nameof(EmployeeDto.CasualLeaveAllowance));

        if (!ModelState.IsValid)
            return View(model);

        string? newProfilePhotoPath = null;
        if (profilePhoto is { Length: > 0 })
        {
            var savedPhoto = await SaveProfilePhotoAsync(profilePhoto, cancellationToken);
            if (savedPhoto.Error is not null)
            {
                ModelState.AddModelError(nameof(profilePhoto), savedPhoto.Error);
                return View(model);
            }
            newProfilePhotoPath = savedPhoto.FileName;
            model.ProfilePhotoPath = newProfilePhotoPath;
        }

        var result = await employeeService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            DeleteProfilePhoto(newProfilePhotoPath);
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

        if (!string.IsNullOrWhiteSpace(newProfilePhotoPath))
            DeleteProfilePhoto(currentEmployee.ProfilePhotoPath);

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

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
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
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto model, CancellationToken cancellationToken)
    {
        var creatorRoles = GetCreatorRoles();
        var allowedRoles = accountService.GetCreatableRoles(creatorRoles);
        var reportingPersons = await LoadReportingPersonsAsync(model.ReportingPersonId);
        if (!allowedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Role), "You cannot create an account with this role.");
        if (RoleNames.RequiresReportingPerson(model.Role) && !model.ReportingPersonId.HasValue)
            ModelState.AddModelError(nameof(model.ReportingPersonId), "Assign a reporting person for this role.");
        else if (model.ReportingPersonId.HasValue)
        {
            var personRoles = ViewBag.ReportingPersonRoles as IReadOnlyDictionary<int, string> ?? new Dictionary<int, string>();
            var allowedReportingRoles = RoleNames.ReportingRolesFor(model.Role);
            if (!reportingPersons.Any(person => person.Id == model.ReportingPersonId.Value) ||
                !personRoles.TryGetValue(model.ReportingPersonId.Value, out var selectedRole) ||
                !allowedReportingRoles.Contains(selectedRole, StringComparer.OrdinalIgnoreCase))
            {
                var allowedRoleText = allowedReportingRoles.Count == 0 ? "none" : string.Join(" or ", allowedReportingRoles);
                ModelState.AddModelError(nameof(model.ReportingPersonId), $"{model.Role} accounts can only report to: {allowedRoleText}.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = allowedRoles;
            ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
            await LoadDepartmentsAsync(model.DepartmentId);
            return View(model);
        }

        var result = await accountService.CreateAsync(model, creatorRoles, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Roles = allowedRoles;
            ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
            await LoadDepartmentsAsync(model.DepartmentId);
            return View(model);
        }

        TempData["Success"] = $"{model.FirstName} {model.LastName} was created with the {model.Role} role.";
        return RedirectToAction("Index", "Employee");
    }

    private async Task<IReadOnlyList<EmployeeDto>> LoadReportingPersonsAsync(int? selectedReportingPersonId = null)
    {
        var reportingUserIds = new HashSet<string>(StringComparer.Ordinal);
        var reportingRolesByUserId = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var role in new[] { RoleNames.Admin, RoleNames.Hr, RoleNames.Manager })
        {
            var reportingUsers = await userManager.GetUsersInRoleAsync(role);
            reportingUserIds.UnionWith(reportingUsers.Select(user => user.Id));
            foreach (var user in reportingUsers)
                reportingRolesByUserId[user.Id] = role;
        }

        var people = (await employeeService.GetAllAsync())
            .Where(employee => employee.EmpIsActive &&
                employee.IdentityUserId is not null &&
                reportingUserIds.Contains(employee.IdentityUserId))
            .ToList();
        ViewBag.Managers = people;
        ViewBag.ReportingPersonRoles = people.ToDictionary(
            employee => employee.Id,
            employee => reportingRolesByUserId[employee.IdentityUserId!]);
        ViewBag.SelectedReportingPersonId = selectedReportingPersonId;
        return people;
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

    private async Task<(string? FileName, string? Error)> SaveProfilePhotoAsync(IFormFile photo, CancellationToken cancellationToken)
    {
        const long maxFileSize = 5 * 1024 * 1024;
        if (photo.Length > maxFileSize)
            return (null, "Profile photos must be 5 MB or smaller.");

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            return (null, "Choose a JPG, PNG, or WebP image.");

        await using var input = photo.OpenReadStream();
        var header = new byte[12];
        var bytesRead = await input.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        var isJpeg = (extension is ".jpg" or ".jpeg") && bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = extension == ".png" && bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isWebp = extension == ".webp" && bytesRead >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
        if (!isJpeg && !isPng && !isWebp)
            return (null, "The selected file is not a valid JPG, PNG, or WebP image.");

        input.Position = 0;
        var directory = Path.Combine(environment.ContentRootPath, "App_Data", "ProfilePhotos");
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var output = System.IO.File.Create(Path.Combine(directory, fileName));
        await input.CopyToAsync(output, cancellationToken);
        return (fileName, null);
    }

    private void DeleteProfilePhoto(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var safeFileName = Path.GetFileName(fileName);
        var path = Path.Combine(environment.ContentRootPath, "App_Data", "ProfilePhotos", safeFileName);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }
    private Task<EmployeeDto?> GetCurrentEmployeeAsync(CancellationToken cancellationToken) =>
        employeeService.GetByIdentityUserIdAsync(userManager.GetUserId(User)!, cancellationToken);
}





