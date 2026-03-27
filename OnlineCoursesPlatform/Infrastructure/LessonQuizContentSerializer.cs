using System.Text.Json;
using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.Infrastructure;

internal static class LessonQuizContentSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(
        QuizQuestionType questionType,
        string prompt,
        IEnumerable<string> options,
        string? correctAnswer,
        string? referenceAnswer)
    {
        var payload = new LessonQuizContentPayload
        {
            QuestionType = questionType,
            Prompt = prompt.Trim(),
            Options = options
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Select(option => option.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            CorrectAnswer = (correctAnswer ?? string.Empty).Trim(),
            ReferenceAnswer = (referenceAnswer ?? string.Empty).Trim()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public static LessonQuizContentPayload? Deserialize(string? rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<LessonQuizContentPayload>(rawContent, JsonOptions);
            if (payload == null || string.IsNullOrWhiteSpace(payload.Prompt))
            {
                return null;
            }

            payload.Options ??= [];
            return payload;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

internal sealed class LessonQuizContentPayload
{
    public QuizQuestionType QuestionType { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string CorrectAnswer { get; init; } = string.Empty;
    public string ReferenceAnswer { get; init; } = string.Empty;
}
