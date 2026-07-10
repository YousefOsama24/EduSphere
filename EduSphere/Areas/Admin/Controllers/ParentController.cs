using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ParentModel = EduSphere.Models.Parent;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class ParentController : Controller
    {

        private readonly IRepository<ParentModel> _context;

        public ParentController(IRepository<ParentModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Parents = await _context.GetAsync(
                includes: new Expression<Func<ParentModel, object>>[]
                {
                    s => s.User
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Parents = Parents.Where(e => (e.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Parents.Count() / 3.0);
            Parents = Parents.Skip((page - 1) * 3).Take(3);

            return View(new ParentsVM()
            {
                Parents = Parents.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ParentModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ParentModel Parent, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Parent);



            await _context.CreateAsync(Parent, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Parent Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
