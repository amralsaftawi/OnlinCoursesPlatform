using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.Models;

public class Lesson
{
    public int Id { get; set; }

    public int SectionId { get; set; }
    public Section Section { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string ContentUrl { get; set; } = string.Empty;

    public int Duration { get; set; }

    public int OrderIndex { get; set; }

    public LessonType Type { get; set; }

    public bool IsFree { get; set; }

    public ICollection<UserProgress> UserProgresses { get; set; } = [];
}
