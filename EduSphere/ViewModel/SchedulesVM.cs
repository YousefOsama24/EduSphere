namespace EduSphere.ViewModel
{
    public class SchedulesVM
    {
        public Schedule Schedule { get; set; } = new Schedule();

        public IEnumerable<Schedule> Schedules { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
