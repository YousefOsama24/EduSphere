using EduSphere.Models;
namespace EduSphere.ViewModel
{
    public class CoursesVM
    {
        public Course Course { get; set; } = new Course();

        public IEnumerable<Course> Courses { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
