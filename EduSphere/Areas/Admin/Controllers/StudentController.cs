using EduSphere.Data;
using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EduSphere.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    public class StudentController : Controller
    {
        private readonly IRepository<Student> _context;

        public StudentController(IRepository<Student> context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, string? query = null, CancellationToken cancellationToken = default)
        {
            var students = await _context.GetAsync(
                includes: new Expression<Func<Student, object>>[]
                {
                    s => s.User
                },
                cancellationToken: cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                students = students.Where(e => (e.User.FullName).ToLower().Contains(q));
                ViewBag.Query = query;
            }


            double totalPages = System.Math.Ceiling(students.Count() / 3.0);
            students = students.Skip((page - 1) * 3).Take(3);

            return View(new StudentsVM()
            {
                Students = students.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page,
            });
        }

        public IActionResult Create() => View();
        public IActionResult Update() => View();
        public IActionResult Delete() => View();
    }
}