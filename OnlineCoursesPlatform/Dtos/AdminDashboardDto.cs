using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Dtos
{
    public class AdminDashboardDto
    {
        public int UsersCount { get; set; }
        public int CoursesCount { get; set; }
        public int PendingCoursesCount { get; set; }

        public IEnumerable<User> LatestUsers { get; set; }
        public IEnumerable<Course> PendingCourses { get; set; }

    }
}
