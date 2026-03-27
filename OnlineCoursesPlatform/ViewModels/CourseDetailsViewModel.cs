namespace OnlineCoursesPlatform.ViewModels
{
    public class CourseDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "/images/default-course.jpg";
        public string Language { get; set; } = string.Empty;
        public int TotalDuration { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string InstructorEmail { get; set; } = string.Empty;
        public string InstructorProfilePicture { get; set; } = "/images/profiles/default-avatar.png";
        public string Level { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? FirstLessonId { get; set; }
        public int SectionCount { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public int TotalLessons { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<SectionDetailsViewModel> Sections { get; set; } = new();
        public List<ReviewDetailsViewModel> Reviews { get; set; } = new();
        public bool IsEnrolled { get; set; }
        public bool IsOwner { get; set; }
        public int ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public bool CanReview { get; set; }
        public bool HasReviewed { get; set; }
        public AddReviewViewModel ReviewForm { get; set; } = new();
    }
}
