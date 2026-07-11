using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using AttendanceSessionModel = EduSphere.Models.AttendanceSession;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class AttendanceSessionController : Controller
    {

        private readonly IRepository<AttendanceSessionModel> _context;

        public AttendanceSessionController(IRepository<AttendanceSessionModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var AttendanceSessions = await _context.GetAsync(
                includes: new Expression<Func<AttendanceSessionModel, object>>[]
                {
                    s => s.Teacher
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                AttendanceSessions = AttendanceSessions.Where(e => (e.Teacher.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(AttendanceSessions.Count() / 3.0);
            AttendanceSessions = AttendanceSessions.Skip((page - 1) * 3).Take(3);

            return View(new AttendanceSessionsVM()
            {
                AttendanceSessions = AttendanceSessions.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AttendanceSessionModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AttendanceSessionModel AttendanceSession, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(AttendanceSession);



            await _context.CreateAsync(AttendanceSession, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add AttendanceSession Successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var AttendanceSession = await _context.GetOneAsync(
                c => c.AttendanceSessionId == id,
                cancellationToken: cancellationToken);

            if (AttendanceSession == null)
                return NotFound();

            _context.Delete(AttendanceSession);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "AttendanceSession deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
