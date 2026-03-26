namespace OnlineCoursesPlatform.Dtos;

public class LessonEditorResultDto
{
    public bool NotFound { get; init; }
    public bool IsForbidden { get; init; }
    public int LessonId { get; init; }
    public int SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Type { get; init; }
    public string ContentUrl { get; init; } = string.Empty;
    public int Duration { get; init; }
    public bool IsFree { get; init; }
}
