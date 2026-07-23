namespace EduSphere.Models
{

    public class Student
    {
        public int StudentId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
        public int ParentId { get; set; }
        public Parent? Parent { get; set; }
        public int CenterId { get; set; }

        public Center? Center { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string AcademicLevel { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation

        public ICollection<Enrollment>? Enrollments { get; set; }

        public ICollection<ExamAttempt>? ExamAttempts { get; set; }

        public ICollection<AttendanceRecord>? AttendanceRecords { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}