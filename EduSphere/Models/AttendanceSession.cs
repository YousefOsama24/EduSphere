namespace EduSphere.Models
{
    public class AttendanceSession
    {
        public int AttendanceSessionId { get; set; }

        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime SessionDate { get; set; }

        // Navigation

        public ICollection<AttendanceRecord>? AttendanceRecords { get; set; }
    }
}