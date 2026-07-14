using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ChoiceModel = EduSphere.Models.Choice;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    public class ChoiceController : Controller
    {

        private readonly IRepository<ChoiceModel> _context;

        public ChoiceController(IRepository<ChoiceModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Choices = await _context.GetAsync(
                includes: new Expression<Func<ChoiceModel, object>>[]
                {
                    s => s.Question
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Choices = Choices.Where(e => (e.Content).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Choices.Count() / 10.0);
            Choices = Choices.Skip((page - 1) * 10).Take(10);

            return View(new ChoicesVM()
            {
                Choices = Choices.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ChoiceModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChoiceModel choice, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(choice);

            await _context.CreateAsync(choice, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Choice added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var choice = await _context.GetOneAsync(
                c => c.ChoiceId == id,
                cancellationToken: cancellationToken);

            if (choice == null)
                return NotFound();

            return View(choice);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Choice choice, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(choice);

            var oldChoice = await _context.GetOneAsync(
                c => c.ChoiceId == choice.ChoiceId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldChoice == null)
                return NotFound();

            oldChoice.QuestionId = choice.QuestionId;
            oldChoice.Content = choice.Content;
            oldChoice.IsCorrect = choice.IsCorrect;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Choice updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var choice = await _context.GetOneAsync(
                c => c.ChoiceId == id,
                cancellationToken: cancellationToken);

            if (choice == null)
                return NotFound();

            _context.Delete(choice);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Choice deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
