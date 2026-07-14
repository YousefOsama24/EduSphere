using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using ScheduleModel = EduSphere.Models.Schedule;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class ScheduleController : Controller
    {

        private readonly IRepository<ScheduleModel> _context;

        public ScheduleController(IRepository<ScheduleModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Schedules = await _context.GetAsync(
                includes: new Expression<Func<ScheduleModel, object>>[]
                {
                    s => s.Group
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Schedules = Schedules.Where(e => (e.Room).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Schedules.Count() / 3.0);
            Schedules = Schedules.Skip((page - 1) * 3).Take(3);

            return View(new SchedulesVM()
            {
                Schedules = Schedules.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ScheduleModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleModel Schedule, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Schedule);

            await _context.CreateAsync(Schedule, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Schedule added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Schedule = await _context.GetOneAsync(
                c => c.ScheduleId == id,
                cancellationToken: cancellationToken);

            if (Schedule == null)
                return NotFound();

            return View(Schedule);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Schedule Schedule, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Schedule);

            var oldSchedule = await _context.GetOneAsync(
                c => c.ScheduleId == Schedule.ScheduleId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldSchedule == null)
                return NotFound();

            oldSchedule.GroupId = Schedule.GroupId;
            oldSchedule.Day = Schedule.Day;
            oldSchedule.Room = Schedule.Room;
            oldSchedule.StartTime = Schedule.StartTime;
            oldSchedule.EndTime = Schedule.EndTime;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Schedule updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Schedule = await _context.GetOneAsync(
                c => c.ScheduleId == id,
                cancellationToken: cancellationToken);

            if (Schedule == null)
                return NotFound();

            _context.Delete(Schedule);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Schedule deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
