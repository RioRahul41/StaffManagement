using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffManagement.Core.DTOs;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Models;
using System.Diagnostics;

namespace StaffManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = RoleNames.Admin)]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = RoleNames.Hr)]
        public IActionResult Hr()
        {
            return View();
        }

        [Authorize(Roles = RoleNames.Manager)]
        public IActionResult Manager()
        {
            return View();
        }

        [Authorize(Roles = RoleNames.User)]
        public IActionResult User()
        {
            return View();
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
