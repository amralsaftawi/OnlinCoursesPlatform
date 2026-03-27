using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.ViewModels;

public class LessonQuizViewModel
{
    public QuizQuestionType QuestionType { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public bool HasReferenceAnswer { get; set; }
}
