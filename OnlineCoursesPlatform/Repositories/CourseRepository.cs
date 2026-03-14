using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _dbSet
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Currency)
                .Include(c => c.Sections)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(c => c.CategoryId == categoryId)
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Currency)
                .ToListAsync();
        }

        public async Task<Course> GetCourseWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Currency)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.Reviews)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Course>> GetCoursesWithDetailsAsync()
        {
            return await _dbSet
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Currency)
                .Include(c => c.Sections)
                .ToListAsync();
        }



    }
}
