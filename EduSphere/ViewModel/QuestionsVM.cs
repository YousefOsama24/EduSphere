namespace EduSphere.ViewModel
{
    public class QuestionsVM
    {
        public Question Question { get; set; } = new Question();

        public IEnumerable<Question> Questions { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
