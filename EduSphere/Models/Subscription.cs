namespace EduSphere.Models
{
    public enum SubscriptionStatus
    {
        Pending,
        Active,
        Expired,
        Cancelled
    }

    public class Subscription
    {
        public int SubscriptionId { get; set; }

        public int CenterId { get; set; }
        public Center? Center { get; set; }

        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SubscriptionStatus Status { get; set; }

        // Navigation

        public ICollection<Payment>? Payments { get; set; }
    }
}