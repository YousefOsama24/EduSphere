using EduSphere.Data;
using EduSphere.Models;
using EduSphere.Repositories;
using EduSphere.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var students = await _context.GetAsync(
                includes: new Expression<Func<StudentModel, object>>[]
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
        [HttpGet]
        public IActionResult Create()
        {
            return View(new StudentModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(StudentModel student, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(student);



            await _context.CreateAsync(student, cancellationToken);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Add Student Successfully";

            return RedirectToAction(nameof(Index));
        }
        // Controller GET: return a tracked entity
        [HttpGet]
        public async Task<IActionResult> Update(int id, CancellationToken cancellationToken = default)
        {
            var student = await _context.GetOneAsync(e => e.StudentId == id, tracked: true, cancellationToken: cancellationToken);

            if (student is null)
                return RedirectToAction("NotFoundPage", SD.HOME_CONTROLLER);

            return View(student);
        }

        // Controller POST: copy into tracked entity and save
        [HttpPost]
        public async Task<IActionResult> Update(StudentModel student, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(student);

            var studentInDb = await _context.GetOneAsync(e => e.StudentId == student.StudentId, tracked: true, cancellationToken: cancellationToken);
            if (studentInDb is null)
                return RedirectToAction("NotFoundPage", SD.HOME_CONTROLLER);

            // copy editable scalars
            studentInDb.UserId = student.UserId;
            studentInDb.CenterId = student.CenterId;
            studentInDb.DateOfBirth = student.DateOfBirth;
            studentInDb.Gender = student.Gender;
            studentInDb.AcademicLevel = student.AcademicLevel;

            _context.Update(studentInDb);
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Update Student Successfully";
            return RedirectToAction(nameof(Index));
        }
        //public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken = default)
        //{
        //    //_context.Categories.Update(student);

        //    //var studentInDB = _context.Categories.Find(id);
        //    var studentInDB = await _context.GetOneAsync(e => e.StudentId == id, cancellationToken: cancellationToken);

        //    if (studentInDB is null)
        //        return RedirectToAction(nameof(HomeController.NotFoundPage), SD.HOME_CONTROLLER);
        //    studentInDB.StudentId = !studentInDB.StudentId;

        //    //_context.SaveChanges();
        //    await _context.CommitAsync(cancellationToken);

        //    return RedirectToAction(nameof(Index));
        //}

       
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            //var student = _context.Categories.Find(id);
            var student = await _context.GetOneAsync(e => e.StudentId == id, cancellationToken: cancellationToken);

            if (student is null)
                return RedirectToAction("NotFoundPage", SD.HOME_CONTROLLER);

            _context.Delete(student);
            //_context.Categories.Remove(student);
            //_context.SaveChanges();
            await _context.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Delete student Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
