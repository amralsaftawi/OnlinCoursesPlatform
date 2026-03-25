namespace OnlineCoursesPlatform.ViewModels;

public class LearningViewModel
{
    public int CourseId { get; set; }
    public LessonDetailsViewModel CurrentLesson { get; set; } = new();
    public List<LessonDetailsViewModel> CourseLessons { get; set; } = new();
    public string CourseTitle { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOwnerPreview { get; set; }
    public bool CanTrackProgress { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
}
