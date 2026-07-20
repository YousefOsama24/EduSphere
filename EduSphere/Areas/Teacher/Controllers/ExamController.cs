using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ExamModel = EduSphere.Models.Exam;
using EduSphere.ViewModel;
using EduSphere.Models;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    [Authorize(Roles = "Teacher,SuperAdmin")]
    public class ExamController : Controller
    {

        private readonly IRepository<ExamModel> _context;

        public ExamController(IRepository<ExamModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Exams = await _context.GetAsync(
                includes: new Expression<Func<ExamModel, object>>[]
                {
                    s => s.Course
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Exams = Exams.Where(e => (e.Title).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Exams.Count() / 3.0);
            Exams = Exams.Skip((page - 1) * 3).Take(3);

            return View(new ExamsVM()
            {
                Exams = Exams.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public IActionResult Create()
        {
            return View(new ExamModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Create(ExamModel Exam, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Exam);

            await _context.CreateAsync(Exam, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Exam added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Exam = await _context.GetOneAsync(
                c => c.ExamId == id,
                cancellationToken: cancellationToken);

            if (Exam == null)
                return NotFound();

            return View(Exam);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Update(Exam Exam, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Exam);

            var oldExam = await _context.GetOneAsync(
                c => c.ExamId == Exam.ExamId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldExam == null)
                return NotFound();

            oldExam.CourseId = Exam.CourseId;
            oldExam.Description = Exam.Description;
            oldExam.Title = Exam.Title;
            oldExam.DurationMinutes = Exam.DurationMinutes;
            oldExam.TotalMarks = Exam.TotalMarks;
            oldExam.StartDate = Exam.StartDate;
            oldExam.EndDate = Exam.EndDate;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Exam updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Teacher,SuperAdmin")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Exam = await _context.GetOneAsync(
                c => c.ExamId == id,
                cancellationToken: cancellationToken);

            if (Exam == null)
                return NotFound();

            _context.Delete(Exam);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Exam deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
