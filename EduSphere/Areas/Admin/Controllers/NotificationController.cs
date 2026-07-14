using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using NotificationModel = EduSphere.Models.Notification;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class NotificationController : Controller
    {

        private readonly IRepository<NotificationModel> _context;

        public NotificationController(IRepository<NotificationModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Notifications = await _context.GetAsync(
                includes: new Expression<Func<NotificationModel, object>>[]
                {
                    s => s.User
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Notifications = Notifications.Where(e => (e.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Notifications.Count() / 3.0);
            Notifications = Notifications.Skip((page - 1) * 3).Take(3);

            return View(new NotificationsVM()
            {
                Notifications = Notifications.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new NotificationModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotificationModel notification, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(notification);

            notification.CreatedAt = DateTime.Now;
            notification.IsRead = false;

            await _context.CreateAsync(notification, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success"] = "Notification created successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Notification = await _context.GetOneAsync(
                a => a.NotificationId == id,
                includes: new Expression<Func<NotificationModel, object>>[]
                {
                    a => a.User,
                   
                },
                cancellationToken: cancellationToken);

            if (Notification == null)
                return NotFound();

            return View(Notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(NotificationModel Notification, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Notification);

            var oldNotification = await _context.GetOneAsync(
                a => a.NotificationId == Notification.NotificationId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldNotification == null)
                return NotFound();

            oldNotification.Title = Notification.Title;
            oldNotification.Message = Notification.Message;
            oldNotification.Type = Notification.Type;
            oldNotification.IsRead = Notification.IsRead;
            oldNotification.CreatedAt = Notification.CreatedAt;
           
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Attendance Record updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Notification = await _context.GetOneAsync(
                c => c.NotificationId == id,
                cancellationToken: cancellationToken);

            if (Notification == null)
                return NotFound();

            _context.Delete(Notification);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Notification deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
