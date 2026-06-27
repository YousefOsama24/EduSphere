namespace EduSphere.Models
{
    public enum ExamAttemptStatus
    {
        Started,
        Submitted,
        Expired
    }

    public class ExamAttempt
    {
        public int ExamAttemptId { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? SubmitTime { get; set; }

        public decimal Score { get; set; }

        public ExamAttemptStatus Status { get; set; }

        // Navigation

        public ICollection<StudentAnswer>? StudentAnswers { get; set; }
    }
}