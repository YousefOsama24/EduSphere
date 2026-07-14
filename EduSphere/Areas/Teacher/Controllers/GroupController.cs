using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using GroupModel = EduSphere.Models.Group;

namespace EduSphere.Areas.Teacher.Controllers
{
    [Area(SD.TEACHER_AREA)]
    public class GroupController : Controller
    {

        private readonly IRepository<GroupModel> _context;

        public GroupController(IRepository<GroupModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
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
            return View(new GroupModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupModel Group, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Group);

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

            return View(Group);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Group Group, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Group);

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
