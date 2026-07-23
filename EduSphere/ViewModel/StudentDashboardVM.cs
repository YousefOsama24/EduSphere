using EduSphere.Models;

namespace EduSphere.ViewModel
{
    public class StudentDashboardVM
    {
        #region Existing Properties

        public Student Student { get; set; } = null!;

        public int EnrolledCoursesCount { get; set; }

        public int ActiveSubscriptionsCount { get; set; }

        public double AttendancePercentage { get; set; }

        public double AverageGrade { get; set; }

        public IEnumerable<Course> EnrolledCourses { get; set; } = new List<Course>();

        public IEnumerable<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        public IEnumerable<ExamResultVM> RecentExamResults { get; set; } = new List<ExamResultVM>();

        public IEnumerable<PaymentHistoryVM> PaymentHistory { get; set; } = new List<PaymentHistoryVM>();

        public IEnumerable<NotificationVM> Notifications { get; set; } = new List<NotificationVM>();

        #endregion

        #region Dashboard Properties

        public int ActiveCoursesCount { get; set; }

        public int CompletedCoursesCount { get; set; }

        public int UpcomingExamsCount { get; set; }

        public int PresentCount { get; set; }

        public int AbsentCount { get; set; }

        public int LateCount { get; set; }

        public decimal HighestGrade { get; set; }

        public decimal LowestGrade { get; set; }

        public int CompletedExamsCount { get; set; }

        public string CurrentSubscriptionStatus { get; set; } = "No Subscription";

        public string CurrentGroupName { get; set; } = "No Group";

        public IEnumerable<CourseCardVM> Courses { get; set; } = new List<CourseCardVM>();

        public IEnumerable<TeacherCardVM> Teachers { get; set; } = new List<TeacherCardVM>();

        public IEnumerable<UpcomingExamVM> UpcomingExams { get; set; } = new List<UpcomingExamVM>();

        public IEnumerable<LectureCardVM> LatestLectures { get; set; } = new List<LectureCardVM>();

        #endregion
    }

    public class ExamResultVM
    {
        public string ExamTitle { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public double Score { get; set; }

        public double MaxScore { get; set; }

        public DateTime ExamDate { get; set; }
    }

    public class PaymentHistoryVM
    {
        public int PaymentId { get; set; }

        public string ItemTitle { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    // ===========================
    // New Classes
    // ===========================

    public class CourseCardVM
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string TeacherName { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public EnrollmentStatus Status { get; set; }
    }

    public class TeacherCardVM
    {
        public int TeacherId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;
    }

   

    public class LectureCardVM
    {
        public int LectureId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}