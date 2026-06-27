namespace EduSphere.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late
    }

    public class AttendanceRecord
    {
        public int AttendanceRecordId { get; set; }

        public int AttendanceSessionId { get; set; }
        public AttendanceSession? AttendanceSession { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public AttendanceStatus Status { get; set; }

        public string? Notes { get; set; }
    }
}