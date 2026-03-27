using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AddSectionViewModel
{
    [Required(ErrorMessage = "Course id is required.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Please enter a section title.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Section title must be between 2 and 100 characters.")]
    [Display(Name = "Section Title")]
    public string Title { get; set; } = string.Empty;
}
