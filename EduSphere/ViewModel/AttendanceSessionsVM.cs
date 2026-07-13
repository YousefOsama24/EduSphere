namespace EduSphere.ViewModel
{
    public class AttendanceSessionsVM
    {
        public AttendanceSession AttendanceSession { get; set; } = new AttendanceSession();

        public IEnumerable<AttendanceSession> AttendanceSessions { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
