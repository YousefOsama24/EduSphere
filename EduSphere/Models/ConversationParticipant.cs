namespace EduSphere.Models
{
    public class ConversationParticipant
    {
        public int ConversationParticipantId { get; set; }

        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}