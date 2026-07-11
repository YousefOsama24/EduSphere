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
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Enrollment = await _context.GetOneAsync(
                a => a.EnrollmentId == id,
                includes: new Expression<Func<EnrollmentModel, object>>[]
                {
                    a => a.Student,
                    a => a.Group
                },
                cancellationToken: cancellationToken);

            if (Enrollment == null)
                return NotFound();

            return View(Enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(EnrollmentModel Enrollment, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Enrollment);

            var oldEnrollment = await _context.GetOneAsync(
                a => a.EnrollmentId == Enrollment.EnrollmentId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldEnrollment == null)
                return NotFound();

            oldEnrollment.GroupId = Enrollment.GroupId;
            oldEnrollment.StudentId = Enrollment.StudentId;
            oldEnrollment.Status = Enrollment.Status;
            oldEnrollment.EnrollmentDate = Enrollment.EnrollmentDate;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Attendance Record updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Student = await _context.GetOneAsync(
                c => c.EnrollmentId == id,
                cancellationToken: cancellationToken);

            if (Student == null)
                return NotFound();

            _context.Delete(Student);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
