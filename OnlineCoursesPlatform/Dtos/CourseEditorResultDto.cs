using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Dtos
{
    public class CourseEditorResultDto
    {
        public bool NotFound { get; init; }
        public bool IsForbidden { get; init; }
        public EditCourseViewModel? ViewModel { get; init; }
    }
}
