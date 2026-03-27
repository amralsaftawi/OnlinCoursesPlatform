namespace OnlineCoursesPlatform.Dtos;

public class InstructorCourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EnrollmentsCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = "$";
    public int SectionCount { get; set; }
    public int LessonCount { get; set; }
    public int TotalDuration { get; set; }
    public List<string> Tags { get; set; } = new();
}
