namespace OnlineCoursesPlatform.ViewModels
{
    public class InstructorPublicProfileViewModel
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public List<CourseListViewModel> Courses { get; set; } = new();
    }
}
