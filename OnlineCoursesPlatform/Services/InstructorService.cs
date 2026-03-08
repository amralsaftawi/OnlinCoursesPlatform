using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Dtos;
using OnlinCoursesPlatform.Data;

namespace OnlinCoursePlatform.Services;

public class InstructorService (AppDbContext context): IInstructorService
{
    private readonly AppDbContext _context=context;

public async Task<IEnumerable<InstructorCourseDto>> GetInstructorCoursesAsync(int instructorId)
{
    return await _context.Courses
        .AsNoTracking() // تحسين أداء للقراءة فقط
        .Include(c => c.Category)
        .Where(c => c.InstructorId == instructorId)
        .Select(c => new InstructorCourseDto
        {
            Id = c.Id,
            Title = c.Title,
            Price = c.Price,
            CategoryName = c.Category.Title,
            EnrollmentsCount = c.Enrollments.Count(),
            Status = c.Status.ToString(),
            ImageUrl = c.ImageUrl
        })
        .ToListAsync();
}

    public async Task<InstructorStatsDto> GetInstructorStatsAsync(int instructorId)
    {
        // حساب إحصائيات المدرس
        var courses = _context.Courses.Where(c => c.InstructorId == instructorId);
        
        return new InstructorStatsDto
        {
            TotalCourses = await courses.CountAsync(),
            TotalStudents = await _context.Enrollments
                                .CountAsync(e => e.Course.InstructorId == instructorId),
           TotalRevenue = await _context.Enrollments
                                 .Where(e => e.Course.InstructorId == instructorId)
                                .SumAsync(e => (decimal?)e.Course.Price) ?? 0
        };
    }
}