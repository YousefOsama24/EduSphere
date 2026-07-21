using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using EduSphere.ViewModel;
// استخدام الـ Alias بوضوح
using GroupModel = EduSphere.Models.Group;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    public class GroupController : Controller
    {
        private readonly IRepository<GroupModel> _context;
        // لو عندك Repositories للكورسات والمدرسين، هتحتاج تعملهم Inject هنا عشان الـ Dropdowns

        public GroupController(IRepository<GroupModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            // ملحوظة: يفضل تعدل GetAsync في الـ Repository عشان تستقبل skip و take
            // بدل ما تجيب الداتا كلها للميموري
            var Groups = await _context.GetAsync(
                includes: new Expression<Func<GroupModel, object>>[]
                {
                    s => s.Course
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Groups = Groups.Where(e => (e.Name).ToLower().Contains(q));
                ViewBag.Query = query;
            }

            double totalPages = System.Math.Ceiling(Groups.Count() / 10.0);
            Groups = Groups.Skip((page - 1) * 10).Take(10);

            return View(new GroupsVM()
            {
                Groups = Groups.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            // TODO: Populate ViewBag.Courses and ViewBag.Teachers for the dropdowns
            return View(new GroupModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupModel Group, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                // TODO: Repopulate ViewBag.Courses and ViewBag.Teachers here before returning View
                return View(Group);
            }

            await _context.CreateAsync(Group, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Group added successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Group = await _context.GetOneAsync(
                c => c.GroupId == id,
                cancellationToken: cancellationToken);

            if (Group == null)
                return NotFound();

            // TODO: Populate ViewBag.Courses and ViewBag.Teachers for the dropdowns
            return View(Group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // تم تعديل Group لـ GroupModel عشان نتفادى أي تعارض في الأسماء
        public async Task<IActionResult> Update(GroupModel Group, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                // TODO: Repopulate ViewBag.Courses and ViewBag.Teachers here before returning View
                return View(Group);
            }

            var oldGroup = await _context.GetOneAsync(
                c => c.GroupId == Group.GroupId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldGroup == null)
                return NotFound();

            oldGroup.Name = Group.Name;
            oldGroup.CourseId = Group.CourseId;
            oldGroup.TeacherId = Group.TeacherId;
            oldGroup.Capacity = Group.Capacity;
            oldGroup.StartDate = Group.StartDate;
            oldGroup.EndDate = Group.EndDate;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Group updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        // لو حابب تحمي الـ Delete أكتر ممكن تضيفلها [ValidateAntiForgeryToken] 
        // وتستخدم Form في الـ View بدل زرار عادي
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Group = await _context.GetOneAsync(
                c => c.GroupId == id,
                cancellationToken: cancellationToken);

            if (Group == null)
                return NotFound();

            _context.Delete(Group);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Group deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}