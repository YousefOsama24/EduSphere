namespace EduSphere.Models
{
    public enum PlanTier
    {
        Free,
        Basic,
        Premium
    }

    public class SubscriptionPlan
    {
        public int SubscriptionPlanId { get; set; }

        public string Name { get; set; } = string.Empty;

        public PlanTier Tier { get; set; }

        public decimal Price { get; set; }

        public int DurationInMonths { get; set; }

        public int MaxTeachers { get; set; }

        public int MaxStudents { get; set; }

        public string Features { get; set; } = string.Empty;

        // Navigation

        public ICollection<Subscription>? Subscriptions { get; set; }
    }
}