namespace OnlineCoursesPlatform.Models;

public class Currency
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = [];
}
