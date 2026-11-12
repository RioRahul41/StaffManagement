using Microsoft.AspNetCore.Mvc;

namespace StaffManagement.Controllers
{
    public class DepartmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
