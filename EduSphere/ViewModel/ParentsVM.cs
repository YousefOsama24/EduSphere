namespace EduSphere.ViewModel
{
    public class ParentsVM
    {
        public Parent Parent { get; set; } = new Parent();

        public IEnumerable<Parent> Parents { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
