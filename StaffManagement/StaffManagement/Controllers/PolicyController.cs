using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;
using StaffManagement.Models;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class PolicyController(
    IPolicyDocumentService policyService,
    IWebHostEnvironment environment) : Controller
{
    private const long MaxPdfSize = 15 * 1024 * 1024;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var documents = await policyService.GetAllAsync(cancellationToken);
        return View(new PolicyLibraryViewModel { Documents = documents });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        string policyName,
        IFormFile? pdf,
        CancellationToken cancellationToken)
    {
        policyName = policyName?.Trim() ?? string.Empty;
        if (policyName.Length is 0 or > 120)
            ModelState.AddModelError(nameof(policyName), "Enter a policy name up to 120 characters.");

        if (pdf is null || pdf.Length == 0)
            ModelState.AddModelError(nameof(pdf), "Choose a PDF to upload.");
        else if (pdf.Length > MaxPdfSize)
            ModelState.AddModelError(nameof(pdf), "The PDF cannot be larger than 15 MB.");
        else if (!string.Equals(Path.GetExtension(pdf.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(pdf), "Only PDF files are allowed.");
        else
        {
            await using var stream = pdf.OpenReadStream();
            var signature = new byte[5];
            var bytesRead = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
            if (bytesRead != signature.Length || !signature.AsSpan().SequenceEqual("%PDF-"u8))
                ModelState.AddModelError(nameof(pdf), "The selected file is not a valid PDF.");
        }

        if (!ModelState.IsValid)
        {
            var documents = await policyService.GetAllAsync(cancellationToken);
            return View("Index", new PolicyLibraryViewModel
            {
                Documents = documents,
                PolicyName = policyName
            });
        }

        var storedFileName = $"{Guid.NewGuid():N}.pdf";
        var uploadDirectory = GetUploadDirectory();
        Directory.CreateDirectory(uploadDirectory);
        var fullPath = Path.Combine(uploadDirectory, storedFileName);
        var originalFileName = Path.GetFileName(pdf!.FileName) ?? "policy.pdf";
        var uploadedBy = User.Identity?.Name ?? "Staff";
        if (uploadedBy.Length > 256)
            uploadedBy = uploadedBy[..256];

        try
        {
            await using (var destination = System.IO.File.Create(fullPath))
                await pdf.CopyToAsync(destination, cancellationToken);

            await policyService.AddAsync(new PolicyDocument
            {
                PolicyName = policyName,
                OriginalFileName = originalFileName.Length > 255 ? originalFileName[^255..] : originalFileName,
                StoredFileName = storedFileName,
                UploadedBy = uploadedBy,
                UploadedAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }
        catch
        {
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
            throw;
        }

        TempData["Success"] = "Policy uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ViewPdf(int id, CancellationToken cancellationToken)
    {
        var document = await policyService.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return NotFound();

        var fullPath = Path.Combine(GetUploadDirectory(), Path.GetFileName(document.StoredFileName));
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        Response.Headers["Content-Disposition"] = "inline";
        return PhysicalFile(fullPath, "application/pdf", enableRangeProcessing: true);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var document = await policyService.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return NotFound();

        var fullPath = Path.Combine(GetUploadDirectory(), Path.GetFileName(document.StoredFileName));
        if (await policyService.DeleteAsync(id, cancellationToken) && System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        TempData["Success"] = "Policy removed.";
        return RedirectToAction(nameof(Index));
    }

    private string GetUploadDirectory() =>
        Path.Combine(environment.ContentRootPath, "App_Data", "Policies");
}
