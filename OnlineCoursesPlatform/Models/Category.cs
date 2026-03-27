namespace OnlineCoursesPlatform.Models;

public class Category
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = [];
}
