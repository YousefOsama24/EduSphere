using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class GroupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Update()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }
    }
}
