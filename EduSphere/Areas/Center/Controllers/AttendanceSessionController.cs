using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;


namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class AttendanceSessionController : Controller
    {

        private readonly IRepository<AttendanceSession> _attendanceSessionRepository;

        public AttendanceSessionController(IRepository<AttendanceSession> attendanceSessionRepository)
        {
            _attendanceSessionRepository = attendanceSessionRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var AttendanceSessions = await _attendanceSessionRepository.GetAsync(
                includes: new Expression<Func<AttendanceSession, object>>[]
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
            return View(new AttendanceSession());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AttendanceSession AttendanceSession, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(AttendanceSession);



            await _attendanceSessionRepository.CreateAsync(AttendanceSession, cancellationToken);
            await _attendanceSessionRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add AttendanceSession Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Update()
        {
            return View(new AttendanceSession());
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var AttendanceSession = await _attendanceSessionRepository.GetOneAsync(
                c => c.AttendanceSessionId == id,
                cancellationToken: cancellationToken);

            if (AttendanceSession == null)
                return NotFound();

            _attendanceSessionRepository.Delete(AttendanceSession);
            await _attendanceSessionRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "AttendanceSession deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
