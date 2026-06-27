using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Center.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
