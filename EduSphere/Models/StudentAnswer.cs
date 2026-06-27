namespace EduSphere.Models
{
    public class StudentAnswer
    {
        public int StudentAnswerId { get; set; }

        public int ExamAttemptId { get; set; }
        public ExamAttempt? ExamAttempt { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int? SelectedChoiceId { get; set; }
        public Choice? SelectedChoice { get; set; }

        public string? EssayAnswer { get; set; }

        public decimal MarksAwarded { get; set; }
    }
}