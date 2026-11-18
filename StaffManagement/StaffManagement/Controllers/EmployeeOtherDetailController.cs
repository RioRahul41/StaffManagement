using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class EmployeeOtherDetailController(ApplicationDbContext dbContext, IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int employeeId, bool returnToProfile = false, bool returnToEditProfile = false, CancellationToken cancellationToken = default)
    {
        if (!await CanManageEmployeeAsync(employeeId, cancellationToken))
            return Forbid();
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewData["Title"] = "Add document detail";
        return View(new EmployeeOtherDetail { EmployeeId = employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeOtherDetail model, IFormFile? documentFile, bool returnToProfile = false, bool returnToEditProfile = false, CancellationToken cancellationToken = default)
    {
        if (!await CanManageEmployeeAsync(model.EmployeeId, cancellationToken))
            return Forbid();

        if (!await dbContext.Employees.AnyAsync(employee => employee.Id == model.EmployeeId && !employee.EmpIsDeleted, cancellationToken))
            return NotFound();

        RemoveEmployeeValidationErrors();
        if (model.ValidFrom.HasValue && model.ValidTo.HasValue && model.ValidFrom.Value.Date > model.ValidTo.Value.Date)
            ModelState.AddModelError(nameof(model.ValidTo), "Valid To must be on or after Valid From.");
        if (documentFile is null || documentFile.Length == 0)
            ModelState.AddModelError(nameof(documentFile), "Choose a document file to upload.");

        var fileInfo = documentFile is { Length: > 0 } ? ValidateFile(documentFile) : (ContentType: (string?)null, Extension: (string?)null, Error: (string?)null);
        if (fileInfo.Error is not null)
            ModelState.AddModelError(nameof(documentFile), fileInfo.Error);

        if (!ModelState.IsValid)
        {
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
        dbContext.EmployeeOtherDetails.Add(model);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
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
        ModelState.Remove(nameof(EmployeeOtherDetail.OriginalFileName));
        ModelState.Remove(nameof(EmployeeOtherDetail.StoredFileName));
        ModelState.Remove(nameof(EmployeeOtherDetail.ContentType));
        foreach (var key in ModelState.Keys.Where(key => key == nameof(EmployeeOtherDetail.Employee) || key.StartsWith(nameof(EmployeeOtherDetail.Employee) + ".", StringComparison.Ordinal)).ToList())
            ModelState.Remove(key);
    }
}


