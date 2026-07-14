using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ParentModel = EduSphere.Models.Parent;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
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
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Parent = await _context.GetOneAsync(
                a => a.ParentId == id,
                includes: new Expression<Func<ParentModel, object>>[]
                {
                    a => a.User,

                },
                cancellationToken: cancellationToken);

            if (Parent == null)
                return NotFound();

            return View(Parent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ParentModel Parent, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Parent);

            var oldParent = await _context.GetOneAsync(
                a => a.ParentId == Parent.ParentId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldParent == null)
                return NotFound();

            oldParent.Occupation = Parent.Occupation;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Attendance Record updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Parent = await _context.GetOneAsync(
                c => c.ParentId == id,
                cancellationToken: cancellationToken);

            if (Parent == null)
                return NotFound();

            _context.Delete(Parent);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Parent deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
