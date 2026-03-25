using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<IEnumerable<AdminUserDto>> GetUsersAsync();
        Task UpdateUserRolesAsync(int userId, string primaryRole, bool isAdmin, int actingAdminId);
        Task DeleteUserAsync(int userId);
        Task<IEnumerable<AdminCourseDto>> GetCoursesAsync();
        Task ApproveCourseAsync(int courseId);
        Task RejectCourseAsync(int courseId);
        Task MoveCourseToPendingAsync(int courseId);
        Task DeleteCourseAsync(int courseId);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Currency>> GetCurrenciesAsync();
        Task<IEnumerable<Tag>> GetTagsAsync();
        Task AddCategoryAsync(string title);
        Task UpdateCategoryAsync(int categoryId, string title);
        Task DeleteCategoryAsync(int categoryId);
        Task AddCurrencyAsync(string name, string code, string symbol);
        Task UpdateCurrencyAsync(int currencyId, string name, string code, string symbol);
        Task DeleteCurrencyAsync(int currencyId);
        Task AddTagAsync(string name);
        Task UpdateTagAsync(int tagId, string name);
        Task DeleteTagAsync(int tagId);
    }
}
