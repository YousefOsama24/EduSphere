using Microsoft.EntityFrameworkCore;
using EduSphere.Data;
using EduSphere.Models;
using EduSphere.Repositories.Interfaces;

namespace EduSphere.Repositories.Implementations;

public class CourseRepository
    : GenericRepository<Course>,
      ICourseRepository
{
    public CourseRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }


    public async Task<IEnumerable<Course>>
        GetCoursesByTeacherIdAsync(
            int teacherId)
    {
        return await _context.Courses
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync();
    }
    public async Task<IEnumerable<Course>> GetCoursesByCenterIdAsync(int centerId)
    {
        return await _context.Courses
            .Where(c => c.CenterId == centerId)
            .ToListAsync();
    }
    public async Task<Course?>
        GetCourseWithDetailsAsync(
            int courseId)
    {
        return await _context.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Groups)
            .Include(c => c.Lectures)
            .Include(c => c.Exams)
            .FirstOrDefaultAsync(
                c => c.CourseId == courseId);
    }
    public async Task<int> GetEnrolledStudentsCountAsync(int courseId)
    {
        return await _context.Enrollments
            .CountAsync(e => e.Group != null && e.Group.CourseId == courseId);
    }
}