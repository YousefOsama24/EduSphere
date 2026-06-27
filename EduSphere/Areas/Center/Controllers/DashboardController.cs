using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Center.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
