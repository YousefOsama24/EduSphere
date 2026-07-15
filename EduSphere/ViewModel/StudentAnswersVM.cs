using EduSphere.Models;
namespace EduSphere.ViewModel
{
    public class StudentAnswersVM
    {
        public StudentAnswer StudentAnswer { get; set; } = new StudentAnswer();

        public IEnumerable<StudentAnswer> StudentAnswers { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
