using OnlineCoursesPlatform.Dtos;

namespace OnlineCoursesPlatform.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync();

        Task<IEnumerable<AdminUserDto>> GetUsersAsync();
        Task MakeAdminAsync(int userId);
        Task DeleteUserAsync(int userId);

        Task<IEnumerable<AdminCourseDto>> GetCoursesAsync();
        Task ApproveCourseAsync(int courseId);
        Task RejectCourseAsync(int courseId);
    }
}
