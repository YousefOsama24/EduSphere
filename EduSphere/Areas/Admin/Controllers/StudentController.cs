using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using StudentModel = EduSphere.Models.Student;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class StudentController : Controller
    {

        private readonly IRepository<StudentModel> _context;

        public StudentController(IRepository<StudentModel> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Students = await _context.GetAsync(
                includes: new Expression<Func<StudentModel, object>>[]
                {
                    s => s.User
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                Students = Students.Where(e => (e.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(Students.Count() / 3.0);
            Students = Students.Skip((page - 1) * 3).Take(3);

            return View(new StudentsVM()
            {
                Students = Students.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new StudentModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(StudentModel Student, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Student);



            await _context.CreateAsync(Student, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Student Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Student = await _context.GetOneAsync(
                c => c.StudentId == id,
                cancellationToken: cancellationToken);

            if (Student == null)
                return NotFound();

            _context.Delete(Student);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
