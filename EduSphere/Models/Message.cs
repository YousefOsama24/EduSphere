namespace EduSphere.Models
{
    public class Message
    {
        public int MessageId { get; set; }

        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        public string SenderUserId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; }

        public DateTime? EditedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}