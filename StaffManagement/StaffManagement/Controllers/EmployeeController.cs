using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using Microsoft.EntityFrameworkCore;
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
    UserManager<ApplicationIdentity> userManager,
    IWebHostEnvironment environment,
    ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(new EmployeeListViewModel
        {
            Departments = await departmentService.GetAllAsync(),
            Designations = (await designationService.GetAllAsync())
                .Where(item => item.IsActive)
                .OrderBy(item => item.DesignationName)
                .ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Data(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortDirection = null,
        int? departmentId = null,
        string? designation = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var pageStart = Math.Max(start, 0);
        var page = await employeeService.GetDataTablePageAsync(
            new EmployeeDataTableQueryDto
            {
                Start = pageStart,
                Length = length,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortAscending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase),
                DepartmentId = departmentId,
                Designation = designation,
                IsActive = isActive
            },
            cancellationToken);

        var canEdit = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Manager);
        var canDelete = User.IsInRole(RoleNames.Admin);
        var canAssignProjects = canEdit;

        return Json(new
        {
            draw = Math.Max(draw, 0),
            recordsTotal = page.TotalCount,
            recordsFiltered = page.FilteredCount,
            data = page.Items.Select((employee, index) => new
            {
                serialNumber = pageStart + index + 1,
                id = employee.Id,
                firstName = employee.FirstName,
                lastName = employee.LastName,
                designation = employee.Designation,
                email = employee.Email,
                department = employee.Department,
                assignedProjects = employee.AssignedProjects.Count == 0
                    ? "None"
                    : string.Join(", ", employee.AssignedProjects.Select(project => project.ProjectName)),
                reportingPersonName = employee.ReportingPersonName,
                isLinked = employee.IdentityUserId is not null,
                isActive = employee.EmpIsActive,
                canEdit,
                canDelete,
                canAssignProjects,
                viewUrl = Url.Action(nameof(Details), new { id = employee.Id }),
                editUrl = Url.Action(nameof(Edit), new { id = employee.Id }),
                deleteUrl = Url.Action(nameof(Delete), new { id = employee.Id }),
                assignProjectsUrl = Url.Action(nameof(AssignProjects), new { id = employee.Id })
            })
        });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        if (employee is null)
            return NotFound();
        ViewBag.HasProfileInformation = await dbContext.EmployeeProfiles.AnyAsync(profile => profile.EmployeeId == id, cancellationToken);
        return View(employee);
    }

    public async Task<IActionResult> ProfilePhoto(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
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
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = accountService.GetCreatableRoles(GetCreatorRoles());
        ViewBag.Designations = designationService.GetAllAsync().Result.Where(d => d.IsActive).ToList();
        await LoadDepartmentsAsync();
        var model = new CreateAccountDto();
        var reportingPersons = await LoadReportingPersonsAsync();
        if (IsManagerOnly() && reportingPersons.Count > 0)
            model.ReportingPersonId = reportingPersons[0].Id;
        return View("~/Views/Account/Create.cshtml", model);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto model, CancellationToken cancellationToken)
    {
        var creatorRoles = GetCreatorRoles();
        var allowedRoles = accountService.GetCreatableRoles(creatorRoles);
        if (!allowedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Role), "You cannot create an account with this role.");

        // Managers can create Users only for their own team. Do not trust a posted reporting person.
        if (IsManagerOnly() && string.Equals(model.Role, RoleNames.User, StringComparison.OrdinalIgnoreCase))
        {
            var creator = await employeeService.GetByIdentityUserIdAsync(userManager.GetUserId(User)!, cancellationToken);
            if (creator is null || !creator.EmpIsActive)
                ModelState.AddModelError(string.Empty, "Your active employee profile could not be found, so an employee cannot be created.");
            else
                model.ReportingPersonId = creator.Id;
        }

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
            await LoadReportingPersonsAsync(model.ReportingPersonId);
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
        ViewBag.TargetEmployeeRole = await GetEmployeeRoleAsync(employee);
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

        model.PersonalEmail = existingEmployee.PersonalEmail;
        model.Skills = existingEmployee.Skills;
        model.ProfilePhotoPath = existingEmployee.ProfilePhotoPath;
        model.Gender = existingEmployee.Gender;
        model.DOB = existingEmployee.DOB;
        model.MaritalStatus = existingEmployee.MaritalStatus;
        model.BloodGroup = existingEmployee.BloodGroup;
        model.EmergencyContactName = existingEmployee.EmergencyContactName;
        model.EmergencyContactRelationship = existingEmployee.EmergencyContactRelationship;
        model.EmergencyContactPhone = existingEmployee.EmergencyContactPhone;
        model.SickLeaveAllowance = existingEmployee.SickLeaveAllowance;
        model.CasualLeaveAllowance = existingEmployee.CasualLeaveAllowance;
        ModelState.Remove(nameof(EmployeeDto.PersonalEmail));
        ModelState.Remove(nameof(EmployeeDto.Skills));
        ModelState.Remove(nameof(EmployeeDto.ProfilePhotoPath));
        ModelState.Remove(nameof(EmployeeDto.Gender));
        ModelState.Remove(nameof(EmployeeDto.DOB));
        ModelState.Remove(nameof(EmployeeDto.MaritalStatus));
        ModelState.Remove(nameof(EmployeeDto.BloodGroup));
        ModelState.Remove(nameof(EmployeeDto.EmergencyContactName));
        ModelState.Remove(nameof(EmployeeDto.EmergencyContactRelationship));
        ModelState.Remove(nameof(EmployeeDto.EmergencyContactPhone));
        ModelState.Remove(nameof(EmployeeDto.SickLeaveAllowance));
        ModelState.Remove(nameof(EmployeeDto.CasualLeaveAllowance));

        var canAssignReportingPerson = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr);
        var targetEmployeeRole = await GetEmployeeRoleAsync(existingEmployee);
        ViewBag.TargetEmployeeRole = targetEmployeeRole;
        if (!canAssignReportingPerson)
            model.ReportingPersonId = existingEmployee.ReportingPersonId;
        else
        {
            var reportingPersons = await LoadReportingPersonsAsync(model.ReportingPersonId);
            var allowedReportingRoles = RoleNames.ReportingRolesFor(targetEmployeeRole);
            var personRoles = ViewBag.ReportingPersonRoles as IReadOnlyDictionary<int, string> ?? new Dictionary<int, string>();
            if (RoleNames.RequiresReportingPerson(targetEmployeeRole) && !model.ReportingPersonId.HasValue)
                ModelState.AddModelError(nameof(model.ReportingPersonId), "Assign a reporting person for this role.");
            else if (model.ReportingPersonId.HasValue &&
                (!reportingPersons.Any(person => person.Id == model.ReportingPersonId.Value) ||
                 model.ReportingPersonId == model.Id ||
                 !personRoles.TryGetValue(model.ReportingPersonId.Value, out var selectedRole) ||
                 !allowedReportingRoles.Contains(selectedRole, StringComparer.OrdinalIgnoreCase)))
            {
                var allowedRoleText = allowedReportingRoles.Count == 0 ? "none" : string.Join(" or ", allowedReportingRoles);
                ModelState.AddModelError(nameof(model.ReportingPersonId), $"This account can only report to: {allowedRoleText}.");
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
        if (employee is null)
            return NotFound();
        ViewBag.HasProfileInformation = await dbContext.EmployeeProfiles.AnyAsync(profile => profile.EmployeeId == id, cancellationToken);
        return View(employee);
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

    private bool IsManagerOnly() =>
        User.IsInRole(RoleNames.Manager) &&
        !User.IsInRole(RoleNames.Admin) &&
        !User.IsInRole(RoleNames.Hr);

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
        var reportingRolesByUserId = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var role in reportingRoles)
        {
            var users = await userManager.GetUsersInRoleAsync(role);
            reportingUserIds.UnionWith(users.Select(user => user.Id));
            foreach (var user in users)
                reportingRolesByUserId[user.Id] = role;
        }

        var reportingPersons = (await employeeService.GetAllAsync())
            .Where(employee => employee.EmpIsActive &&
                employee.IdentityUserId is not null &&
                reportingUserIds.Contains(employee.IdentityUserId))
            .ToList();

        if (IsManagerOnly())
        {
            var currentUserId = userManager.GetUserId(User);
            reportingPersons = reportingPersons
                .Where(employee => employee.IdentityUserId == currentUserId)
                .ToList();
        }

        ViewBag.Managers = reportingPersons;
        ViewBag.ReportingPersonRoles = reportingPersons.ToDictionary(
            employee => employee.Id,
            employee => reportingRolesByUserId[employee.IdentityUserId!]);
        ViewBag.SelectedReportingPersonId = selectedReportingPersonId;
        return reportingPersons;
    }

    private async Task<string?> GetEmployeeRoleAsync(EmployeeDto? employee)
    {
        if (string.IsNullOrWhiteSpace(employee?.IdentityUserId))
            return null;
        var user = await userManager.FindByIdAsync(employee.IdentityUserId);
        if (user is null)
            return null;
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault(role => RoleNames.All.Contains(role, StringComparer.OrdinalIgnoreCase));
    }
}




