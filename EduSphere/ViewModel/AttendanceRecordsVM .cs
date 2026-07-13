namespace EduSphere.ViewModel
{
    public class AttendanceRecordsVM
    {
        public AttendanceRecord AttendanceRecord { get; set; } = new AttendanceRecord();

        public IEnumerable<AttendanceRecord> AttendanceRecords { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
