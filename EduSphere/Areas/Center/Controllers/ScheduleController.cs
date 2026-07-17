using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class ScheduleController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<Schedule> _scheduleRepository;

        public ScheduleController(IRepository<Schedule> scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var schedules = await _scheduleRepository.GetAsync(
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim();

                schedules = schedules.Where(x =>
                    x.GroupId.ToString().Contains(search) ||
                    x.Room.Contains(search, StringComparison.OrdinalIgnoreCase));

                ViewBag.Query = query;
            }

            double totalPages =
                Math.Ceiling(schedules.Count() / (double)PageSize);

            schedules = schedules
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new SchedulesVM
            {
                Schedules = schedules,
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Schedule());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Schedule schedule,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(schedule);

            await _scheduleRepository.CreateAsync(
                schedule,
                cancellationToken);

            await _scheduleRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Schedule created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(
            int id,
            CancellationToken cancellationToken = default)
        {
            var schedule = await _scheduleRepository.GetOneAsync(
                x => x.ScheduleId == id,
                cancellationToken: cancellationToken);

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Schedule schedule,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(schedule);

            var oldSchedule = await _scheduleRepository.GetOneAsync(
                x => x.ScheduleId == schedule.ScheduleId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldSchedule == null)
                return NotFound();

            oldSchedule.GroupId = schedule.GroupId;
            oldSchedule.Day = schedule.Day;
            oldSchedule.StartTime = schedule.StartTime;
            oldSchedule.EndTime = schedule.EndTime;
            oldSchedule.Room = schedule.Room;

            await _scheduleRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Schedule updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            var schedule = await _scheduleRepository.GetOneAsync(
                x => x.ScheduleId == id,
                cancellationToken: cancellationToken);

            if (schedule == null)
                return NotFound();

            _scheduleRepository.Delete(schedule);

            await _scheduleRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Schedule deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
