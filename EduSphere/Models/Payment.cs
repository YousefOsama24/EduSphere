namespace EduSphere.Models
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public enum PaymentMethod
    {
        Cash,
        Visa,
        MasterCard,
        Paymob,
        VodafoneCash,
        Instapay
    }

    public class Payment
    {
        public int PaymentId { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; }

        public string TransactionReference { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}