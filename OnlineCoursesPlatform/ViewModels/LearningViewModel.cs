namespace OnlineCoursesPlatform.ViewModels;

public class LearningViewModel
{
    [System.ComponentModel.DataAnnotations.Display(Name = "Course Id")]
    public int CourseId { get; set; }
    public LessonDetailsViewModel CurrentLesson { get; set; } = new();
    public List<LessonDetailsViewModel> CourseLessons { get; set; } = new();
    [System.ComponentModel.DataAnnotations.Display(Name = "Course Title")]
    public string CourseTitle { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Display(Name = "Progress Percentage")]
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOwnerPreview { get; set; }
    public bool CanTrackProgress { get; set; }
    [System.ComponentModel.DataAnnotations.Display(Name = "Completed Lessons")]
    public int CompletedLessons { get; set; }
    [System.ComponentModel.DataAnnotations.Display(Name = "Total Lessons")]
    public int TotalLessons { get; set; }
}
