namespace EduSphere.ViewModel
{
    public class SubscriptionsVM
    {
        public Subscription Subscription { get; set; } = new Subscription();

        public IEnumerable<Subscription> Subscriptions { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
