namespace OnlineCoursesPlatform.Dtos;

public class CourseContentActionResultDto : ServiceResult
{
    public bool NotFound { get; init; }
    public bool IsForbidden { get; init; }
}
