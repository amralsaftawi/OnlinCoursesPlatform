namespace OnlineCoursesPlatform.Dtos;

public class LessonCompletionResultDto : ServiceResult
{
    public int? NextLessonId { get; init; }
    public int ProgressPercentage { get; init; }
}
