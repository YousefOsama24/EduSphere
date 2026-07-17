using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using TeacherModel = EduSphere.Models.Teacher;

namespace EduSphere.Areas.Center.Controllers
{
    [Area(SD.Center_AREA)]
    [Authorize(Roles = "CenterManager,SuperAdmin")]
    public class TeachersController : Controller
    {
        private readonly IRepository<TeacherModel> _teacherRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<EduSphere.Models.Center> _centerRepository;

        public TeachersController(
            IRepository<TeacherModel> teacherRepository,
            IRepository<ApplicationUser> userRepository,
            IRepository<EduSphere.Models.Center> centerRepository)
        {
            _teacherRepository = teacherRepository;
            _userRepository = userRepository;
            _centerRepository = centerRepository;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken = default)
        {
            var teachers = await _teacherRepository.GetAsync(
                includes: new Expression<Func<TeacherModel, object>>[]
                {
                    x => x.User,
                    x => x.Center
                },
                cancellationToken: cancellationToken);

            return View(teachers);
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            CancellationToken cancellationToken = default)
        {
            await PopulateListsAsync(cancellationToken);

            return View(new TeacherModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TeacherModel teacher,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                await PopulateListsAsync(cancellationToken);

                return View(teacher);
            }

            bool exists = await _teacherRepository.AnyAsync(
                x => x.UserId == teacher.UserId,
                cancellationToken);

            if (exists)
            {
                ModelState.AddModelError(
                    "",
                    "This user is already registered as a teacher.");

                await PopulateListsAsync(cancellationToken);

                return View(teacher);
            }

            await _teacherRepository.CreateAsync(
                teacher,
                cancellationToken);

            await _teacherRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Teacher created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken = default)
        {
            var teacher = await _teacherRepository.GetOneAsync(
                x => x.TeacherId == id,
                includes: new Expression<Func<EduSphere.Models.Teacher, object>>[]
                {
                    x => x.User
                },
                cancellationToken: cancellationToken);

            if (teacher == null)
                return NotFound();

            await PopulateListsAsync(cancellationToken);

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            TeacherModel teacher,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                await PopulateListsAsync(cancellationToken);

                return View(teacher);
            }

            var oldTeacher = await _teacherRepository.GetOneAsync(
                x => x.TeacherId == teacher.TeacherId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (oldTeacher == null)
                return NotFound();

            oldTeacher.UserId = teacher.UserId;
            oldTeacher.CenterId = teacher.CenterId;
            oldTeacher.Specialization = teacher.Specialization;
            oldTeacher.HireDate = teacher.HireDate;
            oldTeacher.IsActive = teacher.IsActive;

            await _teacherRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Teacher updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            var teacher = await _teacherRepository.GetOneAsync(
                x => x.TeacherId == id,
                cancellationToken: cancellationToken);

            if (teacher == null)
                return NotFound();

            _teacherRepository.Delete(teacher);

            await _teacherRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Teacher deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateListsAsync(
            CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAsync(
                x => x.UserType == UserType.Teacher,
                cancellationToken: cancellationToken);

            var centers = await _centerRepository.GetAsync(
                cancellationToken: cancellationToken);

            ViewBag.UserId = new SelectList(
                users,
                "Id",
                "FullName");

            ViewBag.CenterId = new SelectList(
                centers,
                "CenterId",
                "Name");
        }
    }
}
