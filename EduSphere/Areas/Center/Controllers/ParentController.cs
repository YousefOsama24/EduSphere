using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
     [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class ParentController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<Parent> _parentRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly ILogger<ParentController> _logger;

        public ParentController(
            IRepository<Parent> parentRepository,
            IRepository<ApplicationUser> userRepository,
            ILogger<ParentController> logger)
        {
            _parentRepository = parentRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        #region Index
        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            // 1. بناء شرط الفلترة
            Expression<Func<Parent, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();
                filter = p => p.User != null && p.User.FullName.ToLower().Contains(search);
                ViewBag.Query = query;
            }

            // 2. سحب البيانات مع الـ Includes والـ Pagination من الداتابيز
            var parents = await _parentRepository.GetAsync(
                filter: filter,
                includes: new Expression<Func<Parent, object>>[]
                {
                    s => s.User!
                },
                skip: (page - 1) * PageSize,
                take: PageSize,
                tracked: false,
                cancellationToken: cancellationToken
            );

            // 3. حساب إجمالي عدد السجلات والصفحات
            int totalCount = await _parentRepository.CountAsync(
                predicate: filter,
                cancellationToken: cancellationToken
            );

            double totalPages = Math.Ceiling(totalCount / (double)PageSize);

            return View(new ParentsVM
            {
                Parents = parents,
                TotalPages = totalPages,
                CurrentPage = page
            });
        }
        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            await PopulateUsersDropDownListAsync(cancellationToken);
            return View(new Parent());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Parent parent, CancellationToken cancellationToken = default)
        {
            // استثناء الـ Navigations من الفاليديشن
            ModelState.Remove(nameof(parent.User));
            ModelState.Remove(nameof(parent.Students));
            ModelState.Remove(nameof(parent.ParentStudents));

            if (!ModelState.IsValid)
            {
                await PopulateUsersDropDownListAsync(cancellationToken, parent.UserId);
                return View(parent);
            }

            try
            {
                await _parentRepository.CreateAsync(parent, cancellationToken);
                await _parentRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "تم إضافة ولي الأمر بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating parent.");
                TempData["Error"] = "حدث خطأ أثناء إضافة ولي الأمر.";
                await PopulateUsersDropDownListAsync(cancellationToken, parent.UserId);
                return View(parent);
            }
        }
        #endregion

        #region Update
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var parent = await _parentRepository.GetOneAsync(
                a => a.ParentId == id,
                includes: new Expression<Func<Parent, object>>[]
                {
                    a => a.User!
                },
                cancellationToken: cancellationToken);

            if (parent == null)
                return NotFound();

            return View(parent);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Parent parent, CancellationToken cancellationToken = default)
        {
            ModelState.Remove(nameof(parent.User));
            ModelState.Remove(nameof(parent.Students));
            ModelState.Remove(nameof(parent.ParentStudents));

            if (!ModelState.IsValid)
                return View(parent);

            try
            {
                var oldParent = await _parentRepository.GetOneAsync(
                    a => a.ParentId == parent.ParentId,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (oldParent == null)
                    return NotFound();

                oldParent.Occupation = parent.Occupation;

                await _parentRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "تم تحديث بيانات ولي الأمر بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating parent. Id: {Id}", parent.ParentId);
                TempData["Error"] = "حدث خطأ أثناء تعديل بيانات ولي الأمر.";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var parent = await _parentRepository.GetOneAsync(
                    c => c.ParentId == id,
                    cancellationToken: cancellationToken);

                if (parent == null)
                    return NotFound();

                _parentRepository.Delete(parent);
                await _parentRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "تم حذف ولي الأمر بنجاح.";
                _logger.LogInformation("Parent deleted successfully. Id: {Id}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting parent. Id: {Id}", id);
                TempData["Error"] = "حدث خطأ أثناء حذف ولي الأمر.";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

        #region Helpers
        private async Task PopulateUsersDropDownListAsync(CancellationToken cancellationToken, string? selectedUserId = null)
        {
            var users = await _userRepository.GetAsync(cancellationToken: cancellationToken);
            ViewBag.Users = new SelectList(users, "Id", "FullName", selectedUserId);
        }
        #endregion
    }
}