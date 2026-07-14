using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using SubscriptionPlanModel = EduSphere.Models.SubscriptionPlan;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class SubscriptionPlanController : Controller
    {

        private readonly IRepository<SubscriptionPlanModel> _context;

        public SubscriptionPlanController(IRepository<SubscriptionPlanModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var SubscriptionPlans = await _context.GetAsync(
                includes: new Expression<Func<SubscriptionPlanModel, object>>[]
                {
                    s => s.Subscriptions
                    
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                SubscriptionPlans = SubscriptionPlans.Where(e => (e.Name).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(SubscriptionPlans.Count() / 10.0);
            SubscriptionPlans = SubscriptionPlans.Skip((page - 1) * 10).Take(10);

            return View(new SubscriptionPlansVM()
            {
                SubscriptionPlans = SubscriptionPlans.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SubscriptionPlanModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriptionPlanModel SubscriptionPlan, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(SubscriptionPlan);

            await _context.CreateAsync(SubscriptionPlan, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "SubscriptionPlan added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var SubscriptionPlan = await _context.GetOneAsync(
                c => c.SubscriptionPlanId == id,
                cancellationToken: cancellationToken);

            if (SubscriptionPlan == null)
                return NotFound();

            return View(SubscriptionPlan);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SubscriptionPlan SubscriptionPlan, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(SubscriptionPlan);

            var oldSubscriptionPlan = await _context.GetOneAsync(
                c => c.SubscriptionPlanId == SubscriptionPlan.SubscriptionPlanId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldSubscriptionPlan == null)
                return NotFound();

            oldSubscriptionPlan.Name = SubscriptionPlan.Name;
            oldSubscriptionPlan.Tier = SubscriptionPlan.Tier;
            oldSubscriptionPlan.Price = SubscriptionPlan.Price;
            oldSubscriptionPlan.DurationInMonths = SubscriptionPlan.DurationInMonths;
            oldSubscriptionPlan.MaxTeachers = SubscriptionPlan.MaxTeachers;
            oldSubscriptionPlan.MaxStudents = SubscriptionPlan.MaxStudents;
            oldSubscriptionPlan.Features = SubscriptionPlan.Features;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "SubscriptionPlan updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var SubscriptionPlan = await _context.GetOneAsync(
                c => c.SubscriptionPlanId == id,
                cancellationToken: cancellationToken);

            if (SubscriptionPlan == null)
                return NotFound();

            _context.Delete(SubscriptionPlan);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "SubscriptionPlan deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
