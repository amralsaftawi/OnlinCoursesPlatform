using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public CourseService(ICourseRepository courseRepository, IMapper mapper, AppDbContext context)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }

        public async Task<Course> GetCourseWithDetailsAsync(int id)
        {
            return await _courseRepository.GetCourseWithDetailsAsync(id);
        }

        public async Task<CourseDetailsViewModel> GetCourseDetailsProjectedAsync(int id)
        {
            return await _courseRepository.GetQueryable()
                .Where(c => c.Id == id)
                .ProjectTo<CourseDetailsViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesWithDetailsAsync()
        {
            return await _courseRepository.GetCoursesWithDetailsAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _courseRepository.GetCoursesByInstructorAsync(instructorId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _courseRepository.GetCoursesByCategoryAsync(categoryId);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            if (string.IsNullOrWhiteSpace(course.Title))
            {
                throw new ArgumentException("Course title is required", nameof(course.Title));
            }

            if (string.IsNullOrWhiteSpace(course.Description))
            {
                throw new ArgumentException("Course description is required", nameof(course.Description));
            }

            if (course.Price < 0)
            {
                throw new ArgumentException("Course price cannot be negative", nameof(course.Price));
            }

            if (course.CategoryId <= 0)
            {
                throw new ArgumentException("Valid category is required", nameof(course.CategoryId));
            }

            if (course.CurrencyId <= 0)
            {
                throw new ArgumentException("Valid currency is required", nameof(course.CurrencyId));
            }

            if (course.InstructorId <= 0)
            {
                throw new ArgumentException("Instructor must be assigned", nameof(course.InstructorId));
            }

            if (course.TotalDuration < 0)
            {
                throw new ArgumentException("Course duration cannot be negative", nameof(course.TotalDuration));
            }

            var createdCourse = await _courseRepository.AddAsync(course);
            await _courseRepository.SaveAsync();
            return createdCourse;
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            var updatedCourse = await _courseRepository.UpdateAsync(course);
            await _courseRepository.SaveAsync();
            return updatedCourse;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.CourseTags)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return false;
            }

            var lessonIds = course.Sections
                .SelectMany(section => section.Lessons)
                .Select(lesson => lesson.Id)
                .ToList();

            if (lessonIds.Count != 0)
            {
                var progresses = await _context.UserProgresses
                    .Where(progress => lessonIds.Contains(progress.LessonId))
                    .ToListAsync();

                if (progresses.Count != 0)
                {
                    _context.UserProgresses.RemoveRange(progresses);
                }
            }

            if (course.Reviews.Any())
            {
                _context.Reviews.RemoveRange(course.Reviews);
            }

            if (course.Enrollments.Any())
            {
                _context.Enrollments.RemoveRange(course.Enrollments);
            }

            if (course.CourseTags.Any())
            {
                _context.CourseTags.RemoveRange(course.CourseTags);
            }

            var lessons = course.Sections.SelectMany(section => section.Lessons).ToList();
            if (lessons.Count != 0)
            {
                _context.Lessons.RemoveRange(lessons);
            }

            if (course.Sections.Any())
            {
                _context.Sections.RemoveRange(course.Sections);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _courseRepository.SaveAsync();
        }

        public async Task<(IEnumerable<CourseListViewModel> Courses, int TotalCount)> GetPaginatedCoursesAsync(int pageNumber, int pageSize)
        {
            var query = _courseRepository.GetQueryable();
            var totalCount = await query.CountAsync();

            var courses = await query
                .OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CourseListViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (courses, totalCount);
        }
    }
}
