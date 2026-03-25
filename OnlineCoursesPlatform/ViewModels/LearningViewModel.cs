namespace OnlineCoursesPlatform.ViewModels;

public class LearningViewModel
{
    public int CourseId { get; set; }
    public LessonDetailsViewModel CurrentLesson { get; set; }
    public List<LessonDetailsViewModel> CourseLessons { get; set; } = new();
    public string CourseTitle { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOwnerPreview { get; set; }
}
