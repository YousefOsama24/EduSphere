namespace EduSphere.Models
{
    public class Lecture
    {
        public int LectureId { get; set; }

        public int CourseId { get; set; }

        public Course? Course { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        public string? PdfUrl { get; set; }

        public bool IsPreview { get; set; }

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}