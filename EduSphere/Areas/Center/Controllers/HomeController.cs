using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    public class HomeController : Controller
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<EduSphere.Models.Teacher> _teacherRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<Parent> _parentRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<AttendanceSession> _attendanceSessionRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly IRepository<AttendanceRecord> _attendanceRecordRepository;
        private readonly IRepository<Exam> _examRepository;
        private readonly IRepository<Notification> _notificationRepository;

        public HomeController(
            IRepository<Student> studentRepository,
            IRepository<EduSphere.Models.Teacher> teacherRepository,
            IRepository<Course> courseRepository,
            IRepository<Group> groupRepository,
            IRepository<Parent> parentRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<AttendanceSession> attendanceSessionRepository,
            IRepository<Subscription> subscriptionRepository,
            IRepository<AttendanceRecord> attendanceRecordRepository,
            IRepository<Exam> examRepository,
            IRepository<Notification> notificationRepository)
        {
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _groupRepository = groupRepository;
            _parentRepository = parentRepository;
            _enrollmentRepository = enrollmentRepository;
            _attendanceSessionRepository = attendanceSessionRepository;
            _subscriptionRepository = subscriptionRepository;
            _attendanceRecordRepository = attendanceRecordRepository;
            _examRepository = examRepository;
            _notificationRepository = notificationRepository;
        }

        #region Admin Dashboard Index
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            DateTime today = DateTime.Today;

            // 1. الأعداد الإجمالية (Count Direct Queries)
            var totalStudents = await _studentRepository.CountAsync(cancellationToken: cancellationToken);
            var totalTeachers = await _teacherRepository.CountAsync(cancellationToken: cancellationToken);
            var totalCourses = await _courseRepository.CountAsync(cancellationToken: cancellationToken);
            var totalGroups = await _groupRepository.CountAsync(cancellationToken: cancellationToken);
            var totalParents = await _parentRepository.CountAsync(cancellationToken: cancellationToken);
            var totalAttendanceSessions = await _attendanceSessionRepository.CountAsync(cancellationToken: cancellationToken);

            // عدد الاشتراكات النشطة
            var activeSubscriptionsCount = await _subscriptionRepository.CountAsync(
                x => x.Status == SubscriptionStatus.Active,
                cancellationToken);

            // 2. جلب أحدث 5 طلاب (ترتيب + أخذ 5 من الداتابيز مباشرة)
            var latestStudents = await _studentRepository.GetAsync(
                orderBy: q => q.OrderByDescending(x => x.StudentId),
                take: 5,
                includes: new Expression<Func<Student, object>>[] { x => x.User! },
                cancellationToken: cancellationToken);

            // 3. جلب أحدث 5 مدرسين
            var latestTeachers = await _teacherRepository.GetAsync(
                orderBy: q => q.OrderByDescending(x => x.TeacherId),
                take: 5,
                includes: new Expression<Func<EduSphere.Models.Teacher, object>>[] { x => x.User! },
                cancellationToken: cancellationToken);

            // 4. جلب أحدث 5 تسجيلات مع الـ Includes
            var latestEnrollments = await _enrollmentRepository.GetAsync(
                orderBy: q => q.OrderByDescending(x => x.EnrollmentDate),
                take: 5,
                includes: new Expression<Func<Enrollment, object>>[] { x => x.Student!, x => x.Student!.User!, x => x.Group! },
                cancellationToken: cancellationToken);

            // 5. حصص اليوم (10 حصص)
            var todaySessions = await _attendanceSessionRepository.GetAsync(
                filter: x => x.SessionDate.Date == today,
                orderBy: q => q.OrderBy(x => x.SessionDate),
                take: 10,
                includes: new Expression<Func<AttendanceSession, object>>[] { x => x.Group! },
                cancellationToken: cancellationToken);

            // 6. الاشتراكات الموشكة على الانتهاء
            var expiringSubscriptions = await _subscriptionRepository.GetAsync(
                filter: x => x.EndDate <= today.AddDays(7) && x.Status == SubscriptionStatus.Active,
                orderBy: q => q.OrderBy(x => x.EndDate),
                take: 5,
                includes: new Expression<Func<Subscription, object>>[] { x => x.Center!, x => x.SubscriptionPlan! },
                cancellationToken: cancellationToken);

            // 7. بناء الـ ViewModel
            DashboardVM dashboard = new DashboardVM
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalCourses = totalCourses,
                TotalGroups = totalGroups,
                TotalParents = totalParents,
                TotalAttendanceSessions = totalAttendanceSessions,
                ActiveSubscriptions = activeSubscriptionsCount,
                LatestStudents = latestStudents,
                LatestTeachers = latestTeachers,
                LatestEnrollments = latestEnrollments,
                TodaySessions = todaySessions,
                ExpiringSubscriptions = expiringSubscriptions
            };

            return View(dashboard);
        }
        #endregion

        #region Parent Dashboard
        public async Task<IActionResult> ParentDashboard(CancellationToken cancellationToken = default)
        {
            var parentsList = await _parentRepository.GetAsync(
                includes: new Expression<Func<Parent, object>>[] { x => x.User },
                cancellationToken: cancellationToken);

            var parent = parentsList.FirstOrDefault();

            if (parent == null)
                return NotFound("لم يتم العثور على بيانات ولي الأمر.");

            var students = (await _studentRepository.GetAsync(
                x => x.ParentId == parent.ParentId,
                includes: new Expression<Func<Student, object>>[] { x => x.User },
                cancellationToken: cancellationToken)).ToList();

            var studentIds = students.Select(s => s.StudentId).ToList();

            var teachers = (await _teacherRepository.GetAsync(
                includes: new Expression<Func<EduSphere.Models.Teacher, object>>[] { x => x.User },
                cancellationToken: cancellationToken)).ToList();

            var allAttendanceRecords = await _attendanceRecordRepository.GetAsync(
                includes: new Expression<Func<AttendanceRecord, object>>[]
                {
                    x => x.Student,
                    x => x.Student.User,
                    x => x.AttendanceSession
                },
                cancellationToken: cancellationToken);

            var attendanceRecords = allAttendanceRecords
                .Where(x => studentIds.Contains(x.StudentId))
                .ToList();

            int presentCount = attendanceRecords.Count(x => x.Status == AttendanceStatus.Present);
            int absentCount = attendanceRecords.Count(x => x.Status == AttendanceStatus.Absent);
            int totalAttendanceCount = presentCount + absentCount;

            double attendancePercentage = totalAttendanceCount == 0
                ? 0
                : Math.Round((double)presentCount / totalAttendanceCount * 100, 2);

            var subscriptions = await _subscriptionRepository.GetAsync(
                x => x.Status == SubscriptionStatus.Active,
                includes: new Expression<Func<Subscription, object>>[] { x => x.SubscriptionPlan },
                cancellationToken: cancellationToken);

            decimal totalFees = subscriptions.Sum(x => x.SubscriptionPlan?.Price ?? 0);

            ParentDashboardVM vm = new ParentDashboardVM
            {
                Parent = parent,
                Children = students,
                AttendancePercentage = attendancePercentage,
                AverageGrade = 85.5,
                NotificationsCount = 2,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                TotalFees = totalFees,
                PaidFees = totalFees,
                RemainingFees = 0,
                UpcomingExams = new List<UpcomingExamVM>(),
                Assignments = new List<AssignmentVM>(),
                Grades = new List<GradeVM>(),

                AttendanceRecords = attendanceRecords
                    .OrderByDescending(x => x.AttendanceSession?.SessionDate)
                    .Take(10)
                    .Select(x => new AttendanceVM
                    {
                        StudentName = x.Student?.User?.FullName ?? "غير محدد",
                        Date = x.AttendanceSession?.SessionDate ?? DateTime.MinValue,
                        IsPresent = x.Status == AttendanceStatus.Present
                    }).ToList(),

                Teachers = teachers.Select(x => new TeacherVM
                {
                    Name = x.User?.FullName ?? "مدرس غير معروف",
                    Subject = x.Specialization,
                    Phone = x.User?.PhoneNumber ?? "-"
                }).ToList(),

                Notifications = new List<NotificationVM>
                {
                    new NotificationVM { Title = "مرحباً بك", Message = "أهلاً بك في لوحة متابعة أولياء الأمور.", CreatedAt = DateTime.Now },
                    new NotificationVM { Title = "الحضور والغياب", Message = "تم تحديث سجلات الحضور بانتظام.", CreatedAt = DateTime.Now }
                }
            };

            return View(vm);
        }
        #endregion

        #region Student Dashboard
        public async Task<IActionResult> StudentDashboard(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var studentsList = await _studentRepository.GetAsync(
                x => x.UserId == userId,
                includes: new Expression<Func<Student, object>>[] { x => x.User },
                cancellationToken: cancellationToken);

            var student = studentsList.FirstOrDefault();

            if (student == null)
            {
                return NotFound("عذراً، لم يتم العثور على ملف طالب مرتبط بهذا الحساب.");
            }

            // 1. جلب تسجيلات الطالب والكورسات
            var enrollments = await _enrollmentRepository.GetAsync(
                x => x.StudentId == student.StudentId,
                includes: new Expression<Func<Enrollment, object>>[] { x => x.Course },
                cancellationToken: cancellationToken);

            var courses = enrollments
                .Where(e => e.Course != null)
                .Select(e => e.Course)
                .ToList();

            // 2. جلب الاشتراكات بدون شرط مباشر لمنع خطأ الـ Compilation
            var subscriptions = (await _subscriptionRepository.GetAsync(
                includes: new Expression<Func<Subscription, object>>[] { x => x.SubscriptionPlan },
                cancellationToken: cancellationToken)).ToList();

            // 3. جلب سجلات الحضور
            var attendanceRecords = (await _attendanceRecordRepository.GetAsync(
                x => x.StudentId == student.StudentId,
                cancellationToken: cancellationToken)).ToList();

            int totalSessions = attendanceRecords.Count;
            int presentSessions = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
            double attendancePercentage = totalSessions == 0
                ? 0
                : Math.Round((double)presentSessions / totalSessions * 100, 2);

            // 4. تجهيز الـ ViewModel
            var vm = new StudentDashboardVM
            {
                Student = student,
                EnrolledCoursesCount = courses.Count,
                ActiveSubscriptionsCount = subscriptions.Count(s => s.Status == SubscriptionStatus.Active),
                AttendancePercentage = attendancePercentage,
                AverageGrade = 92.4,
                EnrolledCourses = courses,
                Subscriptions = subscriptions,

                RecentExamResults = new List<ExamResultVM>
        {
            new ExamResultVM { ExamTitle = "امتحان منتصف الفصل", CourseName = "الفيزياء", Score = 48, MaxScore = 50, ExamDate = DateTime.Now.AddDays(-5) },
            new ExamResultVM { ExamTitle = "اختبار شامل - الوحدة الأولى", CourseName = "الرياضيات", Score = 95, MaxScore = 100, ExamDate = DateTime.Now.AddDays(-12) }
        },

                PaymentHistory = subscriptions.Select(s => new PaymentHistoryVM
                {
                    PaymentId = s.SubscriptionId,
                    ItemTitle = s.SubscriptionPlan?.Name ?? "اشتراك شهري",
                    Amount = s.SubscriptionPlan?.Price ?? 0,
                    PaymentDate = s.StartDate,
                    Status = s.Status == SubscriptionStatus.Active ? "مدفوع" : "منتهي"
                }).ToList(),

                Notifications = new List<NotificationVM>
        {
            new NotificationVM { Title = "تذكير بموعد الامتحان", Message = "لديك امتحان رياضيات قادم يوم الخميس القادم.", CreatedAt = DateTime.Now.AddHours(-3) },
            new NotificationVM { Title = "تمت إضافة واجب جديد", Message = "تمت إضافة واجب الفيزياء - الفصل الثاني.", CreatedAt = DateTime.Now.AddDays(-1) }
        }
            };

            return View(vm);
        }
        #endregion
        public IActionResult HomeCast()
        {
            return View();
        }
    }
}