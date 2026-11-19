using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.DepartmentModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;

namespace StaffManagement.Web.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
public class DepartmentController(IDepartmentService departmentService) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await departmentService.GetAllAsync());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var department = await departmentService.GetByIdAsync(id);
        return department is null ? NotFound() : View(department);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DepartmentDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentDto department)
    {
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        var result = await departmentService.CreateAsync(department);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(DepartmentDto.DepartmentName), result.Error!);
            return View(department);
        }

        TempData["SuccessMessage"] = "Department created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var department = await departmentService.GetByIdAsync(id);
        return department is null ? NotFound() : View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentDto department)
    {
        if (id != department.DepartmentId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        var existingDepartment = await departmentService.GetByIdAsync(id);
        if (existingDepartment is null)
        {
            return NotFound();
        }

        existingDepartment.DepartmentName = department.DepartmentName;
        existingDepartment.Description = department.Description;
        existingDepartment.IsActive = department.IsActive;

        var result = await departmentService.UpdateAsync(existingDepartment);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(DepartmentDto.DepartmentName), result.Error!);
            return View(department);
        }

        TempData["SuccessMessage"] = "Department updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await departmentService.GetByIdAsync(id);
        return department is null ? NotFound() : View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await departmentService.DeleteAsync(id))
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Department deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
