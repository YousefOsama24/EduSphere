using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using QuestionModel = EduSphere.Models.Question;
using EduSphere.ViewModel;
using EduSphere.Models;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    [Authorize(Roles = "Teacher,SuperAdmin")]
    public class QuestionController : Controller
    {

        private readonly IRepository<QuestionModel> _context;

        public QuestionController(IRepository<QuestionModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Questions = await _context.GetAsync(
                includes: new Expression<Func<QuestionModel, object>>[]
                {
                    s => s.Exam
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Questions = Questions.Where(e => (e.Content).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Questions.Count() / 10.0);
            Questions = Questions.Skip((page - 1) * 10).Take(10);

            return View(new QuestionsVM()
            {
                Questions = Questions.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new QuestionModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionModel Question, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Question);

            await _context.CreateAsync(Question, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Question added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Question = await _context.GetOneAsync(
                c => c.QuestionId == id,
                cancellationToken: cancellationToken);

            if (Question == null)
                return NotFound();

            return View(Question);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Question Question, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Question);

            var oldQuestion = await _context.GetOneAsync(
                c => c.QuestionId == Question.QuestionId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldQuestion == null)
                return NotFound();

            oldQuestion.ExamId = Question.ExamId;
            oldQuestion.Content = Question.Content;
            oldQuestion.Type = Question.Type;
            oldQuestion.Marks = Question.Marks;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Question updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Question = await _context.GetOneAsync(
                c => c.QuestionId == id,
                cancellationToken: cancellationToken);

            if (Question == null)
                return NotFound();

            _context.Delete(Question);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Question deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
