using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using TeacherModel = EduSphere.Models.Teacher;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class TeacherController : Controller
    {

        private readonly IRepository<TeacherModel> _context;

        public TeacherController(IRepository<TeacherModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Teachers = await _context.GetAsync(
                includes: new Expression<Func<TeacherModel, object>>[]
                {
                    s => s.User
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Teachers = Teachers.Where(e => (e.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Teachers.Count() / 10.0);
            Teachers = Teachers.Skip((page - 1) * 10).Take(10);

            return View(new TeachersVM()
            {
                Teachers = Teachers.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TeacherModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherModel Teacher, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Teacher);

            await _context.CreateAsync(Teacher, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Teacher added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Teacher = await _context.GetOneAsync(
                c => c.TeacherId == id,
                cancellationToken: cancellationToken);

            if (Teacher == null)
                return NotFound();

            return View(Teacher);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(TeacherModel Teacher, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Teacher);

            var oldTeacher = await _context.GetOneAsync(
                c => c.TeacherId == Teacher.TeacherId,
                includes: new Expression<Func<TeacherModel, object>>[]
                {
                    a => a.User,
                },
                
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldTeacher == null)
                return NotFound();

            oldTeacher.User.FullName = Teacher.User.FullName;
            oldTeacher.CenterId = Teacher.CenterId;
            oldTeacher.Specialization = Teacher.Specialization;
            oldTeacher.HireDate = Teacher.HireDate;
            oldTeacher.IsActive = Teacher.IsActive;
            

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Teacher updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Teacher = await _context.GetOneAsync(
                c => c.TeacherId == id,
                cancellationToken: cancellationToken);

            if (Teacher == null)
                return NotFound();

            _context.Delete(Teacher);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Teacher deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
