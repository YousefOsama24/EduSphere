using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
