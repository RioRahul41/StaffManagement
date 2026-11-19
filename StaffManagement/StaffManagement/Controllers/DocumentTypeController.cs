using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.DocumentTypeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;

namespace StaffManagement.Web.Controllers;

[Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
public sealed class DocumentTypeController(IDocumentTypeService documentTypeService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await documentTypeService.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new DocumentTypeDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentTypeDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await documentTypeService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.DocumentTypeName), result.Error ?? "Document type could not be created.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Document type created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await documentTypeService.GetByIdAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentTypeDto model, CancellationToken cancellationToken)
    {
        if (id != model.DocumentTypeId)
            return BadRequest();
        if (!ModelState.IsValid)
            return View(model);

        var result = await documentTypeService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.DocumentTypeName), result.Error ?? "Document type could not be updated.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Document type updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        if (!await documentTypeService.DeactivateAsync(id, cancellationToken))
            return NotFound();

        TempData["SuccessMessage"] = "Document type deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
