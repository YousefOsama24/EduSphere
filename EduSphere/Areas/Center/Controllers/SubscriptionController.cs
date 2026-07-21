using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using EduSphere.ViewModel;

using SubscriptionModel = EduSphere.Models.Subscription;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class SubscriptionController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<SubscriptionModel> _subscriptionRepository;
        private readonly IRepository<EduSphere.Models.Center> _centerRepository;
        private readonly IRepository<SubscriptionPlan> _subscriptionPlanRepository;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            IRepository<SubscriptionModel> subscriptionRepository,
            IRepository<EduSphere.Models.Center> centerRepository,
            IRepository<SubscriptionPlan> subscriptionPlanRepository,
            ILogger<SubscriptionController> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _centerRepository = centerRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _logger = logger;
        }

        #region Index

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            Expression<Func<SubscriptionModel, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();
                filter = x => x.SubscriptionId.ToString().Contains(search) ||
                              x.Center.Name.ToLower().Contains(search) ||
                              x.SubscriptionPlan.Name.ToLower().Contains(search);

                ViewBag.Query = query;
            }

            // سحب البيانات من الداتابيز مباشرة مع الـ Includes والـ Pagination
            var subscriptions = await _subscriptionRepository.GetAsync(
                filter: filter,
                includes: new Expression<Func<SubscriptionModel, object>>[]
                {
                    x => x.SubscriptionPlan,
                    x => x.Center
                },
                skip: (page - 1) * PageSize,
                take: PageSize,
                tracked: false,
                cancellationToken: cancellationToken);

            int totalCount = await _subscriptionRepository.CountAsync(
                predicate: filter,
                cancellationToken: cancellationToken);

            double totalPages = Math.Ceiling(totalCount / (double)PageSize);

            return View(new SubscriptionsVM
            {
                Subscriptions = subscriptions,
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            await PopulateDropdownsAsync(cancellationToken);
            return View(new SubscriptionModel { StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SubscriptionModel subscription,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(cancellationToken, subscription.CenterId, subscription.SubscriptionPlanId);
                return View(subscription);
            }

            try
            {
                await _subscriptionRepository.CreateAsync(subscription, cancellationToken);
                await _subscriptionRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Subscription created successfully.";
                _logger.LogInformation("Subscription created successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating subscription.");
                TempData["Error"] = "Something went wrong while creating the subscription.";

                await PopulateDropdownsAsync(cancellationToken, subscription.CenterId, subscription.SubscriptionPlanId);
                return View(subscription);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Update(
            int id,
            CancellationToken cancellationToken = default)
        {
            var subscription = await _subscriptionRepository.GetOneAsync(
                x => x.SubscriptionId == id,
                tracked: false,
                cancellationToken: cancellationToken);

            if (subscription == null)
                return NotFound();

            await PopulateDropdownsAsync(cancellationToken, subscription.CenterId, subscription.SubscriptionPlanId);
            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            SubscriptionModel subscription,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(cancellationToken, subscription.CenterId, subscription.SubscriptionPlanId);
                return View(subscription);
            }

            try
            {
                var oldSubscription = await _subscriptionRepository.GetOneAsync(
                    x => x.SubscriptionId == subscription.SubscriptionId,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (oldSubscription == null)
                    return NotFound();

                oldSubscription.CenterId = subscription.CenterId;
                oldSubscription.SubscriptionPlanId = subscription.SubscriptionPlanId;
                oldSubscription.StartDate = subscription.StartDate;
                oldSubscription.EndDate = subscription.EndDate;
                oldSubscription.Status = subscription.Status;

                await _subscriptionRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Subscription updated successfully.";
                _logger.LogInformation("Subscription updated successfully. Id: {Id}", subscription.SubscriptionId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating subscription. Id: {Id}", subscription.SubscriptionId);
                TempData["Error"] = "Something went wrong while updating the subscription.";

                await PopulateDropdownsAsync(cancellationToken, subscription.CenterId, subscription.SubscriptionPlanId);
                return View(subscription);
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
                var subscription = await _subscriptionRepository.GetOneAsync(
                    x => x.SubscriptionId == id,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (subscription == null)
                    return NotFound();

                _subscriptionRepository.Delete(subscription);
                await _subscriptionRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Subscription deleted successfully.";
                _logger.LogInformation("Subscription deleted successfully. Id: {Id}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting subscription. Id: {Id}", id);
                TempData["Error"] = "Something went wrong while deleting the subscription.";

                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Helper Methods

        private async Task PopulateDropdownsAsync(
            CancellationToken cancellationToken = default,
            object? selectedCenter = null,
            object? selectedPlan = null)
        {
            var centers = await _centerRepository.GetAsync(cancellationToken: cancellationToken);
            var plans = await _subscriptionPlanRepository.GetAsync(cancellationToken: cancellationToken);

            ViewBag.Centers = new SelectList(centers, "CenterId", "Name", selectedCenter);
            ViewBag.Plans = new SelectList(plans, "SubscriptionPlanId", "Name", selectedPlan);
        }

        #endregion
    }
}