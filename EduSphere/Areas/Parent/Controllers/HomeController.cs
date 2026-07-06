using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Parent.Controllers
{
    [Area(SD.PAERNT_AREA)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
