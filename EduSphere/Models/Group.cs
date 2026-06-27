namespace EduSphere.Models
{
    public class Group
    {
        public int GroupId { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // Navigation

        public ICollection<Enrollment>? Enrollments { get; set; }

        public ICollection<AttendanceSession>? AttendanceSessions { get; set; }

        public ICollection<Schedule>? Schedules { get; set; }
    }
}