using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.ViewModels;

public class ManageCourseContentViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<Section> Sections { get; set; } = [];
    public int SectionCount => Sections.Count;
    public int LessonCount => Sections.Sum(section => section.Lessons?.Count ?? 0);
    public int TotalDuration => Sections.Sum(section => section.Lessons?.Sum(lesson => lesson.Duration) ?? 0);
}
