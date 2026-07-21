using EduSphere.Models;

namespace EduSphere.ViewModel
{
    public class StudentDashboardVM
    {
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
}