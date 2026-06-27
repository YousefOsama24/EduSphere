using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Center.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
