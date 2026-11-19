using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
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
        Prepare(employee, returnToEditProfile, false, returnToEmployeeDetails);
        return View(new ExperienceInformationDto { EmployeeId = employeeId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExperienceInformationDto model, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(model.EmployeeId, cancellationToken)) return Forbid();
        var employee = await experienceService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        Prepare(employee, returnToEditProfile, false, returnToEmployeeDetails);
        if (!ModelState.IsValid) return View(model);
        var result = await experienceService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Experience information could not be saved.");
            return View(model);
        }
        TempData["Success"] = "Experience information was added.";
        return ReturnToEmployee(model.EmployeeId, returnToEditProfile, false, returnToEmployeeDetails);
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        var item = await experienceService.GetByIdAsync(id, cancellationToken);
        if (item is null || !await CanEditAsync(item.EmployeeId, cancellationToken)) return NotFound();
        var employee = await experienceService.GetEmployeeByIdAsync(item.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        return View(item);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        var item = await experienceService.GetByIdAsync(id, cancellationToken);
        if (item is null || !await CanEditAsync(item.EmployeeId, cancellationToken)) return NotFound();
        var employee = await experienceService.GetEmployeeByIdAsync(item.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        Prepare(employee, returnToEditProfile, returnToProfile, returnToEmployeeDetails);
        return View(item);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ExperienceInformationDto model, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(model.EmployeeId, cancellationToken)) return Forbid();
        var employee = await experienceService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        Prepare(employee, returnToEditProfile, returnToProfile, returnToEmployeeDetails);
        if (!ModelState.IsValid) return View(model);
        var result = await experienceService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Experience information could not be updated.");
            return View(model);
        }
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
    private void Prepare(EmployeeDto employee, bool returnToEditProfile, bool returnToProfile, bool returnToEmployeeDetails)
    {
        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
    }
    private async Task<bool> CanEditAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager)) return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userId) && await profileService.CanEditAsync(employeeId, userId, cancellationToken);
    }
}




