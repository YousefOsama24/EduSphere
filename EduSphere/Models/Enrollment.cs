namespace EduSphere.Models
{
    public enum EnrollmentStatus
    {
        Pending,
        Active,
        Cancelled,
        Completed
    }

    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }

        public Student? Student { get; set; }

        public int GroupId { get; set; }

        public Group? Group { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public EnrollmentStatus Status { get; set; } =
            EnrollmentStatus.Active;
    }
}