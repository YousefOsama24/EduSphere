using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduSphere.Data; // غير ده لاسم الفولدر بتاع الـ DbContext عندك
using EduSphere.Models;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض كل المدرسين (Index)
        public async Task<IActionResult> Index()
        {
            // بنعمل Include للـ User والـ Center عشان نعرض أساميهم مش مجرد IDs
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Center)
                .ToListAsync();

            return View(teachers);
        }

        // 2. عرض تفاصيل مدرس معين (Details)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Center)
                .FirstOrDefaultAsync(m => m.TeacherId == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // 3. صفحة إضافة مدرس جديد - GET
        public IActionResult Create()
        {
            // بنجهز لستة السناتر والمستخدمين عشان نختار منهم في الـ Dropdown View
            ViewData["CenterId"] = new SelectList(_context.Centers, "CenterId", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // 4. حفظ المدرس الجديد - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,UserId,CenterId,Specialization,HireDate,IsActive")] EduSphere.Models.Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CenterId"] = new SelectList(_context.Centers, "CenterId", "Name", teacher.CenterId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", teacher.UserId);
            return View(teacher);
        }

        // 5. صفحة التعديل - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            ViewData["CenterId"] = new SelectList(_context.Centers, "CenterId", "Name", teacher.CenterId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", teacher.UserId);
            return View(teacher);
        }

        // 6. حفظ التعديلات - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeacherId,UserId,CenterId,Specialization,HireDate,IsActive")] EduSphere.Models.Teacher teacher)
        {
            if (id != teacher.TeacherId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.TeacherId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CenterId"] = new SelectList(_context.Centers, "CenterId", "Name", teacher.CenterId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", teacher.UserId);
            return View(teacher);
        }

        // 7. صفحة الحذف أو إيقاف التفعيل - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Center)
                .FirstOrDefaultAsync(m => m.TeacherId == id);

            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // 8. تأكيد الحذف - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                // نصيحة: لو السيستم شغال وفي داتا مربوطة بالمدرس، يفضل تخلي IsActive = false بدل الحذف الفعلي
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
    }
}