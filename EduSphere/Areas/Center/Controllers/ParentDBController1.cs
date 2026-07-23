using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "Parent,SuperAdmin")]
    public class ParentDBController1 : Controller
    {
        #region Fields

        private readonly IRepository<Parent> _parentRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<AttendanceRecord> _attendanceRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<EduSphere.Models.Teacher> _teacherRepository;
        private readonly ILogger<ParentDBController1> _logger;

        #endregion

        #region Constructor

        public ParentDBController1(
            IRepository<Parent> parentRepository,
            IRepository<Student> studentRepository,
            IRepository<AttendanceRecord> attendanceRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<EduSphere.Models.Teacher> teacherRepository,
            ILogger<ParentDBController1> logger)
        {
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _attendanceRepository = attendanceRepository;
            _enrollmentRepository = enrollmentRepository;
            _teacherRepository = teacherRepository;
            _logger = logger;
        }

        #endregion

        #region Index

        public async Task<IActionResult> ParentDB(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var parent = await GetCurrentParentAsync(userId, cancellationToken);
                if (parent is null)
                {
                    return NotFound();
                }

                var children = await GetChildrenAsync(parent.ParentId, cancellationToken);
                var studentIds = children.Select(c => c.StudentId).ToList();

                var (presentCount, absentCount, attendancePercentage) =
                    await GetAttendanceSummaryAsync(studentIds, cancellationToken);

                var latestAttendance = await GetLatestAttendanceRecordsAsync(studentIds, cancellationToken);
                var teachers = await GetTeachersForChildrenAsync(studentIds, cancellationToken);

                var vm = new ParentDashboardVM
                {
                    Parent = parent,
                    Children = children,
                    AttendanceRecords = latestAttendance,
                    Teachers = teachers,
                    PresentCount = presentCount,
                    AbsentCount = absentCount,
                    AttendancePercentage = attendancePercentage,

                    // NOTE: Exam entity was not provided. Populate once the Exam / ExamAttempt
                    // entity definitions are available.
                    UpcomingExams = new List<UpcomingExamVM>(),

                    // NOTE: Assignment entity was not provided.
                    Assignments = new List<AssignmentVM>(),

                    // NOTE: Grade entity was not provided.
                    Grades = new List<GradeVM>(),
                    AverageGrade = 0,

                    // NOTE: Notification entity was not provided.
                    Notifications = new List<NotificationVM>(),
                    NotificationsCount = 0,

                    // NOTE: Payment entity was not provided, and Subscription has no
                    // StudentId/ParentId link in the given schema, so fees cannot be
                    // attributed to this parent's children yet.
                    TotalFees = 0,
                    PaidFees = 0,
                    RemainingFees = 0
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load parent dashboard for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region Helper Methods

        private async Task<Parent?> GetCurrentParentAsync(string userId, CancellationToken cancellationToken)
        {
            return await _parentRepository.GetOneAsync(
                filter: p => p.UserId == userId,
                tracked: false,
                cancellationToken: cancellationToken);
        }

        private async Task<List<Student>> GetChildrenAsync(
    int parentId,
    CancellationToken cancellationToken)
        {
            var children = await _studentRepository.GetAsync(
                filter: s => s.ParentId == parentId,
                includes: new Expression<Func<Student, object>>[]
                {
            s => s.Center
                },
                tracked: false,
                cancellationToken: cancellationToken);

            return children.ToList();
        }

        private async Task<(int PresentCount, int AbsentCount, double AttendancePercentage)> GetAttendanceSummaryAsync(
            List<int> studentIds, CancellationToken cancellationToken)
        {
            if (studentIds.Count == 0)
            {
                return (0, 0, 0);
            }

            var presentCount = await _attendanceRepository.CountAsync(
                a => studentIds.Contains(a.StudentId) && a.Status == AttendanceStatus.Present);

            var absentCount = await _attendanceRepository.CountAsync(
                a => studentIds.Contains(a.StudentId) && a.Status == AttendanceStatus.Absent);

            var totalCount = await _attendanceRepository.CountAsync(
                a => studentIds.Contains(a.StudentId));

            var percentage = totalCount > 0
                ? Math.Round((double)presentCount / totalCount * 100, 2)
                : 0;

            return (presentCount, absentCount, percentage);
        }

        private async Task<List<AttendanceVM>> GetLatestAttendanceRecordsAsync(
            List<int> studentIds, CancellationToken cancellationToken)
        {
            if (studentIds.Count == 0)
            {
                return new List<AttendanceVM>();
            }

            var records = await _attendanceRepository.GetAsync(
    filter: a => studentIds.Contains(a.StudentId),
    orderBy: q => q.OrderByDescending(a => a.AttendanceSession!.SessionDate),
    includes: new Expression<Func<AttendanceRecord, object>>[]
{
    a => a.Student!,
    a => a.AttendanceSession!,
    a => a.Student!.User!
},
    tracked: false,
    cancellationToken: cancellationToken,
    skip: 0,
    take: 10);
            return records.Select(MapToAttendanceVM).ToList();
        }

        private static AttendanceVM MapToAttendanceVM(AttendanceRecord record)
{
    return new AttendanceVM
    {
        StudentName = record.Student?.User?.FullName ?? "-",
        Date = record.AttendanceSession?.SessionDate ?? DateTime.MinValue,
        IsPresent = record.Status == AttendanceStatus.Present
    };
}

        private async Task<List<TeacherVM>> GetTeachersForChildrenAsync(
            List<int> studentIds, CancellationToken cancellationToken)
        {
            if (studentIds.Count == 0)
            {
                return new List<TeacherVM>();
            }

            var enrollments = await _enrollmentRepository.GetAsync(
     filter: e => studentIds.Contains(e.StudentId),
     includes: new Expression<Func<Enrollment, object>>[]
     {
        e => e.Group!,
        e => e.Course!
     },
     tracked: false,
     cancellationToken: cancellationToken);

            var teacherIds = enrollments
                .Select(e => e.Group?.TeacherId)
                .Concat(enrollments.Select(e => e.Course?.TeacherId))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (teacherIds.Count == 0)
            {
                return new List<TeacherVM>();
            }

            var teachers = await _teacherRepository.GetAsync(
     filter: t => teacherIds.Contains(t.TeacherId),
     includes: new Expression<Func<EduSphere.Models.Teacher, object>>[]
     {
        t => t.User!
     },
     tracked: false,
     cancellationToken: cancellationToken);

            return teachers.Select(MapToTeacherVM).ToList();
        }

        private static TeacherVM MapToTeacherVM(EduSphere.Models.Teacher teacher)
        {
            return new TeacherVM
            {
                // NOTE: Teacher.User navigation exists, but the User entity's own
                // properties (e.g. full name, phone number) were not provided, so
                // Name/Phone cannot be safely mapped yet without inventing property
                // names. Populate once the User entity definition is available.
                Name = teacher.User?.FullName ?? "-",
                Phone = teacher.User?.PhoneNumber ?? "-",
                Subject = teacher.Specialization
            };
        }

        #endregion
    }
}