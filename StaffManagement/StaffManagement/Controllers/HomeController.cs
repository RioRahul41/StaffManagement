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

        [Authorize(Roles = RoleNames.User)]
        [ActionName("User")]
        public async Task<IActionResult> UserDashboard(CancellationToken cancellationToken)
        {
            return View("User", await BuildDashboardViewModelAsync(RoleNames.User, cancellationToken));
        }

        private async Task<DashboardViewModel> BuildDashboardViewModelAsync(
            string role,
            CancellationToken cancellationToken)
        {
            var employees = await employeeService.GetAllAsync(cancellationToken);
            var identityUser = await userManager.GetUserAsync(User);
            var displayName = $"{identityUser?.FirstName} {identityUser?.LastName}".Trim();

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
                    || User.IsInRole(RoleNames.Manager)
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
