using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EduSphere.ViewModel;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class ParentController : Controller
    {
        private const int PageSize = 3;

        private readonly IRepository<Parent> _context;
        private readonly ILogger<ParentController> _logger;

        public ParentController(IRepository<Parent> context, ILogger<ParentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var parents = await _context.GetAsync(
                includes: new Expression<Func<Parent, object>>[]
                {
                    s => s.User
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                parents = parents.Where(e => (e.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }

            double totalPages = Math.Ceiling(parents.Count() / (double)PageSize);
            parents = parents.Skip((page - 1) * PageSize).Take(PageSize);

            return View(new ParentsVM()
            {
                Parents = parents.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }

        [HttpGet]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public IActionResult Create()
        {
            return View(new Parent());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Create(Parent parent, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(parent);

            try
            {
                await _context.CreateAsync(parent, cancellationToken);
                await _context.CommitAsync(cancellationToken);

                TempData["Success"] = "Add Parent Successfully";
                _logger.LogInformation("Parent created successfully. Id: {Id}", parent.ParentId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating parent.");
                TempData["Error"] = "Something went wrong while creating the parent.";
                return View(parent);
            }
        }

        [HttpGet]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var parent = await _context.GetOneAsync(
                a => a.ParentId == id,
                includes: new Expression<Func<Parent, object>>[]
                {
                    a => a.User,

                },
                cancellationToken: cancellationToken);

            if (parent == null)
                return NotFound();

            return View(parent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Update(Parent parent, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(parent);

            try
            {
                var oldParent = await _context.GetOneAsync(
                    a => a.ParentId == parent.ParentId,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (oldParent == null)
                    return NotFound();

                oldParent.Occupation = parent.Occupation;

                await _context.CommitAsync(cancellationToken);

                TempData["Success"] = "Parent updated successfully.";

                _logger.LogInformation("Parent updated successfully. Id: {Id}", parent.ParentId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating parent. Id: {Id}", parent.ParentId);
                TempData["Error"] = "Something went wrong while updating the parent.";
                return View(parent);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var parent = await _context.GetOneAsync(
                    c => c.ParentId == id,
                    cancellationToken: cancellationToken);

                if (parent == null)
                    return NotFound();

                _context.Delete(parent);
                await _context.CommitAsync(cancellationToken);

                TempData["Success"] = "Parent deleted successfully.";
                _logger.LogInformation("Parent deleted successfully. Id: {Id}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting parent. Id: {Id}", id);
                TempData["Error"] = "Something went wrong while deleting the parent.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
