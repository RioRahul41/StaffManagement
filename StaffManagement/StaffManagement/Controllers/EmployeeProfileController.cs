using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class EmployeeProfileController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profiles = await dbContext.EmployeeProfiles
            .AsNoTracking()
            .Include(profile => profile.Employee)
            .Where(profile => !profile.Employee.EmpIsDeleted)
            .OrderBy(profile => profile.Employee.LastName)
            .ThenBy(profile => profile.Employee.FirstName)
            .ToListAsync(cancellationToken);
        return View(profiles);
    }

    public async Task<IActionResult> Details(int employeeId, CancellationToken cancellationToken)
    {
        var profile = await dbContext.EmployeeProfiles
            .AsNoTracking()
            .Include(item => item.Employee)
            .FirstOrDefaultAsync(item => item.EmployeeId == employeeId, cancellationToken);
        if (profile is null)
            return NotFound();

        ViewBag.OtherDetails = await dbContext.EmployeeOtherDetails.AsNoTracking()
            .Where(detail => detail.EmployeeId == employeeId)
            .OrderBy(detail => detail.DocumentType)
            .ToListAsync(cancellationToken);
        ViewBag.CanManageOtherDetails = await CanEditProfileAsync(employeeId, cancellationToken);
        ViewBag.CanEditProfile = await CanEditProfileAsync(employeeId, cancellationToken);
        ViewBag.CanDeleteProfile = CanManageAllProfiles();
        ViewData["Title"] = "Profile Information";
        return View(profile);
    }

    public async Task<IActionResult> Create(int? employeeId, CancellationToken cancellationToken, bool returnToProfile = false)
    {
        var canManageAll = CanManageAllProfiles();
        if (!canManageAll)
        {
            var ownEmployeeId = await GetOwnEmployeeIdAsync(cancellationToken);
            if (!ownEmployeeId.HasValue)
                return Forbid();
            employeeId = ownEmployeeId.Value;
        }

        if (employeeId.HasValue && await dbContext.EmployeeProfiles.AnyAsync(profile => profile.EmployeeId == employeeId.Value, cancellationToken))
            return RedirectToAction(nameof(Details), new { employeeId = employeeId.Value });

        if (canManageAll)
            await LoadEmployeesAsync(employeeId, cancellationToken);
        ViewBag.CanChooseEmployee = canManageAll && !employeeId.HasValue;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewData["Title"] = "Create profile information";
        return View(new EmployeeProfile { EmployeeId = employeeId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeProfile model, CancellationToken cancellationToken, bool returnToProfile = false)
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
        if (await dbContext.EmployeeProfiles.AnyAsync(profile => profile.EmployeeId == model.EmployeeId, cancellationToken))
            ModelState.AddModelError(nameof(model.EmployeeId), "This employee already has profile information. Open it to edit.");

        if (!await dbContext.Employees.AnyAsync(employee => employee.Id == model.EmployeeId && !employee.EmpIsDeleted, cancellationToken))
            ModelState.AddModelError(nameof(model.EmployeeId), "Select a valid employee.");

        if (!ModelState.IsValid)
        {
            if (canManageAll)
                await LoadEmployeesAsync(model.EmployeeId, cancellationToken);
            ViewBag.CanChooseEmployee = canManageAll;
            ViewBag.ReturnToProfile = returnToProfile;
            ViewData["Title"] = "Create profile information";
            return View(model);
        }

        dbContext.EmployeeProfiles.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Employee profile information was created.";
        return returnToProfile ? RedirectToAction("Profile", "Account") : RedirectToAction(nameof(Details), new { employeeId = model.EmployeeId });
    }

    public async Task<IActionResult> Edit(int employeeId, CancellationToken cancellationToken, string section = "profile", bool returnToProfile = false)
    {
        if (!await CanEditProfileAsync(employeeId, cancellationToken))
            return Forbid();
        var editSection = NormalizeSection(section);
        if (editSection is null)
            return BadRequest();

        var profile = await dbContext.EmployeeProfiles
            .Include(item => item.Employee)
            .FirstOrDefaultAsync(item => item.EmployeeId == employeeId, cancellationToken);
        if (profile is null)
            return NotFound();

        ViewBag.EditSection = editSection;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewData["Title"] = editSection switch { "emergency" => "Edit emergency contact", "address" => "Edit address information", _ => "Edit profile information" };
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int employeeId, string section, EmployeeProfile model, CancellationToken cancellationToken, bool returnToProfile = false)
    {
        if (!await CanEditProfileAsync(employeeId, cancellationToken))
            return Forbid();
        if (employeeId != model.EmployeeId)
            return BadRequest();
        var editSection = NormalizeSection(section);
        if (editSection is null)
            return BadRequest();

        var profile = await dbContext.EmployeeProfiles
            .FirstOrDefaultAsync(item => item.EmployeeId == employeeId, cancellationToken);
        if (profile is null)
            return NotFound();

        RemoveEmployeeNavigationValidationErrors();
        if (!ModelState.IsValid)
        {
            model.Employee = await dbContext.Employees.FirstAsync(employee => employee.Id == employeeId, cancellationToken);
            ViewBag.EditSection = editSection;
            ViewBag.ReturnToProfile = returnToProfile;
            ViewData["Title"] = editSection switch { "emergency" => "Edit emergency contact", "address" => "Edit address information", _ => "Edit profile information" };
            return View(model);
        }

        if (editSection == "emergency")
        {
            profile.EmergencyContactName = model.EmergencyContactName;
            profile.EmergencyContactRelationship = model.EmergencyContactRelationship;
            profile.EmergencyContactPhone = model.EmergencyContactPhone;
        }
        else if (editSection == "address")
        {
            profile.PermanentAddressLine1 = model.PermanentAddressLine1;
            profile.PermanentAddressLine2 = model.PermanentAddressLine2;
            profile.PermanentCountry = model.PermanentCountry;
            profile.PermanentState = model.PermanentState;
            profile.PermanentCity = model.PermanentCity;
            profile.PermanentPincode = model.PermanentPincode;
            profile.CurrentAddressLine1 = model.CurrentAddressLine1;
            profile.CurrentAddressLine2 = model.CurrentAddressLine2;
            profile.CurrentCountry = model.CurrentCountry;
            profile.CurrentState = model.CurrentState;
            profile.CurrentCity = model.CurrentCity;
            profile.CurrentPincode = model.CurrentPincode;
        }
        else
        {
            profile.Gender = model.Gender;
            profile.MaritalStatus = model.MaritalStatus;
            profile.DOB = model.DOB;
            profile.BloodGroup = model.BloodGroup;
            profile.PhoneNumber = model.PhoneNumber;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        TempData["Success"] = editSection switch
        {
            "emergency" => "Emergency contact information was updated.",
            "address" => "Address information was updated.",
            _ => "Employee profile information was updated."
        };
        return returnToProfile
            ? RedirectToAction("Profile", "Account")
            : RedirectToAction(nameof(Details), new { employeeId });
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
    public async Task<IActionResult> Delete(int employeeId, CancellationToken cancellationToken)
    {
        var profile = await dbContext.EmployeeProfiles
            .AsNoTracking()
            .Include(item => item.Employee)
            .FirstOrDefaultAsync(item => item.EmployeeId == employeeId, cancellationToken);
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
        var profile = await dbContext.EmployeeProfiles.FirstOrDefaultAsync(item => item.EmployeeId == employeeId, cancellationToken);
        if (profile is null)
            return NotFound();

        dbContext.EmployeeProfiles.Remove(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Employee profile information was deleted.";
        return RedirectToAction(nameof(Index));
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

    private bool CanManageAllProfiles() =>
        User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager);

    private async Task<bool> CanEditProfileAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (CanManageAllProfiles())
            return true;

        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(identityUserId) && await dbContext.EmployeeProfiles
            .AnyAsync(profile => profile.EmployeeId == employeeId && profile.Employee.IdentityUserId == identityUserId, cancellationToken);
    }

    private Task<int?> GetOwnEmployeeIdAsync(CancellationToken cancellationToken)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(identityUserId)
            ? Task.FromResult<int?>(null)
            : dbContext.Employees.Where(employee => employee.IdentityUserId == identityUserId && !employee.EmpIsDeleted)
                .Select(employee => (int?)employee.Id)
                .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task LoadEmployeesAsync(int? selectedId, CancellationToken cancellationToken)
    {
        ViewBag.Employees = await dbContext.Employees
            .AsNoTracking()
            .Where(employee => !employee.EmpIsDeleted)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => new { employee.Id, Name = employee.FirstName + " " + employee.LastName, employee.Email })
            .ToListAsync(cancellationToken);
        ViewBag.SelectedEmployeeId = selectedId;
    }
}











