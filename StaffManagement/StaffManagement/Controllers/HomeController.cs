using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;
using StaffManagement.Models;
using System.Diagnostics;

namespace StaffManagement.Controllers
{
    public class HomeController(
        IEmployeeService employeeService,
        IHolidayService holidayService,
        UserManager<ApplicationIdentity> userManager) : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login", "Account");

            if (User.IsInRole(RoleNames.Admin))
                return RedirectToAction(nameof(Admin));
            if (User.IsInRole(RoleNames.Hr))
                return RedirectToAction(nameof(Hr));
            if (User.IsInRole(RoleNames.Manager))
                return RedirectToAction(nameof(Manager));
            if (User.IsInRole(RoleNames.TechnicalSupport))
                return RedirectToAction(nameof(TechnicalSupport));

            return RedirectToAction("User");
        }

        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Admin(CancellationToken cancellationToken)
        {
            return View(await BuildDashboardViewModelAsync(RoleNames.Admin, cancellationToken));
        }

        [Authorize(Roles = RoleNames.Hr)]
        public async Task<IActionResult> Hr(CancellationToken cancellationToken)
        {
            return View(await BuildDashboardViewModelAsync(RoleNames.Hr, cancellationToken));
        }

        [Authorize(Roles = RoleNames.Manager)]
        public async Task<IActionResult> Manager(CancellationToken cancellationToken)
        {
            return View(await BuildDashboardViewModelAsync(RoleNames.Manager, cancellationToken));
        }

        [Authorize(Roles = RoleNames.User + "," + RoleNames.TechnicalSupport)]
        [ActionName("User")]
        public async Task<IActionResult> UserDashboard(CancellationToken cancellationToken)
        {
            if (User.IsInRole(RoleNames.TechnicalSupport))
                return RedirectToAction(nameof(TechnicalSupport));

            return View("User", await BuildDashboardViewModelAsync(RoleNames.User, cancellationToken));
        }

        [Authorize(Roles = RoleNames.TechnicalSupport)]
        public async Task<IActionResult> TechnicalSupport(CancellationToken cancellationToken)
        {
            return View(await BuildDashboardViewModelAsync(RoleNames.TechnicalSupport, cancellationToken));
        }

        private async Task<DashboardViewModel> BuildDashboardViewModelAsync(
            string role,
            CancellationToken cancellationToken)
        {
            var employees = await employeeService.GetAllAsync(cancellationToken);
            var holidays = await holidayService.GetAllAsync(cancellationToken);
            var identityUser = await userManager.GetUserAsync(User);
            var displayName = $"{identityUser?.FirstName} {identityUser?.LastName}".Trim();
            var currentEmployee = role == RoleNames.User && identityUser is not null
                ? await employeeService.GetByIdentityUserIdAsync(identityUser.Id, cancellationToken)
                : null;
            var today = DateTime.Today;

            return new DashboardViewModel
            {
                Role = role,
                DisplayName = string.IsNullOrWhiteSpace(displayName)
                    ? User.Identity?.Name ?? "there"
                    : displayName,
                TotalEmployees = employees.Count,
                ActiveEmployees = employees.Count(employee => employee.EmpIsActive),
                CanCreateAccounts = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.Hr),
                CanManageEmployees = User.IsInRole(RoleNames.Admin)
                    || User.IsInRole(RoleNames.Hr)
                    || User.IsInRole(RoleNames.Manager),
                CurrentProjects = currentEmployee?.AssignedProjects ?? [],
                MonthlyBirthdays = employees
                    .Where(employee => employee.EmpIsActive
                        && employee.DOB.HasValue
                        && employee.DOB.Value.Month == today.Month)
                    .OrderBy(employee => employee.DOB!.Value.Day)
                    .ThenBy(employee => employee.FirstName)
                    .Select(employee => new DashboardBirthdayItem
                    {
                        EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                        Designation = employee.Designation,
                        Birthday = employee.DOB!.Value,
                        IsCelebrated = employee.DOB.Value.Day < today.Day,
                        IsToday = employee.DOB.Value.Day == today.Day
                    })
                    .ToList(),
                UpcomingHolidays = holidays
                    .Where(holiday => holiday.HolidayDate.Date >= today)
                    .OrderBy(holiday => holiday.HolidayDate)
                    .Select(holiday => new DashboardHolidayItem
                    {
                        HolidayName = holiday.HolidayName,
                        HolidayDate = holiday.HolidayDate
                    })
                    .ToList(),
                MonthlyWorkAnniversaries = employees
                    .Where(employee => employee.EmpIsActive
                        && employee.CreatedTime.Month == today.Month
                        && employee.CreatedTime.Year < today.Year)
                    .OrderBy(employee => employee.CreatedTime.Day)
                    .ThenBy(employee => employee.FirstName)
                    .Select(employee => new DashboardWorkAnniversaryItem
                    {
                        EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                        Designation = employee.Designation,
                        AnniversaryDate = GetAnniversaryDate(employee.CreatedTime, today.Year),
                        CompletedYears = today.Year - employee.CreatedTime.Year
                    })
                    .ToList()
            };
        }

        private static DateTime GetAnniversaryDate(DateTime createdTime, int year)
        {
            var day = Math.Min(createdTime.Day, DateTime.DaysInMonth(year, createdTime.Month));
            return new DateTime(year, createdTime.Month, day);
        }

        public IActionResult Privacy()
        {
            return RedirectToAction("Index", "Policy");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
