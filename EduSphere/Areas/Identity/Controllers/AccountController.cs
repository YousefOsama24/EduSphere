using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Identity.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
