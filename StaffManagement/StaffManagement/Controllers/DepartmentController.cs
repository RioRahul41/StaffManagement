using Microsoft.AspNetCore.Mvc;

namespace StaffManagement.Web.Controllers
{
    public class DepartmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
