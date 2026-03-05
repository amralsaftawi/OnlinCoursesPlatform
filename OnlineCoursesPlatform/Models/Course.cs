using Microsoft.AspNetCore.Mvc.ViewEngines;
using static System.Collections.Specialized.BitVector32;
using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }

        public string ImageUrl { get; set; }

        public int InstructorId { get; set; }
        public User Instructor { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public CourseStatus Status { get; set; }

        public CourseLevel Level { get; set; }

        public string Language { get; set; }

        public int TotalDuration { get; set; }

        public ICollection<Section> Sections { get; set; }

        public ICollection<CourseTag> CourseTags { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }

        public ICollection<Review> Reviews { get; set; }
    }
}
