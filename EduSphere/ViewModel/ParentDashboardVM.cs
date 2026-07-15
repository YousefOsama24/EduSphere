using EduSphere.Models;

namespace EduSphere.ViewModel
{
    public class ParentDashboardVM
    {
        public Parent Parent { get; set; } = new();

        public IEnumerable<Student> Children { get; set; } = new List<Student>();

        public IEnumerable<UpcomingExamVM> UpcomingExams { get; set; } = new List<UpcomingExamVM>();

        public IEnumerable<AssignmentVM> Assignments { get; set; } = new List<AssignmentVM>();

        public IEnumerable<GradeVM> Grades { get; set; } = new List<GradeVM>();

        public IEnumerable<AttendanceVM> AttendanceRecords { get; set; } = new List<AttendanceVM>();

        public IEnumerable<TeacherVM> Teachers { get; set; } = new List<TeacherVM>();

        public IEnumerable<NotificationVM> Notifications { get; set; } = new List<NotificationVM>();

        public double AttendancePercentage { get; set; }

        public double AverageGrade { get; set; }

        public int NotificationsCount { get; set; }

        public int PresentCount { get; set; }

        public int AbsentCount { get; set; }

        public decimal TotalFees { get; set; }

        public decimal PaidFees { get; set; }

        public decimal RemainingFees { get; set; }
    }

    public class UpcomingExamVM
    {
        public string Subject { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public DateTime Date { get; set; }
    }

    public class AssignmentVM
    {
        public string Title { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public bool IsSubmitted { get; set; }
    }

    public class GradeVM
    {
        public string StudentName { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public double Mark { get; set; }

        public double Percentage { get; set; }
    }

    public class AttendanceVM
    {
        public string StudentName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }
    }

    public class TeacherVM
    {
        public string Name { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }

    public class NotificationVM
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}