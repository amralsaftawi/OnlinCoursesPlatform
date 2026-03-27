using OnlineCoursesPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AdminCurrenciesViewModel
{
    public IEnumerable<Currency> Currencies { get; set; } = [];

    [Required(ErrorMessage = "Currency name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Currency name must be between 2 and 50 characters.")]
    [Display(Name = "Currency Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency code is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must contain exactly 3 characters.")]
    [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "Currency code must contain exactly 3 letters.")]
    [Display(Name = "Currency Code")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency symbol is required.")]
    [StringLength(5, MinimumLength = 1, ErrorMessage = "Currency symbol must be between 1 and 5 characters.")]
    [Display(Name = "Currency Symbol")]
    public string Symbol { get; set; } = string.Empty;
}
