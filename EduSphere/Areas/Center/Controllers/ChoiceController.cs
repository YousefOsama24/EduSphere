using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;


namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class ChoiceController : Controller
    {

        private readonly IRepository<Choice> _ChoiceRepository;

        public ChoiceController(IRepository<Choice> ChoiceRepository)
        {
            _ChoiceRepository = ChoiceRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Choices = await _ChoiceRepository.GetAsync(
                includes: new Expression<Func<Choice, object>>[]
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
            return View(new Choice());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Choice choice, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(choice);

            await _ChoiceRepository.CreateAsync(choice, cancellationToken);
            await _ChoiceRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Choice added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var choice = await _ChoiceRepository.GetOneAsync(
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

            var oldChoice = await _ChoiceRepository.GetOneAsync(
                c => c.ChoiceId == choice.ChoiceId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldChoice == null)
                return NotFound();

            oldChoice.QuestionId = choice.QuestionId;
            oldChoice.Content = choice.Content;
            oldChoice.IsCorrect = choice.IsCorrect;

            await _ChoiceRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Choice updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var choice = await _ChoiceRepository.GetOneAsync(
                c => c.ChoiceId == id,
                cancellationToken: cancellationToken);

            if (choice == null)
                return NotFound();

            _ChoiceRepository.Delete(choice);
            await _ChoiceRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Choice deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
