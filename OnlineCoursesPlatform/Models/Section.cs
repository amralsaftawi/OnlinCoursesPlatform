namespace OnlineCoursesPlatform.Models;

public class Section
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];
}
