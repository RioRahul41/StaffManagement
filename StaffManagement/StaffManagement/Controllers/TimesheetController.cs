using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs.TimesheetModel;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Enums;

namespace StaffManagement.Controllers;

[Authorize]
public sealed class TimesheetController(
    ITimesheetService timesheetService,
    IEmployeeService employeeService,
    IProjectService projectService,
    IWebHostEnvironment environment) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Data(
        int draw = 1,
        int start = 0,
        int length = 10,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortDirection = null,
        int? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        TimesheetStatus? statusFilter = null;
        if (status.HasValue)
        {
            if (!Enum.IsDefined(typeof(TimesheetStatus), status.Value))
                return BadRequest();

            statusFilter = (TimesheetStatus)status.Value;
        }

        var page = await timesheetService.GetDataTablePageAsync(
            CanViewAllTimesheets(),
            CurrentIdentityUserId(),
            new TimesheetDataTableQueryDto
            {
                Start = start,
                Length = length,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortAscending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase),
                Status = statusFilter,
                DateFrom = dateFrom,
                DateTo = dateTo
            },
            cancellationToken);

        var canEdit = User.IsInRole("Admin") || User.IsInRole("Manager") ||
            (User.IsInRole("User") && !User.IsInRole("HR"));

        return Json(new
        {
            draw = Math.Max(draw, 0),
            recordsTotal = page.TotalCount,
            recordsFiltered = page.FilteredCount,
            data = page.Items.Select(item => new
            {
                id = item.Id,
                employeeName = item.EmployeeName,
                projectName = item.ProjectName,
                taskName = item.TaskName,
                description = item.Description,
                startDate = item.StartDate.ToString("yyyy-MM-dd"),
                startTime = item.StartTime.ToString("h\\:mm"),
                endTime = item.EndTime.ToString("h\\:mm"),
                status = (int)item.Status,
                statusLabel = item.Status == TimesheetStatus.InProgress ? "In Progress" : item.Status.ToString(),
                statusClass = item.Status switch
                {
                    TimesheetStatus.Completed => "text-bg-success",
                    TimesheetStatus.Pending => "text-bg-secondary",
                    _ => "text-bg-warning"
                },
                documentUrl = item.DocumentPath?.StartsWith("/uploads/timesheets/", StringComparison.OrdinalIgnoreCase) == true
                    ? item.DocumentPath
                    : null,
                viewUrl = Url.Action(nameof(Details), new { id = item.Id }),
                editUrl = Url.Action(nameof(Edit), new { id = item.Id }),
                canEdit
            })
        });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var timesheet = await timesheetService.GetByIdAsync(
            id,
            CanViewAllTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        return timesheet is null ? NotFound() : View(timesheet);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadEmployeesAsync(cancellationToken);
        await LoadProjectsAsync(cancellationToken);
        return View(new CreateTimesheetDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateTimesheetDto model,
        IFormFile? document,
        CancellationToken cancellationToken)
    {
        var canManage = CanCreateForOthers();
        var identityUserId = CurrentIdentityUserId();

        if (!ModelState.IsValid)
        {
            await LoadEmployeesAsync(cancellationToken);
            await LoadProjectsAsync(cancellationToken);
            return View(model);
        }

        var (documentPath, uploadError) = await SaveDocumentAsync(document, cancellationToken);
        if (uploadError is not null)
        {
            ModelState.AddModelError(nameof(document), uploadError);
            await LoadEmployeesAsync(cancellationToken);
            await LoadProjectsAsync(cancellationToken);
            return View(model);
        }

        var result = await timesheetService.CreateAsync(
            model,
            canManage,
            identityUserId,
            documentPath,
            cancellationToken);

        if (!result.Succeeded)
        {
            DeleteDocument(documentPath);
            ModelState.AddModelError(string.Empty, result.Error!);
            await LoadEmployeesAsync(cancellationToken);
            await LoadProjectsAsync(cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Timesheet created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var timesheet = await timesheetService.GetByIdAsync(
            id,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (timesheet is null)
            return NotFound();

        await LoadProjectsAsync(cancellationToken);
        return View(timesheet);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        TimesheetDto model,
        IFormFile? document,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync(cancellationToken);
            return View(model);
        }

        var existingTimesheet = await timesheetService.GetByIdAsync(
            model.Id,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (existingTimesheet is null)
            return NotFound();

        var (documentPath, uploadError) = await SaveDocumentAsync(document, cancellationToken);
        if (uploadError is not null)
        {
            ModelState.AddModelError(nameof(document), uploadError);
            await LoadProjectsAsync(cancellationToken);
            return View(model);
        }

        var result = await timesheetService.UpdateAsync(
            model,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            documentPath,
            cancellationToken);

        if (!result.Succeeded)
        {
            DeleteDocument(documentPath);
            ModelState.AddModelError(string.Empty, result.Error!);
            await LoadProjectsAsync(cancellationToken);
            return View(model);
        }

        if (documentPath is not null && existingTimesheet.DocumentPath is not null)
            DeleteDocument(existingTimesheet.DocumentPath);

        TempData["Success"] = "Timesheet updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        int id,
        TimesheetStatus status,
        CancellationToken cancellationToken)
    {
        var result = await timesheetService.ChangeStatusAsync(
            id,
            status,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
            TempData["Error"] = result.Error;
        else
            TempData["Success"] = "Timesheet status updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var timesheet = await timesheetService.GetByIdAsync(
            id,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        return timesheet is null ? NotFound() : View(timesheet);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var existingTimesheet = await timesheetService.GetByIdAsync(
            id,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        var result = await timesheetService.DeleteAsync(
            id,
            CanManageTimesheets(),
            CurrentIdentityUserId(),
            cancellationToken);

        if (!result.Succeeded)
            TempData["Error"] = result.Error;
        else
        {
            DeleteDocument(existingTimesheet?.DocumentPath);
            TempData["Success"] = "Timesheet deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEmployeesAsync(CancellationToken cancellationToken)
    {
        if (CanCreateForOthers())
            ViewBag.Employees = await employeeService.GetAllAsync(cancellationToken);
    }

    private async Task LoadProjectsAsync(CancellationToken cancellationToken)
    {
        var projects = await projectService.GetAllAsync();
        if (!CanCreateForOthers())
        {
            var employee = await employeeService.GetByIdentityUserIdAsync(
                CurrentIdentityUserId(), cancellationToken);
            projects = projects
                .Where(project => employee?.ProjectIds.Contains(project.ProjectId) == true)
                .ToList();
        }

        ViewBag.Projects = projects;
    }

    private bool CanManageTimesheets() =>
        User.IsInRole(RoleNames.Admin) ||
        User.IsInRole(RoleNames.Manager);

    private bool CanViewAllTimesheets() =>
        CanManageTimesheets() || User.IsInRole(RoleNames.Hr);

    private bool CanCreateForOthers() =>
        CanViewAllTimesheets();

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

        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");

        var uploadDirectory = Path.Combine(webRootPath, "uploads", "timesheets");
        Directory.CreateDirectory(uploadDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, storedFileName);
        await using var stream = System.IO.File.Create(filePath);
        await document.CopyToAsync(stream, cancellationToken);

        return ($"/uploads/timesheets/{storedFileName}", null);
    }

    private void DeleteDocument(string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath) ||
            !documentPath.StartsWith("/uploads/timesheets/", StringComparison.OrdinalIgnoreCase))
            return;

        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");

        var fileName = Path.GetFileName(documentPath);
        var filePath = Path.Combine(webRootPath, "uploads", "timesheets", fileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }
}
