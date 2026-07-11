using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using SubscriptionModel = EduSphere.Models.Subscription;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class SubscriptionController : Controller
    {

        private readonly IRepository<SubscriptionModel> _context;

        public SubscriptionController(IRepository<SubscriptionModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Subscriptions = await _context.GetAsync(
                includes: new Expression<Func<SubscriptionModel, object>>[]
                {
                    s => s.SubscriptionPlan
                    
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Subscriptions = Subscriptions.Where(e =>
                    e.SubscriptionId.ToString().Contains(q) ||
                    e.CenterId.ToString().Contains(q) ||
                    e.SubscriptionPlanId.ToString().Contains(q)
                );
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Subscriptions.Count() / 10.0);
            Subscriptions = Subscriptions.Skip((page - 1) * 10).Take(10);

            return View(new SubscriptionsVM()
            {
                Subscriptions = Subscriptions.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SubscriptionModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriptionModel Subscription, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Subscription);

            await _context.CreateAsync(Subscription, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Subscription added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Subscription = await _context.GetOneAsync(
                c => c.SubscriptionId == id,
                cancellationToken: cancellationToken);

            if (Subscription == null)
                return NotFound();

            return View(Subscription);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Subscription Subscription, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Subscription);

            var oldSubscription = await _context.GetOneAsync(
                c => c.SubscriptionId == Subscription.SubscriptionId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldSubscription == null)
                return NotFound();

            oldSubscription.CenterId = Subscription.CenterId;
            oldSubscription.SubscriptionPlanId = Subscription.SubscriptionPlanId;
            oldSubscription.StartDate = Subscription.StartDate;
            oldSubscription.EndDate = Subscription.EndDate;
            oldSubscription.Status = Subscription.Status;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Subscription updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Subscription = await _context.GetOneAsync(
                c => c.SubscriptionId == id,
                cancellationToken: cancellationToken);

            if (Subscription == null)
                return NotFound();

            _context.Delete(Subscription);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Subscription deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
