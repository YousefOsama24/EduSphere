using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using EnrollmentModel = EduSphere.Models.Enrollment;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class EnrollmentController : Controller
    {

        private readonly IRepository<EnrollmentModel> _context;

        public EnrollmentController(IRepository<EnrollmentModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Enrollments = await _context.GetAsync(
                includes: new Expression<Func<EnrollmentModel, object>>[]
                {
                    s => s.Student
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Enrollments = Enrollments.Where(e => (e.Student.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Enrollments.Count() / 3.0);
            Enrollments = Enrollments.Skip((page - 1) * 3).Take(3);

            return View(new EnrollmentsVM()
            {
                Enrollments = Enrollments.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new EnrollmentModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(EnrollmentModel Enrollment, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Enrollment);



            await _context.CreateAsync(Enrollment, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Enrollment Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
