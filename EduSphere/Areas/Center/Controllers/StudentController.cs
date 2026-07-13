using EduSphere.Repositories.IRepositories;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.CENTER_AREA)]
    public class StudentController : Controller
    {
        private readonly IRepository<Student> _studentRepositry;
        public StudentController(IRepository<Student> repository)
        {
            _studentRepositry = repository;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var Students = await _studentRepositry.GetAsync(
                includes: new Expression<Func<Student, object>>[]
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
            return View(new Student());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Student Student, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(Student);



            await _studentRepositry.CreateAsync(Student, cancellationToken);
            await _studentRepositry.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Student Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Update()
        {
            return View(new Student());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var Student = await _studentRepositry.GetOneAsync(
                c => c.StudentId == id,
                cancellationToken: cancellationToken);

            if (Student == null)
                return NotFound();

            _studentRepositry.Delete(Student);
            await _studentRepositry.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Student deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
