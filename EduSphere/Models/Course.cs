using System.Text.RegularExpressions;

namespace EduSphere.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        public int CenterId { get; set; }
        public Center? Center { get; set; }

        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation

        public ICollection<Group>? Groups { get; set; }

        public ICollection<Lecture>? Lectures { get; set; }

        public ICollection<Exam>? Exams { get; set; }
    }
}