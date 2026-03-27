namespace OnlineCoursesPlatform.ViewModels
{
    public class InstructorPublicProfileViewModel
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = "default-avatar.png";
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public bool ShowsAllCourses { get; set; }
        public List<CourseListViewModel> Courses { get; set; } = new();
    }
}
