using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.SuperAdmin.Controllers
{
    
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
