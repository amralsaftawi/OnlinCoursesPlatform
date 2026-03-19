using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlineCoursesPlatform.Models;
using OnlinCoursePlatform.Abstrctions;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.ViewModels;
namespace OnlineCoursesPlatform.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
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
                throw new ArgumentNullException(nameof(course));

            // Validate required fields
            if (string.IsNullOrWhiteSpace(course.Title))
                throw new ArgumentException("Course title is required", nameof(course.Title));

            if (string.IsNullOrWhiteSpace(course.Description))
                throw new ArgumentException("Course description is required", nameof(course.Description));

            if (course.Price < 0)
                throw new ArgumentException("Course price cannot be negative", nameof(course.Price));

            if (course.CategoryId <= 0)
                throw new ArgumentException("Valid category is required", nameof(course.CategoryId));

            if (course.CurrencyId <= 0)
                throw new ArgumentException("Valid currency is required", nameof(course.CurrencyId));

            if (course.InstructorId <= 0)
                throw new ArgumentException("Instructor must be assigned", nameof(course.InstructorId));

            if (course.TotalDuration <= 0)
                throw new ArgumentException("Course duration must be greater than 0", nameof(course.TotalDuration));

            var createdCourse = await _courseRepository.AddAsync(course);
            await _courseRepository.SaveAsync();
            return createdCourse;
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            if (course == null)
                throw new ArgumentNullException(nameof(course));

            var updatedCourse = await _courseRepository.UpdateAsync(course);
            await _courseRepository.SaveAsync();
            return updatedCourse;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var result = await _courseRepository.DeleteAsync(id);
            if (result)
            {
                await _courseRepository.SaveAsync();
            }
            return result;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _courseRepository.SaveAsync();
        }

        public async Task<(IEnumerable<CourseListViewModel> Courses, int TotalCount)> GetPaginatedCoursesAsync(int pageNumber, int pageSize)
        {
            var query = _courseRepository.GetQueryable();
            
            // 1 - Total count of courses in the database (for pagination metadata)
            var totalCount = await query.CountAsync();

            // 2 - Fetch the paginated courses and project them directly to ViewModel using AutoMapper ✨
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
