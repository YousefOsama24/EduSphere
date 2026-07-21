using EduSphere.Models;
using EduSphere.Repositories.Interfaces;
using EduSphere.Utility;
using EduSphere.ViewModel;
using Microsoft.AspNetCore.Mvc;

[Area(SD.TEACHER_AREA)]
public class HomeController : Controller
{
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Exam> _examRepo;
    private readonly IRepository<Lecture> _lectureRepo;
    private readonly IRepository<Group> _groupRepo;

    public HomeController(
        IRepository<Student> studentRepo,
        IRepository<Exam> examRepo,
        IRepository<Lecture> lectureRepo,
        IRepository<Group> groupRepo)
    {
        _studentRepo = studentRepo;
        _examRepo = examRepo;
        _lectureRepo = lectureRepo;
        _groupRepo = groupRepo;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var students = await _studentRepo.GetAsync(cancellationToken: cancellationToken);
        var exams = await _examRepo.GetAsync(cancellationToken: cancellationToken);
        var lectures = await _lectureRepo.GetAsync(cancellationToken: cancellationToken);
        var groups = await _groupRepo.GetAsync(cancellationToken: cancellationToken);

        var viewModel = new TeacherDashboardVM
        {
            TotalStudents = students.Count(),
            TotalExams = exams.Count(),
            TotalLectures = lectures.Count(),
            TotalGroups = groups.Count(),
            AverageGrade = 85.5 // يمكنك ربطها بجدول النتايج أو الدرجات لديك
        };

        return View(viewModel);
    }
}