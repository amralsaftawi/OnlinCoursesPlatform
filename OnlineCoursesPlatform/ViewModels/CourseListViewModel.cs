namespace OnlineCoursesPlatform.ViewModels
{
    public class CourseListViewModel
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/images/default-course.jpg";
        public string CurrencySymbol { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorProfilePicture { get; set; } = "/images/profiles/default-avatar.png";
    }
}
