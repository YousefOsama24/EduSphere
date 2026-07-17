using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using CourseModel = EduSphere.Models.Course;
using EduSphere.ViewModel;
using EduSphere.Models;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    [Authorize(Roles = "Teacher,SuperAdmin")]
    public class CourseController : Controller
    {

        private readonly IRepository<CourseModel> _context;

        public CourseController(IRepository<CourseModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Courses = await _context.GetAsync(
                includes: new Expression<Func<CourseModel, object>>[]
                {
                    s => s.Center
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Courses = Courses.Where(e => (e.Title).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Courses.Count() / 10.0);
            Courses = Courses.Skip((page - 1) * 10).Take(10);

            return View(new CoursesVM()
            {
                Courses = Courses.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CourseModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseModel Course, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Course);

            await _context.CreateAsync(Course, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Course added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Course = await _context.GetOneAsync(
                c => c.CourseId == id,
                cancellationToken: cancellationToken);

            if (Course == null)
                return NotFound();

            return View(Course);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Course Course, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Course);

            var oldCourse = await _context.GetOneAsync(
                c => c.CourseId == Course.CourseId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldCourse == null)
                return NotFound();

            oldCourse.Title = Course.Title;
            oldCourse.CenterId = Course.CenterId;
            oldCourse.TeacherId = Course.TeacherId;
            oldCourse.Description = Course.Description;
            oldCourse.Price = Course.Price;
            oldCourse.ThumbnailUrl = Course.ThumbnailUrl;
            oldCourse.CreatedAt = Course.CreatedAt;
            oldCourse.UpdatedAt = Course.UpdatedAt;
            oldCourse.IsPublished = Course.IsPublished;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Course updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Course = await _context.GetOneAsync(
                c => c.CourseId == id,
                cancellationToken: cancellationToken);

            if (Course == null)
                return NotFound();

            _context.Delete(Course);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Course deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
