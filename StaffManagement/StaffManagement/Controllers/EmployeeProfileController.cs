using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class EmployeeProfileController(IEmployeeProfileService profileService, IEmployeeExperienceService experienceService, IEmployeeBankDetailService bankDetailService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profiles = await profileService.GetAllAsync(cancellationToken);
        return View(profiles);
    }

    public async Task<IActionResult> Details(int employeeId, CancellationToken cancellationToken)
    {
        var profile = await profileService.GetByEmployeeIdAsync(employeeId, asNoTracking: true, cancellationToken);
        if (profile is null)
            return NotFound();

        ViewBag.OtherDetails = await profileService.GetOtherDetailsAsync(employeeId, cancellationToken);
        ViewBag.EmployeeExperiences = await experienceService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        ViewBag.BankDetails = await bankDetailService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        ViewBag.CanManageOtherDetails = await CanEditProfileAsync(employeeId, cancellationToken);
        ViewBag.CanEditProfile = ViewBag.CanManageOtherDetails;
        ViewBag.CanDeleteProfile = CanManageAllProfiles();
        ViewData["Title"] = "Profile Information";
        return View(profile);
    }

    public async Task<IActionResult> Create(int? employeeId, CancellationToken cancellationToken, bool returnToProfile = false, bool returnToEditProfile = false)
    {
        var canManageAll = CanManageAllProfiles();
        if (!canManageAll)
        {
            var ownEmployeeId = await GetOwnEmployeeIdAsync(cancellationToken);
            if (!ownEmployeeId.HasValue)
                return Forbid();
            employeeId = ownEmployeeId.Value;
        }

        if (employeeId.HasValue && await profileService.EmployeeHasProfileAsync(employeeId.Value, cancellationToken))
            return RedirectToAction(nameof(Details), new { employeeId = employeeId.Value });

        if (canManageAll)
            await LoadEmployeesAsync(employeeId, cancellationToken);
        ViewBag.CanChooseEmployee = canManageAll && !employeeId.HasValue;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewData["Title"] = "Create profile information";
        var employee = employeeId.HasValue ? await profileService.GetEmployeeByIdAsync(employeeId.Value, cancellationToken) : null;
        return View(new EmployeeProfile { EmployeeId = employeeId ?? 0, DOB = employee?.DOB });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeProfile model, CancellationToken cancellationToken, bool returnToProfile = false, bool returnToEditProfile = false)
    {
        var canManageAll = CanManageAllProfiles();
        if (!canManageAll)
        {
            var ownEmployeeId = await GetOwnEmployeeIdAsync(cancellationToken);
            if (!ownEmployeeId.HasValue)
                return Forbid();
            model.EmployeeId = ownEmployeeId.Value;
        }

        RemoveEmployeeNavigationValidationErrors();
        if (!ModelState.IsValid)
        {
            await PrepareCreateViewAsync(model.EmployeeId, canManageAll, returnToProfile, returnToEditProfile, cancellationToken);
            return View(model);
        }

        var result = await profileService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.EmployeeId), result.Error!);
            await PrepareCreateViewAsync(model.EmployeeId, canManageAll, returnToProfile, returnToEditProfile, cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Employee profile information was created.";
        if (returnToEditProfile) return RedirectToAction("EditProfile", "Account", new { section = "profile" });
        return returnToProfile ? RedirectToAction("Profile", "Account") : RedirectToAction(nameof(Details), new { employeeId = model.EmployeeId });
    }

    public async Task<IActionResult> Edit(int employeeId, CancellationToken cancellationToken, string section = "profile", bool returnToProfile = false, bool returnToEditProfile = false)
    {
        if (!await CanEditProfileAsync(employeeId, cancellationToken))
            return Forbid();
        var editSection = NormalizeSection(section);
        if (editSection is null)
            return BadRequest();

        var profile = await profileService.GetByEmployeeIdAsync(employeeId, cancellationToken: cancellationToken);
        if (profile is null)
            return NotFound();

        ViewBag.EditSection = editSection;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewData["Title"] = GetEditTitle(editSection);
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int employeeId, string section, EmployeeProfile model, CancellationToken cancellationToken, bool returnToProfile = false, bool returnToEditProfile = false)
    {
        if (!await CanEditProfileAsync(employeeId, cancellationToken))
            return Forbid();
        if (employeeId != model.EmployeeId)
            return BadRequest();
        var editSection = NormalizeSection(section);
        if (editSection is null)
            return BadRequest();

        RemoveEmployeeNavigationValidationErrors();
        if (!ModelState.IsValid)
        {
            model.Employee = await profileService.GetEmployeeByIdAsync(employeeId, cancellationToken) ?? null!;
            ViewBag.EditSection = editSection;
            ViewBag.ReturnToProfile = returnToProfile;
            ViewBag.ReturnToEditProfile = returnToEditProfile;
            ViewData["Title"] = GetEditTitle(editSection);
            return View(model);
        }

        if (!await profileService.UpdateSectionAsync(employeeId, model, editSection, cancellationToken))
            return NotFound();

        TempData["Success"] = editSection switch
        {
            "emergency" => "Emergency contact information was updated.",
            "address" => "Address information was updated.",
            _ => "Employee profile information was updated."
        };
        if (returnToEditProfile) return RedirectToAction("EditProfile", "Account", new { section = editSection == "address" ? "address" : "profile" });
        return returnToProfile
            ? RedirectToAction("Profile", "Account")
            : RedirectToAction(nameof(Details), new { employeeId });
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    public async Task<IActionResult> Delete(int employeeId, CancellationToken cancellationToken)
    {
        var profile = await profileService.GetByEmployeeIdAsync(employeeId, asNoTracking: true, cancellationToken);
        if (profile is null)
            return NotFound();

        ViewData["Title"] = "Delete profile information";
        return View(profile);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int employeeId, CancellationToken cancellationToken)
    {
        if (!await profileService.DeleteAsync(employeeId, cancellationToken))
            return NotFound();

        TempData["Success"] = "Employee profile information was deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PrepareCreateViewAsync(int? selectedId, bool canManageAll, bool returnToProfile, bool returnToEditProfile, CancellationToken cancellationToken)
    {
        if (canManageAll)
            await LoadEmployeesAsync(selectedId, cancellationToken);
        ViewBag.CanChooseEmployee = canManageAll && !selectedId.HasValue;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewData["Title"] = "Create profile information";
    }

    private void RemoveEmployeeNavigationValidationErrors()
    {
        foreach (var key in ModelState.Keys.Where(key => key == nameof(EmployeeProfile.Employee) || key.StartsWith(nameof(EmployeeProfile.Employee) + ".", StringComparison.Ordinal)).ToList())
            ModelState.Remove(key);
    }

    private static string? NormalizeSection(string? section) =>
        string.Equals(section, "emergency", StringComparison.OrdinalIgnoreCase) ? "emergency" :
        string.Equals(section, "profile", StringComparison.OrdinalIgnoreCase) ? "profile" :
        string.Equals(section, "address", StringComparison.OrdinalIgnoreCase) ? "address" : null;

    private static string GetEditTitle(string section) => section switch
    {
        "emergency" => "Edit emergency contact",
        "address" => "Edit address information",
        _ => "Edit profile information"
    };

    private bool CanManageAllProfiles() =>
        User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager);

    private async Task<bool> CanEditProfileAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (CanManageAllProfiles())
            return true;
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(identityUserId) &&
               await profileService.CanEditAsync(employeeId, identityUserId, cancellationToken);
    }

    private async Task<int?> GetOwnEmployeeIdAsync(CancellationToken cancellationToken)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(identityUserId)
            ? null
            : await profileService.GetOwnEmployeeIdAsync(identityUserId, cancellationToken);
    }

    private async Task LoadEmployeesAsync(int? selectedId, CancellationToken cancellationToken)
    {
        ViewBag.Employees = (await profileService.GetActiveEmployeesAsync(cancellationToken))
            .Select(employee => new { employee.Id, Name = employee.FirstName + " " + employee.LastName, employee.Email })
            .ToList();
        ViewBag.SelectedEmployeeId = selectedId;
    }



}


