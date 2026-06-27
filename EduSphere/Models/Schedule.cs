namespace EduSphere.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }

        public int GroupId { get; set; }

        public Group? Group { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Room { get; set; } = string.Empty;
    }
}