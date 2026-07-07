namespace EduSphere.ViewModel
{
    public class StudentsVM
    {
        public IEnumerable<Student> Students { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
