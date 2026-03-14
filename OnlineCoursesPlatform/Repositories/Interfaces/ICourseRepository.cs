using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.ViewModels;
namespace OnlineCoursesPlatform.Repositories.Interface
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
        Task<Course> GetCourseWithDetailsAsync(int id);
        Task<IEnumerable<Course>> GetCoursesWithDetailsAsync();
    }
}
