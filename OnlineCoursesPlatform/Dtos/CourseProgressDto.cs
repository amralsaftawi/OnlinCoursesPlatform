namespace OnlineCoursesPlatform.Dtos;

public class CourseProgressDto
{
    public int CourseId { get; init; }
    public int TotalLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int ProgressPercentage { get; init; }
    public int? ContinueLessonId { get; init; }
    public bool IsCompleted => TotalLessons > 0 && CompletedLessons >= TotalLessons;
}
