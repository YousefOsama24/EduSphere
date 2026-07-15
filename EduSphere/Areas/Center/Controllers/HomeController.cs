using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Serilog.Parsing;
using System.Diagnostics;
using System.Linq.Expressions;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    public class HomeController : Controller
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<EduSphere.Models.Teacher> _teacherRepository; private readonly IRepository<Course> _courseRepository;
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
            IRepository<Subscription> subscriptionRepository)
        {
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _groupRepository = groupRepository;
            _parentRepository = parentRepository;
            _enrollmentRepository = enrollmentRepository;
            _attendanceSessionRepository = attendanceSessionRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<IActionResult> Index()
        {
            DashboardVM dashboard = new DashboardVM
            {
                TotalStudents =
                    await _studentRepository.CountAsync(),

                TotalTeachers =
                    await _teacherRepository.CountAsync(),

                TotalCourses =
                    await _courseRepository.CountAsync(),

                TotalGroups =
                    await _groupRepository.CountAsync(),

                TotalParents =
                    await _parentRepository.CountAsync(),

                TotalAttendanceSessions =
                    await _attendanceSessionRepository.CountAsync(),

                ActiveSubscriptions =
                    (await _subscriptionRepository.FindAsync(
                        x => x.Status == SubscriptionStatus.Active))
                    .Count(),

                LatestStudents =
                    (await _studentRepository.GetAsync())
                    .OrderByDescending(x => x.StudentId)
                    .Take(5),

                LatestTeachers =
                    (await _teacherRepository.GetAsync())
                    .OrderByDescending(x => x.TeacherId)
                    .Take(5),

                LatestEnrollments =
                    (await _enrollmentRepository.GetAsync())
                    .OrderByDescending(x => x.EnrollmentDate)
                    .Take(5),

                TodaySessions =
                    (await _attendanceSessionRepository.GetAsync())
                    .Where(x => x.SessionDate.Date == DateTime.Today)
                    .OrderBy(x => x.SessionDate)
                    .Take(10),

                ExpiringSubscriptions =
                    (await _subscriptionRepository.GetAsync())
                    .Where(x =>
                        x.EndDate <= DateTime.Today.AddDays(7))
                    .OrderBy(x => x.EndDate)
                    .Take(5)
            };

            return View(dashboard);
        }

        public IActionResult HomeCast()
        {
            return View();
        }

        public async Task<IActionResult> ParentDashboard(
    CancellationToken cancellationToken = default)
        {
            var parent = (await _parentRepository.GetAsync(
                includes: new Expression<Func<Parent, object>>[]
                {
            x => x.User
                },
                cancellationToken: cancellationToken))
                .FirstOrDefault();

            if (parent == null)
                return NotFound();

            var students = await _studentRepository.GetAsync(
                x => x.ParentId == parent.ParentId,
                includes: new Expression<Func<Student, object>>[]
                {
            x => x.User
                },
                cancellationToken: cancellationToken);

            var teachers = await _teacherRepository.GetAsync(
                includes: new Expression<Func<EduSphere.Models.Teacher, object>>[]
                {
            x => x.User
                },
                cancellationToken: cancellationToken);

            var attendanceRecords =
       await _attendanceRecordRepository.GetAsync(
           includes: new Expression<Func<AttendanceRecord, object>>[]
           {
            x => x.Student,
            x => x.AttendanceSession
           },
           cancellationToken: cancellationToken);

            attendanceRecords = attendanceRecords.Where(x =>
                students.Any(s => s.StudentId == x.StudentId));

            int presentCount =
                attendanceRecords.Count(x => x.Status == AttendanceStatus.Present);

            int absentCount =
                attendanceRecords.Count(x => x.Status == AttendanceStatus.Absent);

            var subscriptions =
    await _subscriptionRepository.GetAsync(
        x => x.Status == SubscriptionStatus.Active,
        includes: new Expression<Func<Subscription, object>>[]
        {
            x => x.SubscriptionPlan
        },
        cancellationToken: cancellationToken);

            int totalStudents = students.Count();

            int notificationsCount = 0;

            int presentCount0 = attendanceRecords.Count(x => x.Status == AttendanceStatus.Present);

            int absentCount0 = attendanceRecords.Count(x => x.Status == AttendanceStatus.Absent);

            double attendancePercentage =
                presentCount0 + absentCount0 == 0
                ? 0
                : Math.Round(
                    (double)presentCount0 /
                    (presentCount0 + absentCount0) * 100,
                    2);

            decimal totalFees =
                subscriptions.Sum(x => x.SubscriptionPlan?.Price ?? 0);

            decimal paidFees = totalFees;

            decimal remainingFees = 0;
            ParentDashboardVM vm = new ParentDashboardVM
            {
                Parent = parent,

                Children = students,

                AttendancePercentage = attendancePercentage,

                AverageGrade = 0,

                NotificationsCount = notificationsCount,

                PresentCount = presentCount0,

                AbsentCount = absentCount0,

                TotalFees = totalFees,

                PaidFees = paidFees,

                RemainingFees = remainingFees,

                UpcomingExams = new List<UpcomingExamVM>(),

                Assignments = new List<AssignmentVM>(),

                Grades = new List<GradeVM>(),

                AttendanceRecords = attendanceRecords
    .Take(10)
    .Select(x => new AttendanceVM
    {
        StudentName = x.Student?.User?.FullName ?? "Unknown",

        Date = x.AttendanceSession?.SessionDate ?? DateTime.MinValue,

        IsPresent = x.Status == AttendanceStatus.Present
    })
    .ToList(),

                Teachers = teachers
        .Select(x => new TeacherVM
        {
            Name = x.User != null
                ? x.User.FullName
                : "Unknown Teacher",

            Subject = x.Specialization,

            Phone = x.User != null
                ? x.User.PhoneNumber ?? "-"
                : "-"
        })
        .ToList(),

                Notifications = new List<NotificationVM>()
        {
            new NotificationVM
            {
                Title = "Welcome",

                Message = "Welcome to EduSphere Parent Dashboard.",

                CreatedAt = DateTime.Now
            },

            new NotificationVM
            {
                Title = "Attendance",

                Message = "Attendance records are updated daily.",

                CreatedAt = DateTime.Now
            }
        }
            };

            return View(vm);
        }

        public IActionResult StudentDashboard()
        {
            return View();
        }

        public IActionResult TeacherDashboard()
        {
            return View();
        }
    }
}