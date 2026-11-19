using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.ProfileModel;
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
        return View(new EmployeeProfileDto
        {
            EmployeeId = employeeId ?? 0,
            ProfileInformation = new ProfileInformationDto
            {
                EmployeeId = employeeId ?? 0,
                DOB = employee?.DOB
            },
            AddressInformation = new AddressInformationDto { EmployeeId = employeeId ?? 0 }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeProfileDto model, CancellationToken cancellationToken, bool returnToProfile = false, bool returnToEditProfile = false)
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

        var profile = await profileService.GetFormByEmployeeIdAsync(employeeId, cancellationToken);
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
    public async Task<IActionResult> Edit(int employeeId, string section, EmployeeProfileDto model, CancellationToken cancellationToken, bool returnToProfile = false, bool returnToEditProfile = false)
    {
        if (!await CanEditProfileAsync(employeeId, cancellationToken))
            return Forbid();
        if (employeeId != model.EmployeeId)
            return BadRequest();
        var editSection = NormalizeSection(section);
        if (editSection is null)
            return BadRequest();

        RemoveEmployeeNavigationValidationErrors();
        RemoveUneditedSectionValidationErrors(editSection);
        if (editSection == "profile")
        {
            if (!model.ProfileInformation.Gender.HasValue)
                ModelState.AddModelError("ProfileInformation.Gender", "Select a gender.");
            if (string.IsNullOrWhiteSpace(model.ProfileInformation.MaritalStatus))
                ModelState.AddModelError("ProfileInformation.MaritalStatus", "Select a marital status.");
            if (!model.ProfileInformation.DOB.HasValue)
                ModelState.AddModelError("ProfileInformation.DOB", "Date of birth is required.");
            if (string.IsNullOrWhiteSpace(model.ProfileInformation.BloodGroup))
                ModelState.AddModelError("ProfileInformation.BloodGroup", "Select a blood group.");
            if (string.IsNullOrWhiteSpace(model.ProfileInformation.PhoneNumber))
                ModelState.AddModelError("ProfileInformation.PhoneNumber", "Phone number is required.");
        }
        if (!ModelState.IsValid)
        {
            var employee = await profileService.GetEmployeeByIdAsync(employeeId, cancellationToken);
            model.EmployeeName = employee is null ? null : $"{employee.FirstName} {employee.LastName}";
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

    private void RemoveUneditedSectionValidationErrors(string editSection)
    {
        var profileFields = new[]
        {
            "ProfileInformation.Gender", "ProfileInformation.MaritalStatus",
            "ProfileInformation.DOB", "ProfileInformation.BloodGroup",
            "ProfileInformation.PhoneNumber"
        };
        var emergencyFields = new[]
        {
            "ProfileInformation.EmergencyContactName",
            "ProfileInformation.EmergencyContactRelationship",
            "ProfileInformation.EmergencyContactPhone"
        };
        var addressFields = new[]
        {
            "AddressInformation.PermanentAddressLine1", "AddressInformation.PermanentAddressLine2",
            "AddressInformation.PermanentCountry", "AddressInformation.PermanentState",
            "AddressInformation.PermanentCity", "AddressInformation.PermanentPincode",
            "AddressInformation.CurrentAddressLine1", "AddressInformation.CurrentAddressLine2",
            "AddressInformation.CurrentCountry", "AddressInformation.CurrentState",
            "AddressInformation.CurrentCity", "AddressInformation.CurrentPincode"
        };

        var fieldsToValidate = editSection switch
        {
            "profile" => profileFields,
            "emergency" => emergencyFields,
            "address" => addressFields,
            _ => Array.Empty<string>()
        };

        var fieldsToKeep = fieldsToValidate.ToHashSet(StringComparer.Ordinal);
        var allFields = profileFields.Concat(emergencyFields).Concat(addressFields);

        foreach (var field in allFields.Where(field => !fieldsToKeep.Contains(field)))
            ModelState.Remove(field);
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


