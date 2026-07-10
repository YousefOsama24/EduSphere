using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Student.Controllers
{
    [Area(SD.STUDENT_AREA)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
