using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult HomeCast()
        {
            {
                return View();
            }
        }
        public IActionResult ParentDashboard()
        {
            {
                return View();
            }
        }
        public IActionResult StudentDashboard()
        {
            {
                return View();
            }
        }
        public IActionResult TeacherDashboard()
        {
            {
                return View();
            }
        }


    }
}
