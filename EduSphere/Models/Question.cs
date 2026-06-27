namespace EduSphere.Models
{
    public enum QuestionType
    {
        MCQ,
        Essay
    }

    public class Question
    {
        public int QuestionId { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public string Content { get; set; } = string.Empty;

        public QuestionType Type { get; set; }

        public int Marks { get; set; }

        // Navigation

        public ICollection<Choice>? Choices { get; set; }

        public ICollection<StudentAnswer>? StudentAnswers { get; set; }
    }
}