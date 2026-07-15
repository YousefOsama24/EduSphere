using EduSphere.Models;
namespace EduSphere.ViewModel
{
    public class TeachersVM
    {
        public Teacher Teacher { get; set; } = new Teacher();

        public IEnumerable<Teacher> Teachers { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
