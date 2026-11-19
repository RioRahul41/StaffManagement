using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class EmployeeOtherDetailController(ApplicationDbContext dbContext, IWebHostEnvironment environment, IEmployeeOtherDetailService detailService, IDocumentTypeService documentTypeService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int employeeId, bool returnToProfile = false, bool returnToEditProfile = false, CancellationToken cancellationToken = default)
    {
        if (!await CanManageEmployeeAsync(employeeId, cancellationToken))
            return Forbid();
        await LoadDocumentTypesAsync(cancellationToken);
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewData["Title"] = "Add document detail";
        return View(new EmployeeOtherDetailDto { EmployeeId = employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeOtherDetailDto model, IFormFile? documentFile, bool returnToProfile = false, bool returnToEditProfile = false, CancellationToken cancellationToken = default)
    {
        if (!await CanManageEmployeeAsync(model.EmployeeId, cancellationToken))
            return Forbid();

        if (!await dbContext.Employees.AnyAsync(employee => employee.Id == model.EmployeeId && !employee.EmpIsDeleted, cancellationToken))
            return NotFound();

        RemoveEmployeeValidationErrors();
        var documentType = await documentTypeService.GetByIdAsync(model.DocumentTypeId, cancellationToken);
        if (documentType is null || !documentType.IsActive)
            ModelState.AddModelError(nameof(model.DocumentTypeId), "Choose an active document type.");
        else if (documentType.RequiresValidityDates)
        {
            if (!model.ValidFrom.HasValue)
                ModelState.AddModelError(nameof(model.ValidFrom), "Valid From is required for this document type.");
            if (!model.ValidTo.HasValue)
                ModelState.AddModelError(nameof(model.ValidTo), "Valid To is required for this document type.");
            if (model.ValidFrom.HasValue && model.ValidTo.HasValue && model.ValidFrom.Value.Date > model.ValidTo.Value.Date)
                ModelState.AddModelError(nameof(model.ValidTo), "Valid To must be on or after Valid From.");
        }
        else
        {
            model.ValidFrom = null;
            model.ValidTo = null;
            ModelState.Remove(nameof(model.ValidFrom));
            ModelState.Remove(nameof(model.ValidTo));
        }
        if (documentFile is null || documentFile.Length == 0)
            ModelState.AddModelError(nameof(documentFile), "Choose a document file to upload.");

        var fileInfo = documentFile is { Length: > 0 } ? ValidateFile(documentFile) : (ContentType: (string?)null, Extension: (string?)null, Error: (string?)null);
        if (fileInfo.Error is not null)
            ModelState.AddModelError(nameof(documentFile), fileInfo.Error);

        if (!ModelState.IsValid)
        {
            await LoadDocumentTypesAsync(cancellationToken);
            ViewBag.ReturnToProfile = returnToProfile;
            ViewBag.ReturnToEditProfile = returnToEditProfile;
            ViewData["Title"] = "Add document detail";
            return View(model);
        }

        var file = documentFile!;
        var directory = Path.Combine(environment.ContentRootPath, "App_Data", "EmployeeDocuments");
        Directory.CreateDirectory(directory);
        var storedFileName = $"{Guid.NewGuid():N}{fileInfo.Extension}";
        var fullPath = Path.Combine(directory, storedFileName);
        await using (var output = System.IO.File.Create(fullPath))
            await file.CopyToAsync(output, cancellationToken);

        model.OriginalFileName = Path.GetFileName(file.FileName);
        model.StoredFileName = storedFileName;
        model.ContentType = fileInfo.ContentType!;
        try
        {
            var result = await detailService.CreateAsync(model, cancellationToken);
            if (!result.Succeeded)
            {
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                ModelState.AddModelError(string.Empty, result.Error ?? "Document details could not be saved.");
                await LoadDocumentTypesAsync(cancellationToken);
                ViewBag.ReturnToProfile = returnToProfile;
                ViewBag.ReturnToEditProfile = returnToEditProfile;
                ViewData["Title"] = "Add document detail";
                return View(model);
            }
        }
        catch
        {
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
            throw;
        }

        TempData["Success"] = "Document detail was added.";
        if (returnToEditProfile) return RedirectToAction("EditProfile", "Account", new { section = "other" });
        return returnToProfile
            ? RedirectToAction("Profile", "Account")
            : RedirectToAction("Details", "EmployeeProfile", new { employeeId = model.EmployeeId });
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var detail = await dbContext.EmployeeOtherDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.EmployeeOtherDetailId == id, cancellationToken);
        if (detail is null || !await CanManageEmployeeAsync(detail.EmployeeId, cancellationToken))
            return NotFound();

        var safeName = Path.GetFileName(detail.StoredFileName);
        var directory = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "EmployeeDocuments"));
        var fullPath = Path.GetFullPath(Path.Combine(directory, safeName));
        if (!fullPath.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(fullPath, detail.ContentType, detail.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, bool returnToProfile = false, bool returnToEditProfile = false, bool returnToEmployeeDetails = false, CancellationToken cancellationToken = default)
    {
        var detail = await detailService.GetByIdAsync(id, cancellationToken);
        if (detail is null || !await CanManageEmployeeAsync(detail.EmployeeId, cancellationToken))
            return NotFound();

        if (!await detailService.DeleteAsync(id, cancellationToken))
            return NotFound();

        DeleteStoredFile(detail.StoredFileName);
        TempData["Success"] = "Document detail was deleted.";
        if (returnToEditProfile)
            return RedirectToAction("EditProfile", "Account", new { section = "other" });
        if (returnToProfile)
            return RedirectToAction("Profile", "Account");
        if (returnToEmployeeDetails)
            return RedirectToAction("Details", "Employee", new { id = detail.EmployeeId });
        return RedirectToAction("Details", "EmployeeProfile", new { employeeId = detail.EmployeeId });
    }

    private (string? ContentType, string? Extension, string? Error) ValidateFile(IFormFile file)
    {
        const long maxSize = 10 * 1024 * 1024;
        if (file.Length > maxSize)
            return (null, null, "Documents must be 10 MB or smaller.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => null
        };
        return contentType is null
            ? (null, null, "Choose a PDF, JPG, JPEG, or PNG document.")
            : (contentType, extension, null);
    }

    private async Task<bool> CanManageEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager))
            return true;

        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(identityUserId) && await dbContext.Employees
            .AnyAsync(employee => employee.Id == employeeId && employee.IdentityUserId == identityUserId && !employee.EmpIsDeleted, cancellationToken);
    }

    private void RemoveEmployeeValidationErrors()
    {
        ModelState.Remove(nameof(EmployeeOtherDetailDto.OriginalFileName));
        ModelState.Remove(nameof(EmployeeOtherDetailDto.StoredFileName));
        ModelState.Remove(nameof(EmployeeOtherDetailDto.ContentType));
    }

    private async Task LoadDocumentTypesAsync(CancellationToken cancellationToken) =>
        ViewBag.DocumentTypes = await documentTypeService.GetActiveAsync(cancellationToken);

    private void DeleteStoredFile(string storedFileName)
    {
        var safeName = Path.GetFileName(storedFileName);
        var directory = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "EmployeeDocuments"));
        var fullPath = Path.GetFullPath(Path.Combine(directory, safeName));
        if (fullPath.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }
}


