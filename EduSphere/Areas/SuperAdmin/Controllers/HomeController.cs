using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Repositories;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc;
using TeacherModel = EduSphere.Models.Teacher;
using CenterModel = EduSphere.Models.Center;
namespace EduSphere.Areas.SuperAdmin.Controllers
{
    [Area(SD.SuperAdmin_AREA)]
    public class HomeController : Controller
    {
        private readonly IRepository<TeacherModel> _teacherRepo;
        private readonly IRepository<CenterModel> _centerRepo;
        // ضيف الـ Repositories الثانية الخاصة بـ Payments و Student حسب الموديلز عندك

        public HomeController(
            IRepository<TeacherModel> teacherRepo,
            IRepository<CenterModel> centerRepo)
        {
            _teacherRepo = teacherRepo;
            _centerRepo = centerRepo;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var teachers = await _teacherRepo.GetAsync(cancellationToken: cancellationToken);
            var centers = await _centerRepo.GetAsync(cancellationToken: cancellationToken);

            var viewModel = new SuperAdminDashboardVM
            {
                TotalTeachers = teachers.Count(),
                TotalCenters = centers.Count(),
                TotalStudents = 1250, // استبدلها بـ _studentRepo.GetAsync().Count()
                TotalRevenue = 45800.50m, // استبدلها بمجموع المبالغ من جدول الـ Payments

                // عينات تجريبية لعمليات الدفع والإشعارات لحين ربطها بجداولها
                RecentPayments = new List<PaymentItemVM>
                {
                    new PaymentItemVM { PaymentId = 101, UserOrCenterName = "سنتر النخبة", Amount = 1500, Date = DateTime.Now.AddHours(-2), Status = "Success" },
                    new PaymentItemVM { PaymentId = 102, UserOrCenterName = "أ. أحمد محمود", Amount = 400, Date = DateTime.Now.AddHours(-5), Status = "Success" },
                    new PaymentItemVM { PaymentId = 103, UserOrCenterName = "سنتر التفوق", Amount = 2100, Date = DateTime.Now.AddDays(-1), Status = "Pending" }
                },
                Notifications = new List<NotificationItemVM>
                {
                    new NotificationItemVM { Title = "تسجيل سنتر جديد", Message = "تم طلب انضمام 'سنتر الأمل' للمنظومة.", CreatedAt = DateTime.Now.AddMinutes(-30), Type = "info" },
                    new NotificationItemVM { Title = "عملية دفع ناجحة", Message = "تم استلام مبلغ 1500 ج.م من سنتر النخبة.", CreatedAt = DateTime.Now.AddHours(-2), Type = "success" },
                    new NotificationItemVM { Title = "تنبيه نظام", Message = "تم الوصول لـ 80% من سعة السيرفر الرئيسية.", CreatedAt = DateTime.Now.AddHours(-12), Type = "warning" }
                }
            };

            return View(viewModel);
        }
    }
}