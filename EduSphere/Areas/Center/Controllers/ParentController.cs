using EduSphere.Repositories.IRepositories;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class ParentController : Controller
    {

        private readonly IRepository<Parent> _ParentRepository;

        public ParentController(IRepository<Parent> ParentRepository)
        {
            _ParentRepository = ParentRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Parents = await _ParentRepository.GetAsync(
                includes: new Expression<Func<Parent, object>>[]
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
            return View(new Parent());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Parent Parent, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Parent);



            await _ParentRepository.CreateAsync(Parent, cancellationToken);
            await _ParentRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Parent Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update()
        {
            return View(new Parent());
        }

        //[HttpPost]
        //public IActionResult Update()
        //{
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Parent = await _ParentRepository.GetOneAsync(
                c => c.ParentId == id,
                cancellationToken: cancellationToken);

            if (Parent == null)
                return NotFound();

            _ParentRepository.Delete(Parent);
            await _ParentRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Parent deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
