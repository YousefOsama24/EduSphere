using EduSphere.Models;
namespace EduSphere.ViewModel
{
    public class PaymentsVM
    {
        public Payment Payment { get; set; } = new Payment();

        public IEnumerable<Payment> Payments { get; set; } = null!;

        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
