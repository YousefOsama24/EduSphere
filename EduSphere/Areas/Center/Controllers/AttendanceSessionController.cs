using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class AttendanceSessionController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<AttendanceSession> _attendanceSessionRepository;
        private readonly ILogger<AttendanceSessionController> _logger;
        private readonly IRepository<EduSphere.Models.Teacher> _teacherRepository;
        private readonly IRepository<Group> _groupRepository;
        public AttendanceSessionController(
            IRepository<AttendanceSession> attendanceSessionRepository,
    IRepository<EduSphere.Models.Teacher> teacherRepository,
    IRepository<Group> groupRepository,
    ILogger<AttendanceSessionController> logger)
        {
            _attendanceSessionRepository = attendanceSessionRepository;
            _teacherRepository = teacherRepository;
            _groupRepository = groupRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var attendanceSessions =
                await _attendanceSessionRepository.GetAsync(
                    includes: new Expression<Func<AttendanceSession, object>>[]
                    {
                        x => x.Teacher
                    },
                    cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();

                attendanceSessions = attendanceSessions.Where(x =>
                    x.Teacher.User.FullName.ToLower().Contains(search));

                ViewBag.Query = query;
            }

            double totalPages =
                Math.Ceiling(attendanceSessions.Count() / (double)PageSize);

            attendanceSessions = attendanceSessions
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new AttendanceSessionsVM
            {
                AttendanceSessions = attendanceSessions.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }
        [HttpGet]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Create(
            CancellationToken cancellationToken = default)
        {
            var teachers = await _teacherRepository.GetAsync(
                cancellationToken: cancellationToken);

            var groups = await _groupRepository.GetAsync(
                cancellationToken: cancellationToken);

            ViewBag.Teachers = new SelectList(
                teachers,
                "TeacherId",
                "TeacherId");

            ViewBag.Groups = new SelectList(
                groups,
                "GroupId",
                "Name");

            return View(new AttendanceSession());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Create(
     AttendanceSession attendanceSession,
     CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var teachers = await _teacherRepository.GetAsync(
                    cancellationToken: cancellationToken);

                var groups = await _groupRepository.GetAsync(
                    cancellationToken: cancellationToken);

                ViewBag.Teachers = new SelectList(
                    teachers,
                    "TeacherId",
                    "TeacherId",
                    attendanceSession.TeacherId);

                ViewBag.Groups = new SelectList(
                    groups,
                    "GroupId",
                    "Name",
                    attendanceSession.GroupId);

                return View(attendanceSession);
            }

            try
            {
                await _attendanceSessionRepository.CreateAsync(
                    attendanceSession,
                    cancellationToken);

                await _attendanceSessionRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Attendance session created successfully.";

                _logger.LogInformation(
                    "Attendance Session created successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating attendance session.");

                TempData["Error"] =
                    "Something went wrong while creating the attendance session.";

                var teachers = await _teacherRepository.GetAsync(
                    cancellationToken: cancellationToken);

                var groups = await _groupRepository.GetAsync(
                    cancellationToken: cancellationToken);

                ViewBag.Teachers = new SelectList(
                    teachers,
                    "TeacherId",
                    "TeacherId",
                    attendanceSession.TeacherId);

                ViewBag.Groups = new SelectList(
                    groups,
                    "GroupId",
                    "Name",
                    attendanceSession.GroupId);

                return View(attendanceSession);
            }
        }

        [HttpGet]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Update(
    int id,
    CancellationToken cancellationToken = default)
        {
            var attendanceSession =
                await _attendanceSessionRepository.GetOneAsync(
                    x => x.AttendanceSessionId == id,
                    tracked: true,
                    cancellationToken: cancellationToken);

            if (attendanceSession == null)
                return NotFound();

            var teachers =
                await _teacherRepository.GetAsync(
                    cancellationToken: cancellationToken);

            var groups =
                await _groupRepository.GetAsync(
                    cancellationToken: cancellationToken);

            ViewBag.Teachers = new SelectList(
                teachers,
                "TeacherId",
                "TeacherId",
                attendanceSession.TeacherId);

            ViewBag.Groups = new SelectList(
                groups,
                "GroupId",
                "Name",
                attendanceSession.GroupId);

            return View(attendanceSession);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Update(
    AttendanceSession attendanceSession,
    CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var teachers =
                    await _teacherRepository.GetAsync(
                        cancellationToken: cancellationToken);

                var groups =
                    await _groupRepository.GetAsync(
                        cancellationToken: cancellationToken);

                ViewBag.Teachers = new SelectList(
                    teachers,
                    "TeacherId",
                    "TeacherId",
                    attendanceSession.TeacherId);

                ViewBag.Groups = new SelectList(
                    groups,
                    "GroupId",
                    "Name",
                    attendanceSession.GroupId);

                return View(attendanceSession);
            }

            try
            {
                var oldSession =
                    await _attendanceSessionRepository.GetOneAsync(
                        x => x.AttendanceSessionId ==
                             attendanceSession.AttendanceSessionId,
                        tracked: true,
                        cancellationToken: cancellationToken);

                if (oldSession == null)
                    return NotFound();

                oldSession.Title = attendanceSession.Title;
                oldSession.TeacherId = attendanceSession.TeacherId;
                oldSession.GroupId = attendanceSession.GroupId;
                oldSession.SessionDate = attendanceSession.SessionDate;

                await _attendanceSessionRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Attendance session updated successfully.";

                _logger.LogInformation(
                    "Attendance Session updated successfully. Id: {Id}",
                    attendanceSession.AttendanceSessionId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating attendance session.");

                TempData["Error"] =
                    "Something went wrong while updating the attendance session.";

                var teachers =
                    await _teacherRepository.GetAsync(
                        cancellationToken: cancellationToken);

                var groups =
                    await _groupRepository.GetAsync(
                        cancellationToken: cancellationToken);

                ViewBag.Teachers = new SelectList(
                    teachers,
                    "TeacherId",
                    "TeacherId",
                    attendanceSession.TeacherId);

                ViewBag.Groups = new SelectList(
                    groups,
                    "GroupId",
                    "Name",
                    attendanceSession.GroupId);

                return View(attendanceSession);
            }
        }
        [HttpGet]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Details(
    int id,
    CancellationToken cancellationToken = default)
        {
            var attendanceSession =
                await _attendanceSessionRepository.GetOneAsync(
                    x => x.AttendanceSessionId == id,
                    includes: new Expression<Func<AttendanceSession, object>>[]
                    {
                x => x.Teacher,
                x => x.Group,
                x => x.AttendanceRecords
                    },
                    cancellationToken: cancellationToken);

            if (attendanceSession == null)
            {
                _logger.LogWarning(
                    "Attendance Session not found. Id: {Id}",
                    id);

                return NotFound();
            }

            return View(attendanceSession);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CenterManager,SuperAdmin")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var attendanceSession =
                    await _attendanceSessionRepository.GetOneAsync(
                        x => x.AttendanceSessionId == id,
                        cancellationToken: cancellationToken);

                if (attendanceSession == null)
                    return NotFound();

                _attendanceSessionRepository.Delete(attendanceSession);

                await _attendanceSessionRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Attendance session deleted successfully.";

                _logger.LogInformation(
                    "Attendance Session deleted successfully. Id: {Id}",
                    id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting attendance session. Id: {Id}",
                    id);

                TempData["Error"] =
                    "Something went wrong while deleting the attendance session.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}