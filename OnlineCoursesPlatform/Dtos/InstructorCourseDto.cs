namespace OnlinCoursePlatform.Dtos;

public class InstructorCourseDto
{
 
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EnrollmentsCount { get; set; } // عدد الطلاب المشتركين
    public string Status { get; set; } = string.Empty; // Published, Draft, etc.
    public string ImageUrl { get; set; } = string.Empty;
}
    
