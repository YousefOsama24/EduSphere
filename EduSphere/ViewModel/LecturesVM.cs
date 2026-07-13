namespace EduSphere.ViewModel
{
    public class LecturesVM
    {
        public Lecture Lecture { get; set; } = new Lecture();

        public IEnumerable<Lecture> Lectures { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
