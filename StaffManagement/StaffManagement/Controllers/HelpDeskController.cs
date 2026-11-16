using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.HelpDeskModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Enums;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class HelpDeskController(
    IHelpDeskTicketService ticketService,
    IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var tickets = await ticketService.GetPagedAsync(
            CurrentIdentityUserId(), CanViewAllTickets(), page, PageSize, cancellationToken);
        return View(tickets);
    }

    public IActionResult Create() => View(new CreateHelpDeskTicketDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateHelpDeskTicketDto model,
        IFormFile? document,
        CancellationToken cancellationToken)
    {
        string? documentPath = null;
        if (ModelState.IsValid && document is { Length: > 0 })
        {
            var upload = await SaveDocumentAsync(document, cancellationToken);
            documentPath = upload.Path;
            if (upload.Error is not null)
                ModelState.AddModelError(nameof(document), upload.Error);
        }

        if (!ModelState.IsValid)
            return View(model);

        var result = await ticketService.CreateAsync(model, CurrentIdentityUserId(), documentPath, cancellationToken);
        if (!result.Succeeded)
        {
            DeleteDocument(documentPath);
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = "Help Desk ticket submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Hr + "," + RoleNames.TechnicalSupport)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, HelpDeskStatus status, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Select a valid status.";
            return RedirectToAction(nameof(Index));
        }

        var result = await ticketService.UpdateStatusAsync(id, status, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? "Help Desk ticket status updated."
            : result.Error;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Document(int id, CancellationToken cancellationToken)
    {
        var documentPath = await ticketService.GetDocumentPathAsync(
            id, CurrentIdentityUserId(), CanViewAllTickets(), cancellationToken);
        if (documentPath is null)
            return NotFound();

        var storagePath = Path.Combine(environment.ContentRootPath, "App_Data", "HelpDeskDocuments");
        var fullPath = Path.GetFullPath(Path.Combine(storagePath, Path.GetFileName(documentPath)));
        var rootPath = Path.GetFullPath(storagePath) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "application/octet-stream", Path.GetFileName(fullPath));
    }

    private async Task<(string? Path, string? Error)> SaveDocumentAsync(
        IFormFile document,
        CancellationToken cancellationToken)
    {
        const long maxFileSize = 10 * 1024 * 1024;
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return (null, "Only PDF, Word, Excel, PNG, and JPEG files are allowed.");
        if (document.Length > maxFileSize)
            return (null, "The document cannot be larger than 10 MB.");

        var uploadDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "HelpDeskDocuments");
        Directory.CreateDirectory(uploadDirectory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(uploadDirectory, fileName));
        await document.CopyToAsync(stream, cancellationToken);
        return (fileName, null);
    }

    private void DeleteDocument(string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
            return;
        var path = Path.Combine(environment.ContentRootPath, "App_Data", "HelpDeskDocuments", Path.GetFileName(documentPath));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    private bool CanViewAllTickets() => User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.TechnicalSupport);

    private string CurrentIdentityUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}
