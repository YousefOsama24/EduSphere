using EduSphere.Models;
namespace EduSphere.ViewModel
{
    public class EnrollmentsVM
    {
        public Enrollment Enrollment { get; set; } = new Enrollment();

        public IEnumerable<Enrollment> Enrollments { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
