using OnlineCoursesPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AdminCategoriesViewModel
{
    public IEnumerable<Category> Categories { get; set; } = [];

    [Required(ErrorMessage = "Category title is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category title must be between 2 and 100 characters.")]
    [Display(Name = "Category Title")]
    public string NewCategoryTitle { get; set; } = string.Empty;
}
