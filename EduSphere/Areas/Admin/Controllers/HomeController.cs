using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
