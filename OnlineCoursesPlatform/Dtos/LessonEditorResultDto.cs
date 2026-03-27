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
    public int? QuizQuestionType { get; init; }
    public string QuizPrompt { get; init; } = string.Empty;
    public string QuizOptionA { get; init; } = string.Empty;
    public string QuizOptionB { get; init; } = string.Empty;
    public string QuizOptionC { get; init; } = string.Empty;
    public string QuizOptionD { get; init; } = string.Empty;
    public int? QuizCorrectOption { get; init; }
    public bool? QuizCorrectTrueFalse { get; init; }
    public string QuizReferenceAnswer { get; init; } = string.Empty;
}
