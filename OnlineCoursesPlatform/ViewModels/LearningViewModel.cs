using OnlineCoursesPlatform.Models;

 namespace OnlineCoursesPlatform.ViewModels;

public class LearningViewModel
{
    public LessonDetailsViewModel CurrentLesson { get; set; }
    public List<LessonDetailsViewModel> CourseLessons { get; set; } = new();
    public string CourseTitle { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; } // عشان نتحكم في شكل الزرار
}