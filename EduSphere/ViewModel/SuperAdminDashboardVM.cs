namespace EduSphere.ViewModel
{
    public class SuperAdminDashboardVM
    {
        // الكروت الرئيسية (Statistics)
        public int TotalTeachers { get; set; }
        public int TotalCenters { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalRevenue { get; set; }

        // قائمة بآخر عمليات الدفع والإشعارات
        public IEnumerable<PaymentItemVM> RecentPayments { get; set; } = new List<PaymentItemVM>();
        public IEnumerable<NotificationItemVM> Notifications { get; set; } = new List<NotificationItemVM>();
    }

    public class PaymentItemVM
    {
        public int PaymentId { get; set; }
        public string UserOrCenterName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public class NotificationItemVM
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = "info"; // info, warning, success
    }
}