using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
namespace StaffManagement.Controllers;
[Authorize]
public sealed class EmployeeBankDetailController(IEmployeeBankDetailService bankService, IEmployeeProfileService profileService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int employeeId, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(employeeId, cancellationToken)) return Forbid();
        var employee = await bankService.GetEmployeeByIdAsync(employeeId, cancellationToken);
        if (employee is null) return NotFound();
        var existing = await bankService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        if (existing is not null) return RedirectToAction(nameof(Edit), new { employeeId, returnToEditProfile, returnToEmployeeDetails });
        Prepare(employee, returnToEditProfile, returnToEmployeeDetails);
        return View(new BankDetailsDto { EmployeeId = employeeId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BankDetailsDto model, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(model.EmployeeId, cancellationToken)) return Forbid();
        var employee = await bankService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        model.IFSCCode = model.IFSCCode.Trim().ToUpperInvariant();
        if (!ModelState.IsValid)
        {
            Prepare(employee, returnToEditProfile, returnToEmployeeDetails);
            return View(model);
        }
        var result = await bankService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Bank details could not be saved.");
            Prepare(employee, returnToEditProfile, returnToEmployeeDetails);
            return View(model);
        }
        TempData["Success"] = "Bank details were saved.";
        return ReturnToEmployee(model.EmployeeId, returnToEditProfile, returnToEmployeeDetails);
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int employeeId, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(employeeId, cancellationToken)) return Forbid();
        var existing = await bankService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        if (existing is null) return RedirectToAction(nameof(Create), new { employeeId, returnToEditProfile, returnToEmployeeDetails });
        var employee = await bankService.GetEmployeeByIdAsync(employeeId, cancellationToken);
        if (employee is null) return NotFound();
        Prepare(employee, returnToEditProfile, returnToEmployeeDetails);
        return View(existing);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BankDetailsDto model, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(model.EmployeeId, cancellationToken)) return Forbid();
        var employee = await bankService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null) return NotFound();
        model.IFSCCode = model.IFSCCode.Trim().ToUpperInvariant();
        if (!ModelState.IsValid)
        {
            Prepare(employee, returnToEditProfile, returnToEmployeeDetails);
            return View(model);
        }
        var result = await bankService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Bank details could not be updated.");
            Prepare(employee, returnToEditProfile, returnToEmployeeDetails);
            return View(model);
        }
        TempData["Success"] = "Bank details were updated.";
        return ReturnToEmployee(model.EmployeeId, returnToEditProfile, returnToEmployeeDetails);
    }
    [HttpGet]
    public async Task<IActionResult> Details(int employeeId, bool returnToEditProfile = false, bool returnToProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        if (!await CanEditAsync(employeeId, cancellationToken)) return Forbid();
        var detail = await bankService.GetByEmployeeIdAsync(employeeId, cancellationToken);
        if (detail is null) return NotFound();
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
        return View(detail);
    }
    private void Prepare(EmployeeDto employee, bool returnToEditProfile, bool returnToEmployeeDetails)
    {
        ViewBag.EmployeeName = $"{employee.FirstName} {employee.LastName}";
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewBag.ReturnToEmployeeDetails = returnToEmployeeDetails;
    }
    private IActionResult ReturnToEmployee(int employeeId, bool editProfile, bool employeeDetails)
    {
        if (employeeDetails) return RedirectToAction("Details", "Employee", new { id = employeeId });
        if (editProfile) return RedirectToAction("EditProfile", "Account", new { section = "bank" });
        return RedirectToAction("Profile", "Account");
    }
    private async Task<bool> CanEditAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager)) return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userId) && await profileService.CanEditAsync(employeeId, userId, cancellationToken);
    }
}




