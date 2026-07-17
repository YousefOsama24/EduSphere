using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using CenterModel = EduSphere.Models.Center;
using EduSphere.ViewModel;

namespace EduSphere.Areas.SupeAdmin.Controllers
{
    [Area(SD.SuperAdmin_AREA)]
    [Authorize(Roles = "SuperAdmin")]
    public class CenterController : Controller
    {

        private readonly IRepository<CenterModel> _context;

        public CenterController(IRepository<CenterModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Centers = await _context.GetAsync(
                includes: new Expression<Func<CenterModel, object>>[]
                {
                    s => s.Teachers
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Centers = Centers.Where(e => (e.Name).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Centers.Count() / 10.0);
            Centers = Centers.Skip((page - 1) * 10).Take(10);

            return View(new CentersVM()
            {
                Centers = Centers.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CenterModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CenterModel Center, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Center);

            await _context.CreateAsync(Center, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Center added successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var Center = await _context.GetOneAsync(
                c => c.CenterId == id,
                cancellationToken: cancellationToken);

            if (Center == null)
                return NotFound();

            return View(Center);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CenterModel Center, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Center);

            var oldCenter = await _context.GetOneAsync(
                c => c.CenterId == Center.CenterId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldCenter == null)
                return NotFound();

            oldCenter.Name = Center.Name;
            oldCenter.Description = Center.Description;
            oldCenter.Address = Center.Address;
            oldCenter.Phone = Center.Phone;
            oldCenter.Email = Center.Email;
            oldCenter.LogoUrl = Center.LogoUrl;
            oldCenter.CreatedAt = Center.CreatedAt;
            oldCenter.UpdatedAt = Center.UpdatedAt;
            oldCenter.IsDeleted = Center.IsDeleted;

            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Center updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Center = await _context.GetOneAsync(
                c => c.CenterId == id,
                cancellationToken: cancellationToken);

            if (Center == null)
                return NotFound();

            _context.Delete(Center);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Center deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
