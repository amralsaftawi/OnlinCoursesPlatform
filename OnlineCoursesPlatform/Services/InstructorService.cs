using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Dtos;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.ViewModels;

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

    public async Task<InstructorPublicProfileViewModel?> GetInstructorProfileAsync(int instructorId, int? viewerId, bool viewerIsAdmin)
    {
        var instructor = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == instructorId);

        if (instructor == null)
        {
            return null;
        }

        var canSeeAllCourses = viewerIsAdmin || viewerId == instructorId;

        var coursesQuery = _context.Courses
            .AsNoTracking()
            .Where(course => course.InstructorId == instructorId);

        if (!canSeeAllCourses)
        {
            coursesQuery = coursesQuery.Where(course => course.Status == CourseStatus.Approved);
        }

        var instructorCourses = await coursesQuery
            .OrderByDescending(course => course.Id)
            .Select(course => new CourseListViewModel
            {
                Id = course.Id,
                InstructorId = course.InstructorId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                CategoryName = course.Category != null ? course.Category.Title : "General",
                ImageUrl = course.ImageUrl,
                CurrencySymbol = course.Currency != null ? course.Currency.Symbol : "$",
                InstructorName = $"{instructor.FirstName} {instructor.LastName}".Trim(),
                InstructorProfilePicture = instructor.ProfilePicture ?? "default-avatar.png"
            })
            .ToListAsync();

        return new InstructorPublicProfileViewModel
        {
            InstructorId = instructor.Id,
            InstructorName = $"{instructor.FirstName} {instructor.LastName}".Trim(),
            ProfilePicture = instructor.ProfilePicture,
            TotalCourses = instructorCourses.Count,
            TotalStudents = await _context.Enrollments.CountAsync(enrollment => enrollment.Course.InstructorId == instructorId),
            ShowsAllCourses = canSeeAllCourses,
            Courses = instructorCourses
        };
    }
}
