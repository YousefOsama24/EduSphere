using EduSphere.Models;

namespace EduSphere.ViewModel
{
    public class DashboardVM
    {
        // Statistics
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalGroups { get; set; }

        public int TotalParents { get; set; }

        public int TotalAttendanceSessions { get; set; }

        public int TodayAttendance { get; set; }

        public int ActiveSubscriptions { get; set; }

        // Recent Data

        public IEnumerable<Student>? LatestStudents { get; set; }

        public IEnumerable<Teacher>? LatestTeachers { get; set; }

        public IEnumerable<Enrollment>? LatestEnrollments { get; set; }

        public IEnumerable<AttendanceSession>? TodaySessions { get; set; }

        public IEnumerable<Subscription>? ExpiringSubscriptions { get; set; }
    }
}