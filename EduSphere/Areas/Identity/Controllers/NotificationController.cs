using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Identity.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
