namespace EduSphere.ViewModel
{
    public class ExamsVM
    {
        public Exam Exam { get; set; } = new Exam();

        public IEnumerable<Exam> Exams { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
