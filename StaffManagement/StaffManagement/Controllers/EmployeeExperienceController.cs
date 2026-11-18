using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;
namespace StaffManagement.Controllers;
[Authorize]
public sealed class EmployeeExperienceController(IEmployeeExperienceService experienceService, IEmployeeProfileService profileService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int employeeId, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(employeeId, cancellationToken)) return Forbid();
        var employee = await experienceService.GetEmployeeByIdAsync(employeeId, cancellationToken);
        if (employee is null) return NotFound();
        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        return View(new EmployeeExperience { EmployeeId = employeeId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeExperience model, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(model.EmployeeId, cancellationToken)) return Forbid();
        var employee = await experienceService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        ModelState.Remove(nameof(EmployeeExperience.Employee));
        if (!ModelState.IsValid) return View(model);
        if (!await experienceService.CreateAsync(model, cancellationToken)) return NotFound();
        TempData["Success"] = "Experience information was added.";
        return ReturnToEmployee(model.EmployeeId, returnToEditProfile, false, returnToEmployeeDetails);
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        var item = await experienceService.GetByIdAsync(id, cancellationToken);
        if (item is null || !await CanEditAsync(item.EmployeeId, cancellationToken)) return NotFound();
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        return View(item);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        var item = await experienceService.GetByIdAsync(id, cancellationToken);
        if (item is null || !await CanEditAsync(item.EmployeeId, cancellationToken)) return NotFound();
        ViewBag.EmployeeName = $"{item.Employee.FirstName} {item.Employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        return View(item);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeExperience model, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(model.EmployeeId, cancellationToken)) return Forbid();
        var employee = await experienceService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        ModelState.Remove(nameof(EmployeeExperience.Employee));
        if (!ModelState.IsValid) return View(model);
        if (!await experienceService.UpdateAsync(model, cancellationToken)) return NotFound();
        TempData["Success"] = "Experience information was updated.";
        return ReturnToEmployee(model.EmployeeId, returnToEditProfile, returnToProfile, returnToEmployeeDetails);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        var item = await experienceService.GetByIdAsync(id, cancellationToken);
        if (item is null || !await CanEditAsync(item.EmployeeId, cancellationToken)) return NotFound();
        await experienceService.DeleteAsync(id, cancellationToken);
        TempData["Success"] = "Experience information was deleted.";
        return ReturnToEmployee(item.EmployeeId, returnToEditProfile, returnToProfile, returnToEmployeeDetails);
    }
    private IActionResult ReturnToEmployee(int employeeId, bool editProfile, bool profile, bool employeeDetails)
    {
        if (employeeDetails) return RedirectToAction("Details", "Employee", new { id = employeeId });
        if (editProfile) return RedirectToAction("EditProfile", "Account", new { section = "experience" });
        return RedirectToAction("Profile", "Account");
    }
    private async Task<bool> CanEditAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager)) return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userId) && await profileService.CanEditAsync(employeeId, userId, cancellationToken);
    }
}




