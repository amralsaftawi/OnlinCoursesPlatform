using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class QuizSubmissionViewModel
{
    [Required(ErrorMessage = "Please answer the quiz before submitting.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Please answer the quiz before submitting.")]
    public string Answer { get; set; } = string.Empty;
}
