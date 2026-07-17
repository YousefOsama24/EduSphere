using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class EnrollmentController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly ILogger<EnrollmentController> _logger;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<Course> _courseRepository;

        public EnrollmentController(
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Student> studentRepository,
    IRepository<Course> courseRepository,
    ILogger<EnrollmentController> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        #region Index

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var enrollments = await _enrollmentRepository.GetAsync(
                includes: new Expression<Func<Enrollment, object>>[]
                {
                    x => x.Student
                },
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();

                enrollments = enrollments.Where(x =>
                    x.Student.User.FullName.ToLower().Contains(search));

                ViewBag.Query = query;
            }

            double totalPages =
                Math.Ceiling(enrollments.Count() / (double)PageSize);

            enrollments = enrollments
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new EnrollmentsVM
            {
                Enrollments = enrollments.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<IActionResult> Create(
    CancellationToken cancellationToken = default)
        {
            var students =
                await _studentRepository.GetAsync(
                    cancellationToken: cancellationToken);

            var courses =
                await _courseRepository.GetAsync(
                    cancellationToken: cancellationToken);

            ViewBag.Students = new SelectList(
                students,
                "StudentId",
                "StudentId");

            ViewBag.Courses = new SelectList(
                courses,
                "CourseId",
                "Title");


            return View(new Enrollment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
      Enrollment enrollment,
      CancellationToken cancellationToken = default)
        {
            var students = await _studentRepository.GetAsync(
                cancellationToken: cancellationToken);

            var courses = await _courseRepository.GetAsync(
                cancellationToken: cancellationToken);

            if (!ModelState.IsValid)
            {
                ViewBag.Students = new SelectList(
                    students,
                    "StudentId",
                    "StudentId",
                    enrollment.StudentId);

                ViewBag.Courses = new SelectList(
                    courses,
                    "CourseId",
                    "Title",
                    enrollment.CourseId);

                return View(enrollment);
            }

            bool exists = (
                await _enrollmentRepository.GetAsync(
                    x => x.StudentId == enrollment.StudentId &&
                         x.CourseId == enrollment.CourseId,
                    cancellationToken: cancellationToken))
                .Any();

            if (exists)
            {
                ModelState.AddModelError(
                    "",
                    "Student is already enrolled in this course.");

                ViewBag.Students = new SelectList(
                    students,
                    "StudentId",
                    "StudentId",
                    enrollment.StudentId);

                ViewBag.Courses = new SelectList(
                    courses,
                    "CourseId",
                    "Title",
                    enrollment.CourseId);

                return View(enrollment);
            }

            try
            {
                await _enrollmentRepository.CreateAsync(
                    enrollment,
                    cancellationToken);

                await _enrollmentRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Enrollment created successfully.";

                _logger.LogInformation(
                    "Enrollment created successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating enrollment.");

                TempData["Error"] =
                    "Something went wrong while creating the enrollment.";

                ViewBag.Students = new SelectList(
                    students,
                    "StudentId",
                    "StudentId",
                    enrollment.StudentId);

                ViewBag.Courses = new SelectList(
                    courses,
                    "CourseId",
                    "Title",
                    enrollment.CourseId);

                return View(enrollment);
            }
        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> Details(
    int id,
    CancellationToken cancellationToken = default)
        {
            var enrollment =
                await _enrollmentRepository.GetOneAsync(
                    x => x.EnrollmentId == id,
                    includes: new Expression<Func<Enrollment, object>>[]
                    {
                x => x.Student,
                x => x.Course
                    },
                    cancellationToken: cancellationToken);

            if (enrollment == null)
            {
                _logger.LogWarning(
                    "Enrollment not found. Id: {Id}",
                    id);

                return NotFound();
            }

            return View(enrollment);
        }
        #region Edit

        [HttpGet]
        public async Task<IActionResult> Update(
            int id,
            CancellationToken cancellationToken = default)
        {
            var enrollment = await _enrollmentRepository.GetOneAsync(
                x => x.EnrollmentId == id,
                tracked: true,
                cancellationToken: cancellationToken);

            if (enrollment == null)
                return NotFound();

            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Enrollment enrollment,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(enrollment);

            try
            {
                var oldEnrollment = await _enrollmentRepository.GetOneAsync(
                    x => x.EnrollmentId == enrollment.EnrollmentId,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (oldEnrollment == null)
                    return NotFound();

                oldEnrollment.StudentId = enrollment.StudentId;
                oldEnrollment.GroupId = enrollment.GroupId;
                oldEnrollment.CourseId = enrollment.CourseId;
                oldEnrollment.EnrollmentDate = enrollment.EnrollmentDate;
                oldEnrollment.Status = enrollment.Status;

                await _enrollmentRepository.CommitAsync(cancellationToken);

                TempData["Success"] =
                    "Enrollment updated successfully.";

                _logger.LogInformation(
                    "Enrollment updated successfully. Id: {Id}",
                    enrollment.EnrollmentId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating enrollment. Id: {Id}",
                    enrollment.EnrollmentId);

                TempData["Error"] =
                    "Something went wrong while updating the enrollment.";

                return View(enrollment);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var enrollment = await _enrollmentRepository.GetOneAsync(
                    x => x.EnrollmentId == id,
                    cancellationToken: cancellationToken);

                if (enrollment == null)
                    return NotFound();

                _enrollmentRepository.Delete(enrollment);

                await _enrollmentRepository.CommitAsync(cancellationToken);

                TempData["Success"] =
                    "Enrollment deleted successfully.";

                _logger.LogInformation(
                    "Enrollment deleted successfully. Id: {Id}",
                    id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting enrollment. Id: {Id}",
                    id);

                TempData["Error"] =
                    "Something went wrong while deleting the enrollment.";

                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}
