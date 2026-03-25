namespace OnlineCoursesPlatform.ViewModels
{

    public class CourseListViewModel
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public string CurrencySymbol { get; set; }
        public string InstructorName { get; set; }
        public string InstructorProfilePicture { get; set; }
    }
}

