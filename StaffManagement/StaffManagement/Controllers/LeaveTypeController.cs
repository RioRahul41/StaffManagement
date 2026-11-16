using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
public sealed class LeaveTypeController(ILeaveTypeService leaveTypeService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await leaveTypeService.GetAllAsync(includeInactive: true, cancellationToken));

    public IActionResult Create() => View(new LeaveTypeMaster());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveTypeMaster model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);
        var result = await leaveTypeService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(model);
        }
        TempData["SuccessMessage"] = "Leave type created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var leaveType = await leaveTypeService.GetByIdAsync(id, cancellationToken);
        return leaveType is null ? NotFound() : View(leaveType);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeaveTypeMaster model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return BadRequest();
        if (!ModelState.IsValid)
            return View(model);
        var result = await leaveTypeService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), result.Error!);
            return View(model);
        }
        TempData["SuccessMessage"] = "Leave type updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await leaveTypeService.DeleteAsync(id, cancellationToken))
            TempData["ErrorMessage"] = "Leave type was not found or is already inactive.";
        else
            TempData["SuccessMessage"] = "Leave type deactivated. Existing leave requests remain unchanged.";
        return RedirectToAction(nameof(Index));
    }
}
