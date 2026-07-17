using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using PaymentModel = EduSphere.Models.Payment;
using EduSphere.ViewModel;
namespace EduSphere.Areas.SuperAdmin.Controllers
{
    [Area(SD.SuperAdmin_AREA)]
    [Authorize(Roles = "SuperAdmin")]
    public class PaymentController : Controller
    {

        private readonly IRepository<PaymentModel> _context;

        public PaymentController(IRepository<PaymentModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Payments = await _context.GetAsync(
                includes: new Expression<Func<PaymentModel, object>>[]
                {
                    s => s.Subscription
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Payments = Payments.Where(e => e.TransactionReference != null && e.TransactionReference.ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Payments.Count() / 10.0);
            Payments = Payments.Skip((page - 1) * 10).Take(10);

            return View(new PaymentsVM()
            {
                Payments = Payments.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PaymentModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentModel Payment, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Payment);

            await _context.CreateAsync(Payment, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Payment added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Payment = await _context.GetOneAsync(
                c => c.PaymentId == id,
                cancellationToken: cancellationToken);

            if (Payment == null)
                return NotFound();

            return View(Payment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(PaymentModel Payment, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Payment);

            var oldPayment = await _context.GetOneAsync(
                c => c.PaymentId == Payment.PaymentId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldPayment == null)
                return NotFound();

            oldPayment.SubscriptionId = Payment.SubscriptionId;
            oldPayment.Amount = Payment.Amount;
            oldPayment.PaymentMethod = Payment.PaymentMethod;
            oldPayment.Status = Payment.Status;
            oldPayment.TransactionReference = Payment.TransactionReference;
            oldPayment.PaymentDate = Payment.PaymentDate;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Payment updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Payment = await _context.GetOneAsync(
                c => c.PaymentId == id,
                cancellationToken: cancellationToken);

            if (Payment == null)
                return NotFound();

            _context.Delete(Payment);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Payment deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
