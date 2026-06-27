using System.Text.RegularExpressions;

namespace EduSphere.Models
{
    
        public class Teacher
        {
            public int TeacherId { get; set; }

            public string UserId { get; set; } = string.Empty;

            public ApplicationUser? User { get; set; }

            public int CenterId { get; set; }

            public Center? Center { get; set; }

            public string Specialization { get; set; } = string.Empty;

            public DateTime HireDate { get; set; } = DateTime.Now;

            public bool IsActive { get; set; } = true;

            // Navigation

            public ICollection<Course>? Courses { get; set; }

            public ICollection<Group>? Groups { get; set; }

            public ICollection<AttendanceSession>? AttendanceSessions { get; set; }
        }
    }