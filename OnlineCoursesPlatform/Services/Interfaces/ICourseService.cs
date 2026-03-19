using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services.Interfaces
{
    public interface ICourseService
    {
        Task<(IEnumerable<CourseListViewModel> Courses, int TotalCount)> GetPaginatedCoursesAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course> GetCourseByIdAsync(int id);
        Task<Course> GetCourseWithDetailsAsync(int id);
        Task<CourseDetailsViewModel> GetCourseDetailsProjectedAsync(int id);
        Task<IEnumerable<Course>> GetCoursesWithDetailsAsync();
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
        Task<Course> CreateCourseAsync(Course course);
        Task<Course> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
