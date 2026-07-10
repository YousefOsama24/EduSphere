using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Teacher.Controllers
{

    [Area(SD.TEACHER_AREA)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
