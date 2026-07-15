using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using EduSphere.ViewModel;

using SubscriptionPlanModel = EduSphere.Models.SubscriptionPlan;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class SubscriptionPlanController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<SubscriptionPlanModel> _subscriptionPlanRepository;
        private readonly ILogger<SubscriptionPlanController> _logger;

        public SubscriptionPlanController(
            IRepository<SubscriptionPlanModel> subscriptionPlanRepository,
            ILogger<SubscriptionPlanController> logger)
        {
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _logger = logger;
        }

        #region Index

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var subscriptionPlans =
                await _subscriptionPlanRepository.GetAsync(
                    includes: new Expression<Func<SubscriptionPlanModel, object>>[]
                    {
                        x => x.Subscriptions
                    },
                    cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();

                subscriptionPlans = subscriptionPlans.Where(x =>
                    x.Name.ToLower().Contains(search));

                ViewBag.Query = query;
            }

            double totalPages =
                Math.Ceiling(subscriptionPlans.Count() / (double)PageSize);

            subscriptionPlans = subscriptionPlans
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new SubscriptionPlansVM
            {
                SubscriptionPlans = subscriptionPlans.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        #endregion

        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SubscriptionPlanModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SubscriptionPlanModel subscriptionPlan,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(subscriptionPlan);

            try
            {
                await _subscriptionPlanRepository.CreateAsync(
                    subscriptionPlan,
                    cancellationToken);

                await _subscriptionPlanRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Subscription plan created successfully.";

                _logger.LogInformation(
                    "Subscription plan created successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating subscription plan.");

                TempData["Error"] =
                    "Something went wrong while creating the subscription plan.";

                return View(subscriptionPlan);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Update(
            int id,
            CancellationToken cancellationToken = default)
        {
            var subscriptionPlan =
                await _subscriptionPlanRepository.GetOneAsync(
                    x => x.SubscriptionPlanId == id,
                    tracked: true,
                    cancellationToken: cancellationToken);

            if (subscriptionPlan == null)
                return NotFound();

            return View(subscriptionPlan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            SubscriptionPlanModel subscriptionPlan,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(subscriptionPlan);

            try
            {
                var oldSubscriptionPlan =
                    await _subscriptionPlanRepository.GetOneAsync(
                        x => x.SubscriptionPlanId == subscriptionPlan.SubscriptionPlanId,
                        tracked: true,
                        cancellationToken: cancellationToken);

                if (oldSubscriptionPlan == null)
                    return NotFound();

                oldSubscriptionPlan.Name = subscriptionPlan.Name;
                oldSubscriptionPlan.Tier = subscriptionPlan.Tier;
                oldSubscriptionPlan.Price = subscriptionPlan.Price;
                oldSubscriptionPlan.DurationInMonths = subscriptionPlan.DurationInMonths;
                oldSubscriptionPlan.MaxTeachers = subscriptionPlan.MaxTeachers;
                oldSubscriptionPlan.MaxStudents = subscriptionPlan.MaxStudents;
                oldSubscriptionPlan.Features = subscriptionPlan.Features;

                await _subscriptionPlanRepository.CommitAsync(cancellationToken);

                TempData["Success"] =
                    "Subscription plan updated successfully.";

                _logger.LogInformation(
                    "Subscription plan updated successfully. Id: {Id}",
                    subscriptionPlan.SubscriptionPlanId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating subscription plan. Id: {Id}",
                    subscriptionPlan.SubscriptionPlanId);

                TempData["Error"] =
                    "Something went wrong while updating the subscription plan.";

                return View(subscriptionPlan);
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
                var subscriptionPlan =
                    await _subscriptionPlanRepository.GetOneAsync(
                        x => x.SubscriptionPlanId == id,
                        cancellationToken: cancellationToken);

                if (subscriptionPlan == null)
                    return NotFound();

                _subscriptionPlanRepository.Delete(subscriptionPlan);

                await _subscriptionPlanRepository.CommitAsync(cancellationToken);

                TempData["Success"] =
                    "Subscription plan deleted successfully.";

                _logger.LogInformation(
                    "Subscription plan deleted successfully. Id: {Id}",
                    id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting subscription plan. Id: {Id}",
                    id);

                TempData["Error"] =
                    "Something went wrong while deleting the subscription plan.";

                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}