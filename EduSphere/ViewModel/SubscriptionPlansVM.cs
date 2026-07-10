namespace EduSphere.ViewModel
{
    public class SubscriptionPlansVM
    {
        public SubscriptionPlan SubscriptionPlan { get; set; } = new SubscriptionPlan();

        public IEnumerable<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
