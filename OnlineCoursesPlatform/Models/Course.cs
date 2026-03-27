using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.Models;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;

    public string ImageUrl { get; set; } = "/images/default-course.jpg";

    public int InstructorId { get; set; }
    public User Instructor { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public CourseStatus Status { get; set; }

    public CourseLevel Level { get; set; }

    public string Language { get; set; } = "English";

    public int TotalDuration { get; set; }

    public ICollection<Section> Sections { get; set; } = [];

    public ICollection<CourseTag> CourseTags { get; set; } = [];

    public ICollection<Enrollment> Enrollments { get; set; } = [];

    public ICollection<Review> Reviews { get; set; } = [];
}
