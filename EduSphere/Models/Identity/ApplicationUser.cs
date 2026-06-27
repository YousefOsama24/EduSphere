using Microsoft.AspNetCore.Identity;

namespace EduSphere.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? ProfileImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Teacher? Teacher { get; set; }

        public Student? Student { get; set; }

        public Parent? Parent { get; set; }

        public ICollection<Notification>? Notifications { get; set; }

        public ICollection<Message>? SentMessages { get; set; }

        public ICollection<ConversationParticipant>? Conversations { get; set; }
    }
}