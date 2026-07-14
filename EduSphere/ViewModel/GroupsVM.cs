namespace EduSphere.ViewModel
{
    public class GroupsVM
    {
        public Group Group { get; set; } = new Group();

        public IEnumerable<Group> Groups { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
