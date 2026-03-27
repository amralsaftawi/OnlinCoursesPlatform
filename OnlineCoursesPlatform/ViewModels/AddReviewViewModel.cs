using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AddReviewViewModel
{
    [Required(ErrorMessage = "Please select a rating.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    [Display(Name = "Rating")]
    public int Rating { get; set; } = 5;

    [Required(ErrorMessage = "Please write a review comment.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters.")]
    [Display(Name = "Comment")]
    public string Comment { get; set; } = string.Empty;
}
