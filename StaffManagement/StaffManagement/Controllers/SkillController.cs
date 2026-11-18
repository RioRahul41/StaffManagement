using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.SkillModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;

namespace StaffManagement.Web.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.Manager)]
public sealed class SkillController(ISkillService skillService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await skillService.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new SkillCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SkillCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await skillService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.SkillName), result.Error!);
            return View(model);
        }

        TempData["Success"] = "Skill created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var skill = await skillService.GetByIdAsync(id, cancellationToken);
        return skill is null ? NotFound() : View(skill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SkillCreateViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var result = await skillService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.SkillName), result.Error!);
            return View(model);
        }

        TempData["Success"] = "Skill updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        if (!await skillService.DeactivateAsync(id, cancellationToken))
            return NotFound();

        TempData["Success"] = "Skill deactivated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
