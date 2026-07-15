using EduSphere.Models;
namespace EduSphere.ViewModel
{
    public class ChoicesVM
    {
        public Choice Choice { get; set; } = new Choice();

        public IEnumerable<Choice> Choices { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
