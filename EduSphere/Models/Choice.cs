namespace EduSphere.Models
{
    public class Choice
    {
        public int ChoiceId { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}