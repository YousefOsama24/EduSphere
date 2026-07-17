using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Teacher.Controllers
{

    [Area(SD.TEACHER_AREA)]
    [Authorize(Roles = "Teacher,SuperAdmin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
      

    }
}
