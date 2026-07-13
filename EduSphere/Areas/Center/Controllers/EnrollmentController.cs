using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class EnrollmentController : Controller
    {

        private readonly IRepository<Enrollment> _enrollmentRepository;

        public EnrollmentController(IRepository<Enrollment> enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Enrollments = await _enrollmentRepository.GetAsync(
                includes: new Expression<Func<Enrollment, object>>[]
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
            return View(new Enrollment());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Enrollment Enrollment, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Enrollment);



            await _enrollmentRepository.CreateAsync(Enrollment, cancellationToken);
            await _enrollmentRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Enrollment Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update()
        {
            return View(new Enrollment());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Student = await _enrollmentRepository.GetOneAsync(
                c => c.StudentId == id,
                cancellationToken: cancellationToken);

            if (Student == null)
                return NotFound();

            _enrollmentRepository.Delete(Student);
            await _enrollmentRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
