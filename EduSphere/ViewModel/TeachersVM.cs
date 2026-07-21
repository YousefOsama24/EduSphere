using EduSphere.Models;
using TeacherModel = EduSphere.Models.Teacher;

namespace EduSphere.ViewModel
{
    public class TeachersVM
    {
        public Teacher Teacher { get; set; } = new Teacher();

        public IEnumerable<TeacherModel> Teachers { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
