using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;
using System.Security.Claims;

namespace StaffManagement.Controllers;

[Authorize]
public class ProjectController(
    IProjectService projectService,
    IEmployeeService employeeService) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Manager) || User.IsInRole(RoleNames.Hr))
            return View(await projectService.GetAllAsync());

        var employee = await employeeService.GetByIdentityUserIdAsync(CurrentIdentityUserId());
        if (employee is null || employee.ProjectIds.Count == 0)
            return View(Array.Empty<Project>());

        var allActiveProjects = await projectService.GetAllAsync();
        return View(allActiveProjects.Where(project => employee.ProjectIds.Contains(project.ProjectId)).ToList());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var project = await projectService.GetByIdAsync(id);
        if (project is not null && !await CanViewProjectAsync(project.ProjectId))
            return Forbid();
        return project is null ? NotFound() : View(project);
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    [HttpGet]
    public IActionResult Create() => View(new Project());

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
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

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await projectService.GetByIdAsync(id);
        return project is null ? NotFound() : View(project);
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
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

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await projectService.DeleteAsync(id)) return NotFound();
        TempData["SuccessMessage"] = "Project deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanViewProjectAsync(int projectId)
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Manager) || User.IsInRole(RoleNames.Hr))
            return true;

        var employee = await employeeService.GetByIdentityUserIdAsync(CurrentIdentityUserId());
        return employee?.ProjectIds.Contains(projectId) == true;
    }

    private string CurrentIdentityUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}
