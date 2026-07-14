using EduSphere.Data;
using EduSphere.Models;
using EduSphere.Repositories;
using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceRecordModel = EduSphere.Models.AttendanceRecord;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class AttendanceRecordController : Controller
    {
        private readonly IRepository<AttendanceRecordModel> _context;

        public AttendanceRecordController(IRepository<AttendanceRecordModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var AttendanceRecords = await _context.GetAsync(
                includes: new Expression<Func<AttendanceRecordModel, object>>[]
                {
                    s => s.Student
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                AttendanceRecords = AttendanceRecords.Where(e => (e.Student.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(AttendanceRecords.Count() / 3.0);
            AttendanceRecords = AttendanceRecords.Skip((page - 1) * 3).Take(3);

            return View(new AttendanceRecordsVM()
            {
                AttendanceRecords = AttendanceRecords.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AttendanceRecordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttendanceRecordModel AttendanceRecord, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(AttendanceRecord);

            await _context.CreateAsync(AttendanceRecord, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "AttendanceRecord added successfully.";

            return RedirectToAction(nameof(Index));
        }
        // Controller GET: return a tracked entity
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var attendanceRecord = await _context.GetOneAsync(
                a => a.AttendanceRecordId == id,
                includes: new Expression<Func<AttendanceRecordModel, object>>[]
                {
                    a => a.Student,
                    a => a.AttendanceSession
                },
                cancellationToken: cancellationToken);

            if (attendanceRecord == null)
                return NotFound();

            return View(attendanceRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AttendanceRecordModel attendanceRecord, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(attendanceRecord);

            var oldAttendanceRecord = await _context.GetOneAsync(
                a => a.AttendanceRecordId == attendanceRecord.AttendanceRecordId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldAttendanceRecord == null)
                return NotFound();

            oldAttendanceRecord.AttendanceSessionId = attendanceRecord.AttendanceSessionId;
            oldAttendanceRecord.StudentId = attendanceRecord.StudentId;
            oldAttendanceRecord.Status = attendanceRecord.Status;
            oldAttendanceRecord.Notes = attendanceRecord.Notes;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Attendance Record updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var AttendanceRecord = await _context.GetOneAsync(
                c => c.AttendanceRecordId == id,
                cancellationToken: cancellationToken);

            if (AttendanceRecord == null)
                return NotFound();

            _context.Delete(AttendanceRecord);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "AttendanceRecord deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
