using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class AttendanceRecordController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<AttendanceRecord> _attendanceRecordRepository;
        private readonly ILogger<AttendanceRecordController> _logger;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<AttendanceSession> _attendanceSessionRepository;

        public AttendanceRecordController(
            IRepository<AttendanceRecord> attendanceRecordRepository,
            ILogger<AttendanceRecordController> logger,
            IRepository<Student> studentRepository,
IRepository<AttendanceSession> attendanceSessionRepository)
        {
            _attendanceRecordRepository = attendanceRecordRepository;
            _logger = logger;
            _studentRepository = studentRepository;
            _attendanceSessionRepository = attendanceSessionRepository;

        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var attendanceRecords =
                await _attendanceRecordRepository.GetAsync(
                    includes: new Expression<Func<AttendanceRecord, object>>[]
                    {
                        x => x.Student
                    },
                    cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();

                attendanceRecords = attendanceRecords.Where(x =>
                    x.Student.User.FullName.ToLower().Contains(search));

                ViewBag.Query = query;
            }

            double totalPages =
                Math.Ceiling(attendanceRecords.Count() / (double)PageSize);

            attendanceRecords = attendanceRecords
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new AttendanceRecordsVM
            {
                AttendanceRecords = attendanceRecords.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create(
    CancellationToken cancellationToken = default)
        {
            var students =
                await _studentRepository.GetAsync(
                    cancellationToken: cancellationToken);

            var sessions =
                await _attendanceSessionRepository.GetAsync(
                    cancellationToken: cancellationToken);

            ViewBag.Students = new SelectList(
                students,
                "StudentId",
                "StudentId");

            ViewBag.Sessions = new SelectList(
                sessions,
                "AttendanceSessionId",
                "Title");

            ViewBag.Statuses =
                Enum.GetValues(typeof(AttendanceStatus));

            return View(new AttendanceRecord());
        
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    AttendanceRecord attendanceRecord,
    CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var students = await _studentRepository.GetAsync(
                    cancellationToken: cancellationToken);

                var sessions = await _attendanceSessionRepository.GetAsync(
                    cancellationToken: cancellationToken);

                ViewBag.Students = new SelectList(
                    students,
                    "StudentId",
                    "StudentId",
                    attendanceRecord.StudentId);

                ViewBag.Sessions = new SelectList(
                    sessions,
                    "AttendanceSessionId",
                    "Title",
                    attendanceRecord.AttendanceSessionId);

                ViewBag.Statuses =
                    Enum.GetValues(typeof(AttendanceStatus));

                return View(attendanceRecord);
            }

            try
            {
                await _attendanceRecordRepository.CreateAsync(
                    attendanceRecord,
                    cancellationToken);

                await _attendanceRecordRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Attendance record created successfully.";

                _logger.LogInformation(
                    "Attendance Record created successfully.");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating attendance record.");

                TempData["Error"] =
                    "Something went wrong while creating the attendance record.";

                var students = await _studentRepository.GetAsync(
                    cancellationToken: cancellationToken);

                var sessions = await _attendanceSessionRepository.GetAsync(
                    cancellationToken: cancellationToken);

                ViewBag.Students = new SelectList(
                    students,
                    "StudentId",
                    "StudentId",
                    attendanceRecord.StudentId);

                ViewBag.Sessions = new SelectList(
                    sessions,
                    "AttendanceSessionId",
                    "Title",
                    attendanceRecord.AttendanceSessionId);

                ViewBag.Statuses =
                    Enum.GetValues(typeof(AttendanceStatus));

                return View(attendanceRecord);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(
    int id,
    CancellationToken cancellationToken = default)
        {
            var attendanceRecord =
                await _attendanceRecordRepository.GetOneAsync(
                    x => x.AttendanceRecordId == id,
                    includes: new Expression<Func<AttendanceRecord, object>>[]
                    {
                x => x.Student,
                x => x.AttendanceSession
                    },
                    cancellationToken: cancellationToken);

            if (attendanceRecord == null)
            {
                _logger.LogWarning(
                    "Attendance Record not found. Id: {Id}",
                    id);

                return NotFound();
            }

            return View(attendanceRecord);
        }

        [HttpGet]
        public async Task<IActionResult> Update(
    int id,
    CancellationToken cancellationToken = default)
        {
            var attendanceRecord =
                await _attendanceRecordRepository.GetOneAsync(
                    x => x.AttendanceRecordId == id,
                    tracked: true,
                    cancellationToken: cancellationToken);

            if (attendanceRecord == null)
                return NotFound();

            var students =
                await _studentRepository.GetAsync(
                    cancellationToken: cancellationToken);

            var sessions =
                await _attendanceSessionRepository.GetAsync(
                    cancellationToken: cancellationToken);

            ViewBag.Students = new SelectList(
                students,
                "StudentId",
                "StudentId",
                attendanceRecord.StudentId);

            ViewBag.Sessions = new SelectList(
                sessions,
                "AttendanceSessionId",
                "Title",
                attendanceRecord.AttendanceSessionId);

            ViewBag.Statuses =
                Enum.GetValues(typeof(AttendanceStatus));

            return View(attendanceRecord);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
    AttendanceRecord attendanceRecord,
    CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var students =
                    await _studentRepository.GetAsync(
                        cancellationToken: cancellationToken);

                var sessions =
                    await _attendanceSessionRepository.GetAsync(
                        cancellationToken: cancellationToken);

                ViewBag.Students = new SelectList(
                    students,
                    "StudentId",
                    "StudentId",
                    attendanceRecord.StudentId);

                ViewBag.Sessions = new SelectList(
                    sessions,
                    "AttendanceSessionId",
                    "Title",
                    attendanceRecord.AttendanceSessionId);

                ViewBag.Statuses =
                    Enum.GetValues(typeof(AttendanceStatus));

                return View(attendanceRecord);
            }

            try
            {
                var oldAttendance =
                    await _attendanceRecordRepository.GetOneAsync(
                        x => x.AttendanceRecordId ==
                             attendanceRecord.AttendanceRecordId,
                        tracked: true,
                        cancellationToken: cancellationToken);

                if (oldAttendance == null)
                    return NotFound();

                oldAttendance.StudentId =
                    attendanceRecord.StudentId;

                oldAttendance.AttendanceSessionId =
                    attendanceRecord.AttendanceSessionId;

                oldAttendance.Status =
                    attendanceRecord.Status;

                oldAttendance.Notes =
                    attendanceRecord.Notes;

                await _attendanceRecordRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Attendance record updated successfully.";

                _logger.LogInformation(
                    "Attendance Record updated successfully. Id: {Id}",
                    attendanceRecord.AttendanceRecordId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating attendance record.");

                TempData["Error"] =
                    "Something went wrong while updating the attendance record.";

                return View(attendanceRecord);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var attendanceRecord =
                    await _attendanceRecordRepository.GetOneAsync(
                        x => x.AttendanceRecordId == id,
                        cancellationToken: cancellationToken);

                if (attendanceRecord == null)
                    return NotFound();

                _attendanceRecordRepository.Delete(attendanceRecord);

                await _attendanceRecordRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Attendance record deleted successfully.";

                _logger.LogInformation(
                    "Attendance Record deleted successfully. Id: {Id}",
                    id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting attendance record. Id: {Id}",
                    id);

                TempData["Error"] =
                    "Something went wrong while deleting the attendance record.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}