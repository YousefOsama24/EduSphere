namespace EduSphere.ViewModel
{
    public class NotificationsVM
    {
        public Notification Notification { get; set; } = new Notification();

        public IEnumerable<Notification> Notifications { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
