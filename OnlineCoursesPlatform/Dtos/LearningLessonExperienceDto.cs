using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Dtos;

public class LearningLessonExperienceDto
{
    public bool NotFound { get; init; }
    public int? RedirectCourseId { get; init; }
    public LearningViewModel? ViewModel { get; init; }
}
