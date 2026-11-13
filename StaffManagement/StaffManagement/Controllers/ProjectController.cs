using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Web.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
public class ProjectController(IProjectService projectService) : Controller
{
    public async Task<IActionResult> Index() => View(await projectService.GetAllAsync());

    public async Task<IActionResult> Detail(int id)
    {
        var project = await projectService.GetByIdAsync(id);
        return project is null ? NotFound() : View(project);
    }

    [HttpGet]
    public IActionResult Create() => View(new Project());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (!ModelState.IsValid) return View(project);

        var result = await projectService.CreateAsync(project);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(project);
        }

        TempData["SuccessMessage"] = "Project created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await projectService.GetByIdAsync(id);
        return project is null ? NotFound() : View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.ProjectId) return BadRequest();
        if (!ModelState.IsValid) return View(project);

        var existingProject = await projectService.GetByIdAsync(id);
        if (existingProject is null) return NotFound();

        existingProject.ProjectName = project.ProjectName;
        existingProject.Description = project.Description;
        existingProject.StartDate = project.StartDate;
        existingProject.EndDate = project.EndDate;
        existingProject.IsActive = project.IsActive;

        var result = await projectService.UpdateAsync(existingProject);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(project);
        }

        TempData["SuccessMessage"] = "Project updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await projectService.DeleteAsync(id)) return NotFound();
        TempData["SuccessMessage"] = "Project deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
