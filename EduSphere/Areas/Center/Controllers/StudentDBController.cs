using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EduSphere.Areas.Center.Controllers
{
    [Area("Center")]
  //  [Authorize(Roles = "Student,SuperAdmin")]

    public class StudentDBController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Exam> _examRepository;
        private readonly IRepository<ExamAttempt> _examAttemptRepository;
        private readonly IRepository<AttendanceRecord> _attendanceRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<EduSphere.Models.Teacher> _teacherRepository;
        private readonly IRepository<Lecture> _lectureRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;

        private readonly ILogger<StudentDBController> _logger;

        public StudentDBController(
            UserManager<ApplicationUser> userManager,
            IRepository<Student> studentRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<Course> courseRepository,
            IRepository<Exam> examRepository,
            IRepository<ExamAttempt> examAttemptRepository,
            IRepository<AttendanceRecord> attendanceRepository,
            IRepository<Group> groupRepository,
            IRepository<EduSphere.Models.Teacher> teacherRepository,
            IRepository<Lecture> lectureRepository,
            IRepository<Subscription> subscriptionRepository,
            ILogger<StudentDBController> logger)
        {
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _examRepository = examRepository;
            _examAttemptRepository = examAttemptRepository;
            _attendanceRepository = attendanceRepository;
            _groupRepository = groupRepository;
            _teacherRepository = teacherRepository;
            _lectureRepository = lectureRepository;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> StudentDB( CancellationToken cancellationToken)
        {
            
            try
            {
                var student = (await _studentRepository.GetAsync(
    includes: new Expression<Func<Student, object>>[]
    {
        s => s.User,
        s => s.Center
    },
    cancellationToken: cancellationToken))
    .FirstOrDefault();

                if (student == null)
                    return Content("No students found.");

                var studentId = student.StudentId;

                var enrollments = (await _enrollmentRepository.GetAsync(
                    e => e.StudentId == studentId,
                    includes: new Expression<Func<Enrollment, object>>[]
                    {
        e => e.Course,
        e => e.Group
                    },
                    cancellationToken: cancellationToken))
                    .ToList();

                var courseIds = enrollments
                    .Select(e => e.CourseId)
                    .Distinct()
                    .ToList();

                var teacherIds = enrollments
                    .Where(e => e.Course != null)
                    .Select(e => e.Course.TeacherId)
                    .Distinct()
                    .ToList();

                var courses = (await _courseRepository.GetAsync(
                    c => courseIds.Contains(c.CourseId),
                    cancellationToken: cancellationToken))
                    .ToList();

                var teachers = (await _teacherRepository.GetAsync(
                    t => teacherIds.Contains(t.TeacherId),
                    includes: new Expression<Func<EduSphere.Models.Teacher, object>>[]
                    {
        t => t.User
                    },
                    cancellationToken: cancellationToken))
                    .ToList();

                var attendance = (await _attendanceRepository.GetAsync(
                    a => a.StudentId == studentId,
                    cancellationToken: cancellationToken))
                    .ToList();



                var lectures = (await _lectureRepository.GetAsync(
                    l => courseIds.Contains(l.CourseId),
                    includes: new Expression<Func<Lecture, object>>[]
                    {
                        l => l.Course
                    },
                    cancellationToken: cancellationToken))
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(5)
                    .ToList();
                var totalAttendance = attendance.Count;

                var presentCount = attendance.Count(a => a.Status == AttendanceStatus.Present);
                var absentCount = attendance.Count(a => a.Status == AttendanceStatus.Absent);
                var lateCount = attendance.Count(a => a.Status == AttendanceStatus.Late);

                double attendancePercentage = totalAttendance == 0
                    ? 0
                    : (double)presentCount / totalAttendance * 100;




                var activeEnrollment = enrollments
                    .FirstOrDefault(e => e.Status == EnrollmentStatus.Active);

                var vm = new StudentDashboardVM
                {
                    Student = student,

                    EnrolledCoursesCount = enrollments.Count,


                    ActiveCoursesCount = enrollments.Count(e =>
                        e.Status == EnrollmentStatus.Active),

                    CompletedCoursesCount = enrollments.Count(e =>
                        e.Status == EnrollmentStatus.Completed),

                    AttendancePercentage = attendancePercentage,



                    PresentCount = presentCount,

                    AbsentCount = absentCount,

                    LateCount = lateCount,



                    CurrentGroupName = activeEnrollment?.Group?.Name
                        ?? "No Group"
                };
                vm.Courses = enrollments.Select(e =>
                {
                    var teacher = teachers.FirstOrDefault(t =>
                        t.TeacherId == e.Course?.TeacherId);

                    return new CourseCardVM
                    {
                        CourseId = e.CourseId,
                        CourseTitle = e.Course?.Title ?? "-",
                        TeacherName = teacher?.User?.FullName ?? "-",
                        EnrollmentDate = e.EnrollmentDate,
                        Status = e.Status
                    };
                }).ToList();

                vm.Teachers = teachers.Select(t => new TeacherCardVM
                {
                    TeacherId = t.TeacherId,
                    FullName = t.User?.FullName ?? "-",
                    Email = t.User?.Email ?? "-",
                    PhoneNumber = t.User?.PhoneNumber ?? "-",
                    Specialization = t.Specialization ?? "-"
                }).ToList();

                vm.LatestLectures = lectures.Select(l => new LectureCardVM
                {
                    LectureId = l.LectureId,
                    Title = l.Title,
                    CourseName = l.Course?.Title ?? "-",
                    Order = l.Order,
                    CreatedAt = l.CreatedAt
                }).ToList();


                vm.EnrolledCourses = courses;

                return View(vm);
            }
            catch (Exception ex)
            {
                

                TempData["error"] = "Unable to load student dashboard.";

                return RedirectToAction("Index", "Students");
            }
        }

        public IActionResult Details(int id)
        {
            return RedirectToAction(nameof(StudentDB), new
            {
                studentId = id
            });
        }
    }
}