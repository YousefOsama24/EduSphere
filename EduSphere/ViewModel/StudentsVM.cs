using EduSphere.Models;

namespace EduSphere.ViewModel
{
    public class StudentsVM
    {
        public Student Student { get; set; } = new Student();

        public IEnumerable<Student> Students { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
