
namespace EduSphere.Models
{
    
        public class Center
        {
            public int CenterId { get; set; }

            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string Address { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string? LogoUrl { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public DateTime UpdatedAt { get; set; } = DateTime.Now;
            public bool IsDeleted { get; set; }

        // Navigation

        public ICollection<Teacher>? Teachers { get; set; }

            public ICollection<Student>? Students { get; set; }

            public ICollection<Course>? Courses { get; set; } = new List<Course>();

        public ICollection<Subscription>? Subscriptions { get; set; }
        }
    }