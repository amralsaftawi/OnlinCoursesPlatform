using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineCoursesPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "Course title is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Course title must be between 5 and 200 characters.")]
    [Display(Name = "Course Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(9999, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 9999 characters.")]
    [Display(Name = "Course Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
    [DisplayFormat(DataFormatString = "{0:F2}")]
    [Display(Name = "Course Price")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [Display(Name = "Currency")]
    public int CurrencyId { get; set; }

    public IEnumerable<SelectListItem> CurrenciesList { get; set; } = [];

    [Display(Name = "Course Image")]
    public IFormFile? ImageFile { get; set; }

    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public IEnumerable<SelectListItem> CategoriesList { get; set; } = [];

    public IEnumerable<SelectListItem> TagsList { get; set; } = [];

    [Display(Name = "Course Tags")]
    public IEnumerable<int> SelectedTagIds { get; set; } = [];

    [Required(ErrorMessage = "Course level is required.")]
    [Display(Name = "Course Level")]
    public CourseLevel Level { get; set; }

    [Required(ErrorMessage = "Language is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 50 characters.")]
    [Display(Name = "Course Language")]
    public string Language { get; set; } = "English";

    public int TotalDuration { get; set; }
}
