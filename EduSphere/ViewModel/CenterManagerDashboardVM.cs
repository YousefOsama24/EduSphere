namespace EduSphere.ViewModel
{
    public class CenterManagerDashboardVM
    {
        // الكروت الرئيسية
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalGroups { get; set; }
        public int TotalHalls { get; set; }

        // جدول الحصص اليومي
        public IEnumerable<CenterScheduleVM> TodaySchedule { get; set; } = new List<CenterScheduleVM>();
    }

    public class CenterScheduleVM
    {
        public string Subject { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int StudentCount { get; set; }
    }
}