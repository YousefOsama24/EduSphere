using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class StudentController : Controller
    {
        private const int PageSize = 10;

        private readonly IRepository<Student> _studentRepository;
        private readonly ILogger<StudentController> _logger;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<Parent> _parentRepository;

        public StudentController(
    IRepository<Student> studentRepository,
    IRepository<ApplicationUser> userRepository,
    IRepository<Parent> parentRepository,
    ILogger<StudentController> logger)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _parentRepository = parentRepository;
            _logger = logger;
        }

        #region Index

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var students = await _studentRepository.GetAsync(
                includes: new Expression<Func<Student, object>>[]
                {
                   x => x.User,
                   x => x.Parent,
                   x => x.Parent.User
                },
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();

                students = students.Where(x =>
                    x.User.FullName.ToLower().Contains(search));

                ViewBag.Query = query;
            }

            double totalPages =
                Math.Ceiling(students.Count() / (double)PageSize);

            students = students
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new StudentsVM
            {
                Students = students.AsEnumerable(),
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<IActionResult> Create(
    CancellationToken cancellationToken = default)
        {
           

            var parents = await _parentRepository.GetAsync(
                includes: new Expression<Func<Parent, object>>[]
                {
            x => x.User
                },
                cancellationToken: cancellationToken);

            
           
            ViewBag.Parents = parents
     .Select(x => new SelectListItem
     {
         Value = x.ParentId.ToString(),
         Text = x.User != null
             ? x.User.FullName
             : $"Parent #{x.ParentId}"
     })
     .ToList();

            return View(new Student());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     Student student,
     CancellationToken cancellationToken = default)
        {
            

            var parents = await _parentRepository.GetAsync(
                includes: new Expression<Func<Parent, object>>[]
                {
            x => x.User
                },
                cancellationToken: cancellationToken);

            if (!ModelState.IsValid)
            {
               

                ViewBag.Parents = parents
    .Select(x => new SelectListItem
    {
        Value = x.ParentId.ToString(),
        Text = x.User != null
            ? x.User.FullName
            : $"Parent #{x.ParentId}"
    })
    .ToList();

                return View(student);
            }

            bool exists = (
                await _studentRepository.GetAsync(
                    x => x.UserId == student.UserId,
                    cancellationToken: cancellationToken))
                .Any();
            var user = await _userRepository.GetOneAsync(
    x => x.Id == student.UserId,
    cancellationToken: cancellationToken);

            if (user == null)
            {
                ModelState.AddModelError("UserId", "Invalid Student ID.");

                ViewBag.Parents = parents
                    .Select(x => new SelectListItem
                    {
                        Value = x.ParentId.ToString(),
                        Text = x.User != null
                            ? x.User.FullName
                            : $"Parent #{x.ParentId}"
                    })
                    .ToList();

                return View(student);
            }
            if (exists)
            {
                ModelState.AddModelError(
                    "",
                    "This user is already registered as a student.");

                ViewBag.Users = new SelectList(
                    
                    "Id",
                    "FullName",
                    student.UserId);

                ViewBag.Parents = parents
    .Select(x => new SelectListItem
    {
        Value = x.ParentId.ToString(),
        Text = x.User != null
            ? x.User.FullName
            : $"Parent #{x.ParentId}"
    })
    .ToList();

                return View(student);
            }

            try
            {
                await _studentRepository.CreateAsync(
                    student,
                    cancellationToken);

                await _studentRepository.CommitAsync(
                    cancellationToken);

                TempData["Success"] =
                    "Student created successfully.";

                _logger.LogInformation(
                    "Student created successfully. Id: {Id}",
                    student.StudentId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating student.");

                TempData["Error"] =
                    "Something went wrong while creating the student.";

                ViewBag.Users = new SelectList(
                    
                    "Id",
                    "FullName",
                    student.UserId);

                ViewBag.Parents = parents
    .Select(x => new SelectListItem
    {
        Value = x.ParentId.ToString(),
        Text = x.User != null
            ? x.User.FullName
            : $"Parent #{x.ParentId}"
    })
    .ToList();

                return View(student);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Update(
            int id,
            CancellationToken cancellationToken = default)
        {
            var student = await _studentRepository.GetOneAsync(
                x => x.StudentId == id,
                tracked: true,
                cancellationToken: cancellationToken);

            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Student student,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(student);

            try
            {
                var oldStudent = await _studentRepository.GetOneAsync(
                    x => x.StudentId == student.StudentId,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (oldStudent == null)
                    return NotFound();

                oldStudent.UserId = student.UserId;
                oldStudent.ParentId = student.ParentId;

                await _studentRepository.CommitAsync(cancellationToken);

                TempData["Success"] =
                    "Student updated successfully.";

                _logger.LogInformation(
                    "Student updated successfully. Id: {Id}",
                    student.StudentId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating student. Id: {Id}",
                    student.StudentId);

                TempData["Error"] =
                    "Something went wrong while updating the student.";

                return View(student);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var student = await _studentRepository.GetOneAsync(
                    x => x.StudentId == id,
                    cancellationToken: cancellationToken);

                if (student == null)
                    return NotFound();

                _studentRepository.Delete(student);

                await _studentRepository.CommitAsync(cancellationToken);

                TempData["Success"] =
                    "Student deleted successfully.";

                _logger.LogInformation(
                    "Student deleted successfully. Id: {Id}",
                    id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting student. Id: {Id}",
                    id);

                TempData["Error"] =
                    "Something went wrong while deleting the student.";

                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}
