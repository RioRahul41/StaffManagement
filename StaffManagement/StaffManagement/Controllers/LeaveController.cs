using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Enums;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class LeaveController(
    ILeaveRequestService leaveService,
    IWebHostEnvironment environment) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> Index(
        int page = 1,
        string? search = null,
        LeaveStatus? status = null,
        LeaveType? leaveType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            ModelState.AddModelError(string.Empty, "From date must be on or before To date.");

        var result = await leaveService.GetPagedAsync(
            CurrentIdentityUserId(),
            CanReviewLeaves(),
            CanViewAllLeaves(),
            page,
            PageSize,
            search,
            status,
            leaveType,
            fromDate,
            toDate,
            cancellationToken);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.LeaveType = leaveType;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(result);
    }

    public IActionResult Create() => View(new CreateLeaveRequestDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateLeaveRequestDto model,
        IFormFile? document,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var requestedDays = (model.ToDate.Date - model.FromDate.Date).Days + 1;
            if (model.LeaveType == LeaveType.Sick && requestedDays >= 2 && (document is null || document.Length == 0))
                ModelState.AddModelError(nameof(document), "A supporting document is required for sick leave applications of 2 or more days.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var requestedLeaveDays = (model.ToDate.Date - model.FromDate.Date).Days + 1;
        var requiresDocument = model.LeaveType == LeaveType.Sick && requestedLeaveDays >= 2;
        string? documentPath = null;
        string? uploadError = null;
        if (requiresDocument)
            (documentPath, uploadError) = await SaveDocumentAsync(document, cancellationToken);
        if (uploadError is not null)
        {
            ModelState.AddModelError(nameof(document), uploadError);
            return View(model);
        }

        var result = await leaveService.CreateAsync(
            model,
            CurrentIdentityUserId(),
            documentPath,
            cancellationToken);

        if (!result.Succeeded)
        {
            DeleteDocument(documentPath);
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = "Leave application submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id, CancellationToken cancellationToken) =>
        ReviewAsync(id, LeaveStatus.Approved, cancellationToken);

    [HttpPost]
    [Authorize(Roles = RoleNames.Manager)]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id, CancellationToken cancellationToken) =>
        ReviewAsync(id, LeaveStatus.Rejected, cancellationToken);

    public async Task<IActionResult> Document(int id, CancellationToken cancellationToken)
    {
        var documentPath = await leaveService.GetDocumentPathAsync(
            id,
            CurrentIdentityUserId(),
            CanReviewLeaves(),
            cancellationToken);

        if (documentPath is null)
            return NotFound();

        var storagePath = Path.Combine(environment.ContentRootPath, "App_Data", "LeaveDocuments");
        var fullPath = Path.GetFullPath(Path.Combine(storagePath, Path.GetFileName(documentPath)));
        var rootPath = Path.GetFullPath(storagePath) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "application/octet-stream", Path.GetFileName(fullPath));
    }

    private async Task<IActionResult> ReviewAsync(
        int id,
        LeaveStatus status,
        CancellationToken cancellationToken)
    {
        var result = await leaveService.ReviewAsync(id, status, CurrentIdentityUserId(), cancellationToken);
        if (result.Succeeded)
            TempData["Success"] = status == LeaveStatus.Approved
                ? "Leave request approved."
                : "Leave request rejected.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }

    private bool CanReviewLeaves() =>
        User.IsInRole(RoleNames.Manager);

    private bool CanViewAllLeaves() =>
        User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Admin);

    private string CurrentIdentityUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    private async Task<(string? Path, string? Error)> SaveDocumentAsync(
        IFormFile? document,
        CancellationToken cancellationToken)
    {
        if (document is null || document.Length == 0)
            return (null, null);

        const long maxFileSize = 10 * 1024 * 1024;
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        var extension = Path.GetExtension(document.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return (null, "Only PDF, Word, Excel, PNG, and JPEG files are allowed.");

        if (document.Length > maxFileSize)
            return (null, "The document cannot be larger than 10 MB.");

        var uploadDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "LeaveDocuments");
        Directory.CreateDirectory(uploadDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, storedFileName);
        await using var stream = System.IO.File.Create(filePath);
        await document.CopyToAsync(stream, cancellationToken);

        return (storedFileName, null);
    }

    private void DeleteDocument(string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
            return;

        var fileName = Path.GetFileName(documentPath);
        var filePath = Path.Combine(environment.ContentRootPath, "App_Data", "LeaveDocuments", fileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }
}
