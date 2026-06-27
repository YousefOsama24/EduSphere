namespace EduSphere.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation

        public ICollection<ConversationParticipant>? Participants { get; set; }

        public ICollection<Message>? Messages { get; set; }
    }
}