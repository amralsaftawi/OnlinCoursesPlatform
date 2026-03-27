using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AdminCategoryUpdateViewModel
{
    [Required(ErrorMessage = "Category id is required.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Category title is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category title must be between 2 and 100 characters.")]
    [Display(Name = "Category Title")]
    public string Title { get; set; } = string.Empty;
}
