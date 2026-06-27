namespace EduSphere.Models
{
    public enum NotificationType
    {
        Announcement,
        Attendance,
        Exam,
        Payment,
        Course,
        General
    }

    public class Notification
    {
        public int NotificationId { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}