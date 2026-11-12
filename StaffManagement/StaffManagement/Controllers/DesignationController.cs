using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Web.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
public class DesignationController(IDesignationService designationService) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await designationService.GetAllAsync());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var designation = await designationService.GetByIdAsync(id);
        return designation is null ? NotFound() : View(designation);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Designation());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Designation designation)
    {
        if (!ModelState.IsValid)
        {
            return View(designation);
        }

        var result = await designationService.CreateAsync(designation);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                nameof(Designation.DesignationName),
                result.Error!);

            return View(designation);
        }

        TempData["SuccessMessage"] = "Designation created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var designation = await designationService.GetByIdAsync(id);
        return designation is null ? NotFound() : View(designation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Designation designation)
    {
        if (id != designation.DesignationId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(designation);
        }

        var existingDesignation = await designationService.GetByIdAsync(id);
        if (existingDesignation is null)
        {
            return NotFound();
        }

        existingDesignation.DesignationName = designation.DesignationName;
        existingDesignation.Description = designation.Description;
        existingDesignation.IsActive = designation.IsActive;

        var result = await designationService.UpdateAsync(existingDesignation);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                nameof(Designation.DesignationName),
                result.Error!);

            return View(designation);
        }

        TempData["SuccessMessage"] = "Designation updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var designation = await designationService.GetByIdAsync(id);
        return designation is null ? NotFound() : View(designation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await designationService.DeleteAsync(id))
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Designation deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
