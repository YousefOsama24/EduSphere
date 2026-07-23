using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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
        private readonly IRepository<EduSphere.Models.Center> _CenterRepository;
        private readonly UserManager<ApplicationUser> _userManager;


        public StudentController(
            IRepository<Student> studentRepository,
            IRepository<ApplicationUser> userRepository,
            IRepository<Parent> parentRepository,
            IRepository<EduSphere.Models.Center> CenterRepository,
            ILogger<StudentController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _parentRepository = parentRepository;
            _logger = logger;
            _userManager = userManager;
            _CenterRepository = CenterRepository;
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
                    x => x.User
                },
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string search = query.Trim().ToLower();

                students = students.Where(x =>
                    x.User != null && x.User.FullName.ToLower().Contains(search));

                ViewBag.Query = query;
            }

            int totalItems = students.Count();
            double totalPages = Math.Ceiling(totalItems / (double)PageSize);

            var pagedStudents = students
                .Skip((page - 1) * PageSize)
                .Take(PageSize);

            return View(new StudentsVM
            {
                Students = pagedStudents,
                TotalPages = totalPages,
                CurrentPage = page
            });
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Create(
    CancellationToken cancellationToken = default)
        {
            await PopulateDropdownsAsync(cancellationToken);
            return View(new Student());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     Student model,
     CancellationToken cancellationToken = default)
        {
            ModelState.Remove(nameof(Student.Parent));

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                string combinedErrors = string.Join(" | ", validationErrors);

                _logger.LogWarning("Assign Student Validation Failed: {Errors}", combinedErrors);

                TempData["Error"] = $"Validation Failed: {combinedErrors}";

                await PopulateDropdownsAsync(cancellationToken, model.UserId, model.ParentId);
                return View(model);
            }

            try
            {
                // هات الطالب الموجود بالفعل
                var student = (await _studentRepository.GetAsync(
                    x => x.UserId == model.UserId,
                    cancellationToken: cancellationToken))
                    .FirstOrDefault();

                if (student == null)
                {
                    ModelState.AddModelError("", "Student not found.");

                    await PopulateDropdownsAsync(cancellationToken, model.UserId, model.ParentId);
                    return View(model);
                }

                // لو الطالب متضاف بالفعل لسنتر
                if (student.CenterId != 0)
                {
                    ModelState.AddModelError("", "Student is already assigned to a center.");

                    await PopulateDropdownsAsync(cancellationToken, model.UserId, model.ParentId);
                    return View(model);
                }

               

                student.CenterId = 1; student.ParentId = 1;
                student.ParentId = model.ParentId;
                student.AcademicLevel = model.AcademicLevel;
                student.DateOfBirth = model.DateOfBirth;
                student.Gender = model.Gender;

                student.Parent = null!;

                _studentRepository.Update(student);

                await _studentRepository.CommitAsync(cancellationToken);
                await _studentRepository.CommitAsync(cancellationToken);

                TempData["Success"] = "Student assigned to center successfully.";

                _logger.LogInformation(
                    "Student {StudentId} assigned to center {CenterId}.",
                    student.StudentId,
                    student.CenterId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while assigning student.");

                TempData["Error"] = $"Error while assigning student: {ex.Message}";

                await PopulateDropdownsAsync(cancellationToken, model.UserId, model.ParentId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchStudents(
    string term,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            var users = await _userManager.Users
                .Where(x =>
                    x.UserType == UserType.Student &&
                    x.FullName.Contains(term))
                .OrderBy(x => x.FullName)
                .Take(20)
                .Select(x => new
                {
                    id = x.Id,
                    text = x.FullName
                })
                .ToListAsync(cancellationToken);

            return Json(users);
        }
        #region Edit

        [HttpGet]
        public async Task<IActionResult> Update(
            int id,
            CancellationToken cancellationToken = default)
        {
            var student = await _studentRepository.GetOneAsync(
                x => x.StudentId == id,
                tracked: false,
                cancellationToken: cancellationToken);

            if (student == null)
                return NotFound();

            await PopulateDropdownsAsync(cancellationToken, student.UserId, student.ParentId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Student student,
            CancellationToken cancellationToken = default)
        {
            ModelState.Remove(nameof(Student.User));
            ModelState.Remove(nameof(Student.Parent));

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                string combinedErrors = string.Join(" | ", validationErrors);
                _logger.LogWarning("Update Student Validation Failed: {Errors}", combinedErrors);
                TempData["Error"] = $"Validation Failed: {combinedErrors}";

                await PopulateDropdownsAsync(cancellationToken, student.UserId, student.ParentId);
                return View(student);
            }

            // التأكد من أن الحساب غير مستخدم مع طالب آخر
            bool isUserTakenByAnotherStudent = (await _studentRepository.GetAsync(
                x => x.UserId == student.UserId && x.StudentId != student.StudentId,
                cancellationToken: cancellationToken)).Any();

            if (isUserTakenByAnotherStudent)
            {
                ModelState.AddModelError("", "This user account is already assigned to another student.");
                await PopulateDropdownsAsync(cancellationToken, student.UserId, student.ParentId);
                return View(student);
            }

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

                TempData["Success"] = "Student updated successfully.";
                _logger.LogInformation("Student updated successfully. Id: {Id}", student.StudentId);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating student. Id: {Id}", student.StudentId);
                TempData["Error"] = $"Something went wrong while updating: {ex.Message}";

                await PopulateDropdownsAsync(cancellationToken, student.UserId, student.ParentId);
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

                TempData["Success"] = "Student deleted successfully.";
                _logger.LogInformation("Student deleted successfully. Id: {Id}", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting student. Id: {Id}", id);
                TempData["Error"] = "Something went wrong while deleting the student.";

                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Helper Methods

        private async Task PopulateDropdownsAsync(
            CancellationToken cancellationToken = default,
            string? selectedUserId = null,
            int? selectedParentId = null)
        {
            var studentUsers = await _userRepository.GetAsync(
                x => x.UserType == UserType.Student,
                cancellationToken: cancellationToken);

            var parents = await _parentRepository.GetAsync(
                includes: new Expression<Func<Parent, object>>[] { x => x.User },
                cancellationToken: cancellationToken);

            ViewBag.Users = new SelectList(studentUsers, "Id", "FullName", selectedUserId);

            ViewBag.Parents = parents.Select(x => new SelectListItem
            {
                Value = x.ParentId.ToString(),
                Text = x.User != null ? x.User.FullName : $"Parent #{x.ParentId}",
                Selected = selectedParentId.HasValue && x.ParentId == selectedParentId.Value
            }).ToList();
        }

        #endregion
    }
}