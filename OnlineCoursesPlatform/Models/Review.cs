namespace OnlineCoursesPlatform.Models;

public class Review
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
