using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Web.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
public class DesignationController(IDesignationService designationService, IDepartmentService departmentService) : Controller
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
    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsAsync();
        return View(new Designation());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Designation designation)
    {
        if (!designation.DepartmentId.HasValue)
            ModelState.AddModelError(nameof(designation.DepartmentId), "Select a department.");

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(designation.DepartmentId);
            return View(designation);
        }

        var result = await designationService.CreateAsync(designation);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(Designation.DesignationName), result.Error!);
            await LoadDepartmentsAsync(designation.DepartmentId);
            return View(designation);
        }

        TempData["SuccessMessage"] = "Designation created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var designation = await designationService.GetByIdAsync(id);
        if (designation is null) return NotFound();
        await LoadDepartmentsAsync(designation.DepartmentId);
        return View(designation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Designation designation)
    {
        if (id != designation.DesignationId)
        {
            return BadRequest();
        }

        if (!designation.DepartmentId.HasValue)
            ModelState.AddModelError(nameof(designation.DepartmentId), "Select a department.");

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(designation.DepartmentId);
            return View(designation);
        }

        var existingDesignation = await designationService.GetByIdAsync(id);
        if (existingDesignation is null)
        {
            return NotFound();
        }

        existingDesignation.DesignationName = designation.DesignationName;
        existingDesignation.Description = designation.Description;
        existingDesignation.DepartmentId = designation.DepartmentId;
        existingDesignation.IsActive = designation.IsActive;

        var result = await designationService.UpdateAsync(existingDesignation);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(Designation.DesignationName), result.Error!);
            await LoadDepartmentsAsync(designation.DepartmentId);
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
    private async Task LoadDepartmentsAsync(int? selectedId = null)
    {
        ViewBag.Departments = await departmentService.GetAllForSelectionAsync();
        ViewBag.SelectedDepartmentId = selectedId;
    }
}
