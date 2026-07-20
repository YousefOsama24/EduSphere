using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using LectureModel = EduSphere.Models.Lecture;
using EduSphere.ViewModel;
using EduSphere.Models;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    [Authorize(Roles = "Teacher,SuperAdmin")]
    public class LectureController : Controller
    {

        private readonly IRepository<LectureModel> _context;

        public LectureController(IRepository<LectureModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Lectures = await _context.GetAsync(
                includes: new Expression<Func<LectureModel, object>>[]
                {
                    s => s.Course
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Lectures = Lectures.Where(e => (e.Title).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Lectures.Count() / 10.0);
            Lectures = Lectures.Skip((page - 1) * 10).Take(10);

            return View(new LecturesVM()
            {
                Lectures = Lectures.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public IActionResult Create()
        {
            return View(new LectureModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Create(LectureModel Lecture, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Lecture);

            await _context.CreateAsync(Lecture, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Lecture added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Lecture = await _context.GetOneAsync(
                c => c.LectureId == id,
                cancellationToken: cancellationToken);

            if (Lecture == null)
                return NotFound();

            return View(Lecture);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Update(Lecture Lecture, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Lecture);

            var oldLecture = await _context.GetOneAsync(
                c => c.LectureId == Lecture.LectureId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldLecture == null)
                return NotFound();

            oldLecture.Title = Lecture.Title;
            oldLecture.CourseId = Lecture.CourseId;
            oldLecture.VideoUrl = Lecture.VideoUrl;
            oldLecture.Description = Lecture.Description;
            oldLecture.PdfUrl = Lecture.PdfUrl;
            oldLecture.Order = Lecture.Order;
            oldLecture.CreatedAt = Lecture.CreatedAt;
            oldLecture.IsPreview = Lecture.IsPreview;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Lecture updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Lecture = await _context.GetOneAsync(
                c => c.LectureId == id,
                cancellationToken: cancellationToken);

            if (Lecture == null)
                return NotFound();

            _context.Delete(Lecture);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Lecture deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
