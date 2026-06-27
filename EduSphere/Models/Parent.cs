namespace EduSphere.Models
{
    
        public class Parent
        {
            public int ParentId { get; set; }

            public string UserId { get; set; } = string.Empty;

            public ApplicationUser? User { get; set; }

            public string Occupation { get; set; } = string.Empty;

            // Navigation

            public ICollection<ParentStudent> ParentStudents { get; set; }  = new List<ParentStudent>();
        }
    }