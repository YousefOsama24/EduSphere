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


namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class AttendanceRecordController : Controller
    {
        private readonly IRepository<AttendanceRecord> _attendanceRecordRepository;

        public AttendanceRecordController(IRepository<AttendanceRecord> attendanceRecordRepository)
        {
            _attendanceRecordRepository = attendanceRecordRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var AttendanceRecords = await _attendanceRecordRepository.GetAsync(
                includes: new Expression<Func<AttendanceRecord, object>>[]
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
            return View(new AttendanceRecord());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AttendanceRecord AttendanceRecord, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(AttendanceRecord);



            await _attendanceRecordRepository.CreateAsync(AttendanceRecord, cancellationToken);
            await _attendanceRecordRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add AttendanceRecord Successfully";

            return RedirectToAction(nameof(Index));
        }
        // Controller GET: return a tracked entity
        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var AttendanceRecord = await _attendanceRecordRepository.GetOneAsync(
                c => c.AttendanceRecordId == id,
                cancellationToken: cancellationToken);

            if (AttendanceRecord == null)
                return NotFound();

            _attendanceRecordRepository.Delete(AttendanceRecord);
            await _attendanceRecordRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "AttendanceRecord deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
