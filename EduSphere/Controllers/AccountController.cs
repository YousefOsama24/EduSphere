using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
