using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using CenterModel = EduSphere.Models.Center;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace EduSphere.Areas.SupeAdmin.Controllers
{
    [Area(SD.SuperAdmin_AREA)]
    [Authorize(Roles = "SuperAdmin")]
    public class CenterController : Controller
    {

        private readonly IRepository<CenterModel> _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CenterController(IRepository<CenterModel> context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create(CenterModel center, IFormFile LogoFile)
        {
            if (ModelState.IsValid)
            {
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    // توليد اسم فريد للصورة
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Imges", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }

                    // حفظ المسار في الموديل
                    center.LogoUrl = "/images/" + fileName;
                }

                // تعبئة البيانات التلقائية
                center.CreatedAt = DateTime.Now;
                center.IsDeleted = false;

                await _context.CreateAsync(center);
                await _context.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(center);
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
        public async Task<IActionResult> Update(CenterModel Center, IFormFile? LogoFile, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Center);

            // بنجيب السنتر القديم من الداتابيز ومادام tracked: true التعديلات هتسمع فوراً عند الـ Commit
            var oldCenter = await _context.GetOneAsync(
                c => c.CenterId == Center.CenterId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldCenter == null)
                return NotFound();

            // ---- [ لوجيك رفع شعار السنتر الجديد ] ----
            if (LogoFile != null && LogoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "centers");

                // 1. مسح الصورة القديمة من السيرفر لو موجودة فعلاً
                if (!string.IsNullOrEmpty(oldCenter.LogoUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldCenter.LogoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 2. إنشاء الفولدر لو مش موجود
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 3. توليد اسم فريد وحفظ الصورة الجديدة
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(LogoFile.FileName);
                string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(stream);
                }

                // 4. تحديث مسار الصورة بالجديد
                oldCenter.LogoUrl = "/images/centers/" + uniqueFileName;
            }
            // ملحوظة: لو مرفعش صورة جديدة، مش هندخل الـ if دي، وبالتالي oldCenter.LogoUrl هيفضل محتفظ بقيمته القديمة السليمة!

            // ---- [ مابينج بقية البيانات ] ----
            oldCenter.Name = Center.Name;
            oldCenter.Description = Center.Description;
            oldCenter.Address = Center.Address;
            oldCenter.Phone = Center.Phone;
            oldCenter.Email = Center.Email;

            // تاريخ الإنشاء يفضل ثابت وميتغيرش أبداً في التعديل
            // oldCenter.CreatedAt = oldCenter.CreatedAt; 

            // بنسجل تاريخ التعديل اللحظة دي تلقائي
            oldCenter.UpdatedAt = DateTime.Now;
            oldCenter.IsDeleted = Center.IsDeleted;

            // حفظ التغييرات في الداتابيز
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
