using System;

namespace OnlineCoursesPlatform.ViewModels
{
    public class CourseDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Language { get; set; }
        public int TotalDuration { get; set; }

        // Mapped from relations
        public string CategoryName { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public int InstructorId { get; set; }
        public string InstructorName { get; set; }
        public string InstructorProfilePicture { get; set; }
        public string Level { get; set; }
        public string Status { get; set; }
        public int? FirstLessonId { get; set; }

        public int SectionCount { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }

        public int TotalLessons { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
        public List<SectionDetailsViewModel> Sections { get; set; } = new List<SectionDetailsViewModel>();
        public List<ReviewDetailsViewModel> Reviews { get; set; } = new List<ReviewDetailsViewModel>();

        public bool IsEnrolled { get; set; }
        public bool IsOwner { get; set; }
        public int ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public bool CanReview { get; set; }
        public bool HasReviewed { get; set; }

       
    }



  

    
}
