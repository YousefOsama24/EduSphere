namespace EduSphere.Models
{
    public class Exam
    {
        public int ExamId { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TotalMarks { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // Navigation

        public ICollection<Question>? Questions { get; set; }

        public ICollection<ExamAttempt>? ExamAttempts { get; set; }
    }
}