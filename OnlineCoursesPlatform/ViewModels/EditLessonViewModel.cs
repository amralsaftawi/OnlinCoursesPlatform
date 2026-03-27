using Microsoft.AspNetCore.Http;
using OnlineCoursesPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class EditLessonViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Lesson id is required.")]
    public int LessonId { get; set; }

    [Required(ErrorMessage = "Course id is required.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Section id is required.")]
    public int SectionId { get; set; }

    [Required(ErrorMessage = "Please enter a lesson title.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Lesson title must be between 2 and 150 characters.")]
    [Display(Name = "Lesson Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a lesson type.")]
    [Display(Name = "Lesson Type")]
    public LessonType Type { get; set; }

    [Display(Name = "Video URL")]
    public string? ContentUrl { get; set; }

    [Display(Name = "Article Text File")]
    public IFormFile? ArticleFile { get; set; }

    [Display(Name = "PDF File")]
    public IFormFile? PdfFile { get; set; }

    [Display(Name = "Quiz Type")]
    public QuizQuestionType? QuizMode { get; set; }

    [StringLength(240, MinimumLength = 5, ErrorMessage = "Quiz prompt must be between 5 and 240 characters.")]
    [Display(Name = "Quiz Prompt")]
    public string? QuizPrompt { get; set; }

    [StringLength(120, ErrorMessage = "Each option must be 120 characters or fewer.")]
    [Display(Name = "Option A")]
    public string? QuizOptionA { get; set; }

    [StringLength(120, ErrorMessage = "Each option must be 120 characters or fewer.")]
    [Display(Name = "Option B")]
    public string? QuizOptionB { get; set; }

    [StringLength(120, ErrorMessage = "Each option must be 120 characters or fewer.")]
    [Display(Name = "Option C")]
    public string? QuizOptionC { get; set; }

    [StringLength(120, ErrorMessage = "Each option must be 120 characters or fewer.")]
    [Display(Name = "Option D")]
    public string? QuizOptionD { get; set; }

    [Range(1, 4, ErrorMessage = "Choose the correct option.")]
    [Display(Name = "Correct Option")]
    public int? QuizCorrectOption { get; set; }

    [Display(Name = "Correct True Or False Answer")]
    public bool? QuizCorrectTrueFalse { get; set; }

    [StringLength(400, ErrorMessage = "Reference answer must be 400 characters or fewer.")]
    [Display(Name = "Reference Answer")]
    public string? QuizReferenceAnswer { get; set; }

    [Range(1, 1000, ErrorMessage = "Duration must be at least 1 minute.")]
    [Display(Name = "Duration In Minutes")]
    public int Duration { get; set; }

    [Display(Name = "Free Preview")]
    public bool IsFree { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == LessonType.Article)
        {
            if (string.IsNullOrWhiteSpace(ContentUrl) && ArticleFile == null)
            {
                yield return new ValidationResult("Please keep the current article text file or upload a replacement `.txt` file.", [nameof(ContentUrl), nameof(ArticleFile)]);
                yield break;
            }

            if (ArticleFile != null &&
                !string.Equals(Path.GetExtension(ArticleFile.FileName), ".txt", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Only `.txt` files are allowed for article lessons.", [nameof(ArticleFile)]);
            }

            yield break;
        }

        if (Type == LessonType.PDF)
        {
            if (string.IsNullOrWhiteSpace(ContentUrl) && PdfFile == null)
            {
                yield return new ValidationResult("Please keep the current PDF file, upload a replacement PDF, or paste a direct PDF URL.", [nameof(ContentUrl), nameof(PdfFile)]);
                yield break;
            }

            if (PdfFile != null &&
                !string.Equals(Path.GetExtension(PdfFile.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Only `.pdf` files are allowed for PDF lessons.", [nameof(PdfFile)]);
            }

            if (!string.IsNullOrWhiteSpace(ContentUrl))
            {
                if (!Uri.TryCreate(ContentUrl, UriKind.Absolute, out _)
                    && !ContentUrl.StartsWith("/"))
                {
                    yield return new ValidationResult("Please enter a valid absolute URL or keep the current uploaded PDF path.", [nameof(ContentUrl)]);
                }
                else if (!ContentUrl.Split('?')[0].Split('#')[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult("The PDF lesson content must point directly to a `.pdf` file.", [nameof(ContentUrl)]);
                }
            }

            yield break;
        }

        if (Type == LessonType.Quiz)
        {
            if (!QuizMode.HasValue)
            {
                yield return new ValidationResult("Please choose the quiz type.", [nameof(QuizMode)]);
            }

            if (string.IsNullOrWhiteSpace(QuizPrompt))
            {
                yield return new ValidationResult("Please write the quiz prompt.", [nameof(QuizPrompt)]);
            }

            if (QuizMode == QuizQuestionType.TrueFalse)
            {
                if (!QuizCorrectTrueFalse.HasValue)
                {
                    yield return new ValidationResult("Please choose whether the correct answer is true or false.", [nameof(QuizCorrectTrueFalse)]);
                }

                yield break;
            }

            if (QuizMode == QuizQuestionType.MultipleChoice)
            {
                var options = new[] { QuizOptionA, QuizOptionB, QuizOptionC, QuizOptionD }
                    .Where(option => !string.IsNullOrWhiteSpace(option))
                    .Select(option => option!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (options.Count < 2)
                {
                    yield return new ValidationResult("Please provide at least two answer choices for the quiz.", [nameof(QuizOptionA), nameof(QuizOptionB), nameof(QuizOptionC), nameof(QuizOptionD)]);
                }

                if (!QuizCorrectOption.HasValue || QuizCorrectOption.Value > options.Count)
                {
                    yield return new ValidationResult("Please select which choice is correct.", [nameof(QuizCorrectOption)]);
                }
            }

            yield break;
        }

        if (string.IsNullOrWhiteSpace(ContentUrl))
        {
            yield return new ValidationResult("Please provide the lesson video URL.", [nameof(ContentUrl)]);
            yield break;
        }

        if (!Uri.TryCreate(ContentUrl, UriKind.Absolute, out _))
        {
            yield return new ValidationResult("Please enter a valid absolute URL for the video lesson.", [nameof(ContentUrl)]);
        }
    }
}
