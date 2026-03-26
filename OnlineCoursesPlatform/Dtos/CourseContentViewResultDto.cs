using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Dtos;

public class CourseContentViewResultDto
{
    public bool NotFound { get; init; }
    public bool IsForbidden { get; init; }
    public ManageCourseContentViewModel? ViewModel { get; init; }
}
