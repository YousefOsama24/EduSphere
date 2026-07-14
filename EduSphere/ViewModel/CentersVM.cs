namespace EduSphere.ViewModel
{
    public class CentersVM
    {
        public Center Center { get; set; } = new Center();

        public IEnumerable<Center> Centers { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
