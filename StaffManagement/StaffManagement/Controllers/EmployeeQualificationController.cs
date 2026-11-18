using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class EmployeeQualificationController(
    IEmployeeQualificationService qualificationService,
    IEmployeeProfileService profileService,
    IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int employeeId, CancellationToken cancellationToken)
    {
        if (!await CanAccessEmployeeAsync(employeeId, cancellationToken))
            return Forbid();
        var employee = await qualificationService.GetEmployeeByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            return NotFound();

        ViewBag.Employee = employee;
        ViewBag.CanAddQualification = true;
        ViewData["Title"] = "Qualification Information";
        return View(await qualificationService.GetByEmployeeIdAsync(employeeId, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Create(int employeeId, bool returnToProfile = false, bool returnToEditProfile = false, CancellationToken cancellationToken = default)
    {
        if (!await CanAccessEmployeeAsync(employeeId, cancellationToken))
            return Forbid();
        var employee = await qualificationService.GetEmployeeByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            return NotFound();

        ViewBag.EmployeeName = employee.FirstName + " " + employee.LastName;
        ViewBag.ReturnToProfile = returnToProfile;
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        ViewData["Title"] = "Add qualification";
        return View(new EmployeeQualification { EmployeeId = employeeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeQualification model, IFormFile? qualificationFile, bool returnToProfile = false, bool returnToEditProfile = false, CancellationToken cancellationToken = default)
    {
        if (!await CanAccessEmployeeAsync(model.EmployeeId, cancellationToken))
            return Forbid();
        var employee = await qualificationService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null)
            return NotFound();

        RemoveServerManagedModelState();
        var fileInfo = ValidateUpload(qualificationFile);
        if (fileInfo.Error is not null)
            ModelState.AddModelError(nameof(qualificationFile), fileInfo.Error);
        PrepareForm(employee, returnToProfile);
        ViewBag.ReturnToEditProfile = returnToEditProfile;
        if (!ModelState.IsValid)
            return View(model);

        var file = qualificationFile!;
        var directory = GetStorageDirectory();
        Directory.CreateDirectory(directory);
        var storedFileName = $"{Guid.NewGuid():N}{fileInfo.Extension}";
        var fullPath = Path.Combine(directory, storedFileName);
        await using (var output = System.IO.File.Create(fullPath))
            await file.CopyToAsync(output, cancellationToken);

        model.OriginalFileName = Path.GetFileName(file.FileName);
        model.StoredFileName = storedFileName;
        model.ContentType = fileInfo.ContentType!;
        var result = await qualificationService.CreateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            DeleteStoredFile(fullPath);
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        TempData["Success"] = "Qualification was added.";
        if (returnToEditProfile)
            return RedirectToAction("EditProfile", "Account", new { section = "qualification" });
        return returnToProfile
            ? RedirectToAction("Profile", "Account")
            : RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, bool returnToProfile = false, CancellationToken cancellationToken = default)
    {
        var qualification = await qualificationService.GetByIdAsync(id, cancellationToken);
        if (qualification is null || !await CanAccessEmployeeAsync(qualification.EmployeeId, cancellationToken))
            return NotFound();
        var employee = await qualificationService.GetEmployeeByIdAsync(qualification.EmployeeId, cancellationToken);
        if (employee is null)
            return NotFound();

        PrepareForm(employee, returnToProfile);
        ViewData["Title"] = "Edit qualification";
        return View(qualification);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeQualification model, IFormFile? qualificationFile, bool returnToProfile = false, CancellationToken cancellationToken = default)
    {
        var existing = await qualificationService.GetByIdAsync(model.EmployeeQualificationId, cancellationToken);
        if (existing is null || existing.EmployeeId != model.EmployeeId || !await CanAccessEmployeeAsync(model.EmployeeId, cancellationToken))
            return NotFound();
        var employee = await qualificationService.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null)
            return NotFound();

        RemoveServerManagedModelState();
        (string? ContentType, string? Extension, string? Error) fileInfo = (existing.ContentType, null, null);
        if (qualificationFile is { Length: > 0 })
            fileInfo = ValidateUpload(qualificationFile);
        if (fileInfo.Error is not null)
            ModelState.AddModelError(nameof(qualificationFile), fileInfo.Error);
        PrepareForm(employee, returnToProfile);
        ViewData["Title"] = "Edit qualification";
        if (!ModelState.IsValid)
            return View(model);

        string? newFilePath = null;
        if (qualificationFile is { Length: > 0 })
        {
            var directory = GetStorageDirectory();
            Directory.CreateDirectory(directory);
            model.OriginalFileName = Path.GetFileName(qualificationFile.FileName);
            model.StoredFileName = $"{Guid.NewGuid():N}{fileInfo.Extension}";
            model.ContentType = fileInfo.ContentType!;
            newFilePath = Path.Combine(directory, model.StoredFileName);
            await using var output = System.IO.File.Create(newFilePath);
            await qualificationFile.CopyToAsync(output, cancellationToken);
        }
        else
        {
            model.OriginalFileName = existing.OriginalFileName;
            model.StoredFileName = existing.StoredFileName;
            model.ContentType = existing.ContentType;
        }

        if (!await qualificationService.UpdateAsync(model, cancellationToken))
        {
            if (newFilePath is not null)
                DeleteStoredFile(newFilePath);
            ModelState.AddModelError(string.Empty, "Qualification was not found.");
            return View(model);
        }

        if (newFilePath is not null)
            DeleteStoredFile(Path.Combine(GetStorageDirectory(), Path.GetFileName(existing.StoredFileName)));
        TempData["Success"] = "Qualification was updated.";
        return returnToProfile
            ? RedirectToAction("EditProfile", "Account", new { section = "qualification" })
            : RedirectToAction(nameof(Index), new { employeeId = model.EmployeeId });
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var qualification = await qualificationService.GetByIdAsync(id, cancellationToken);
        if (qualification is null || !await CanAccessEmployeeAsync(qualification.EmployeeId, cancellationToken))
            return NotFound();

        var fileName = Path.GetFileName(qualification.StoredFileName);
        var directory = Path.GetFullPath(GetStorageDirectory());
        var fullPath = Path.GetFullPath(Path.Combine(directory, fileName));
        if (!fullPath.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(fullPath, qualification.ContentType, qualification.OriginalFileName);
    }

    private async Task<bool> CanAccessEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr) || User.IsInRole(RoleNames.Manager))
            return true;
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(identityUserId) &&
               await profileService.CanEditAsync(employeeId, identityUserId, cancellationToken);
    }

    private void PrepareForm(Employee employee, bool returnToProfile)
    {
        ViewBag.EmployeeName = employee.FirstName + " " + employee.LastName;
        ViewBag.ReturnToProfile = returnToProfile;
    }

    private void RemoveServerManagedModelState()
    {
        foreach (var key in new[]
        {
            nameof(EmployeeQualification.Employee),
            nameof(EmployeeQualification.OriginalFileName),
            nameof(EmployeeQualification.StoredFileName),
            nameof(EmployeeQualification.ContentType)
        })
            ModelState.Remove(key);
    }

    private (string? ContentType, string? Extension, string? Error) ValidateUpload(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return (null, null, "Choose a qualification document to upload.");
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

    private string GetStorageDirectory() =>
        Path.Combine(environment.ContentRootPath, "App_Data", "EmployeeQualifications");

    private void DeleteStoredFile(string path)
    {
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }
}

