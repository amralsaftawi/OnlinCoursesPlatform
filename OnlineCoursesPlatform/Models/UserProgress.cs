namespace OnlineCoursesPlatform.Models;

public class UserProgress
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public bool IsCompleted { get; set; }
}
