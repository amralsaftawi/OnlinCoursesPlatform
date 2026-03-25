using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Dtos;
using OnlinCoursesPlatform.Data;

namespace OnlinCoursePlatform.Services;

public class InstructorService(AppDbContext context) : IInstructorService
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<InstructorCourseDto>> GetInstructorCoursesAsync(int instructorId)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Currency)
            .Include(c => c.CourseTags)
                .ThenInclude(ct => ct.Tag)
            .Where(c => c.InstructorId == instructorId)
            .Select(c => new InstructorCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Price = c.Price,
                CategoryName = c.Category.Title,
                EnrollmentsCount = c.Enrollments.Count(),
                Status = c.Status.ToString(),
                ImageUrl = c.ImageUrl,
                CurrencySymbol = c.Currency != null ? c.Currency.Symbol : "$",
                SectionCount = c.Sections.Count(),
                LessonCount = c.Sections.SelectMany(s => s.Lessons).Count(),
                TotalDuration = c.Sections.SelectMany(s => s.Lessons).Sum(l => (int?)l.Duration) ?? 0,
                Tags = c.CourseTags.Where(ct => ct.Tag != null).Select(ct => ct.Tag!.Name).ToList()
            })
            .ToListAsync();
    }

    public async Task<InstructorStatsDto> GetInstructorStatsAsync(int instructorId)
    {
        var courses = _context.Courses.Where(c => c.InstructorId == instructorId);

        return new InstructorStatsDto
        {
            TotalCourses = await courses.CountAsync(),
            TotalStudents = await _context.Enrollments.CountAsync(e => e.Course.InstructorId == instructorId),
            TotalRevenue = await _context.Enrollments
                .Where(e => e.Course.InstructorId == instructorId)
                .SumAsync(e => (decimal?)e.Course.Price) ?? 0
        };
    }
}
