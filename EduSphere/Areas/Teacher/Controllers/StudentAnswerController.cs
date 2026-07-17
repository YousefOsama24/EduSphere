using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using StudentAnswerModel = EduSphere.Models.StudentAnswer;
using EduSphere.ViewModel;
using EduSphere.Models;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    [Authorize(Roles = "Teacher,SuperAdmin")]

    public class StudentAnswerController : Controller
    {

        private readonly IRepository<StudentAnswerModel> _context;

        public StudentAnswerController(IRepository<StudentAnswerModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var StudentAnswers = await _context.GetAsync(
                includes: new Expression<Func<StudentAnswerModel, object>>[]
                {
                    s => s.Question
                    
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                StudentAnswers = StudentAnswers.Where(e => (e.EssayAnswer).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(StudentAnswers.Count() / 10.0);
            StudentAnswers = StudentAnswers.Skip((page - 1) * 10).Take(10);

            return View(new StudentAnswersVM()
            {
                StudentAnswers = StudentAnswers.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new StudentAnswerModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentAnswerModel StudentAnswer, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(StudentAnswer);

            await _context.CreateAsync(StudentAnswer, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "StudentAnswer added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var StudentAnswer = await _context.GetOneAsync(
                c => c.StudentAnswerId == id,
                cancellationToken: cancellationToken);

            if (StudentAnswer == null)
                return NotFound();

            return View(StudentAnswer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(StudentAnswer StudentAnswer, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(StudentAnswer);

            var oldStudentAnswer = await _context.GetOneAsync(
                c => c.StudentAnswerId == StudentAnswer.StudentAnswerId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldStudentAnswer == null)
                return NotFound();

            oldStudentAnswer.ExamAttemptId = StudentAnswer.ExamAttemptId;
            oldStudentAnswer.QuestionId = StudentAnswer.QuestionId;
            oldStudentAnswer.SelectedChoiceId = StudentAnswer.SelectedChoiceId;
            oldStudentAnswer.EssayAnswer = StudentAnswer.EssayAnswer;
            oldStudentAnswer.MarksAwarded = StudentAnswer.MarksAwarded;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "StudentAnswer updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var StudentAnswer = await _context.GetOneAsync(
                c => c.StudentAnswerId == id,
                cancellationToken: cancellationToken);

            if (StudentAnswer == null)
                return NotFound();

            _context.Delete(StudentAnswer);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "StudentAnswer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
