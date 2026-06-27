using EduSphere.Models;

namespace EduSphere.Repositories.Interfaces;

public interface ICourseRepository
    : IGenericRepository<Course>
{
    Task<IEnumerable<Course>>
        GetCoursesByTeacherIdAsync(int teacherId);

    Task<IEnumerable<Course>>
        GetCoursesByCenterIdAsync(int centerId);

    Task<Course?>
        GetCourseWithDetailsAsync(int courseId);

    Task<int>
        GetEnrolledStudentsCountAsync(int courseId);
}