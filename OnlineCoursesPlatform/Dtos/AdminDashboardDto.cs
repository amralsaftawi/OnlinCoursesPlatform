using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Dtos
{
    public class AdminDashboardDto
    {
        public int UsersCount { get; set; }
        public int CoursesCount { get; set; }
        public int PendingCoursesCount { get; set; }
        public int ApprovedCoursesCount { get; set; }
        public int RejectedCoursesCount { get; set; }
        public int CategoriesCount { get; set; }
        public int CurrenciesCount { get; set; }
        public int TagsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public IEnumerable<AdminRevenueDto> RevenueByCurrency { get; set; } = Enumerable.Empty<AdminRevenueDto>();
        public IEnumerable<User> LatestUsers { get; set; } = Enumerable.Empty<User>();
        public IEnumerable<Course> PendingCourses { get; set; } = Enumerable.Empty<Course>();
    }
}
