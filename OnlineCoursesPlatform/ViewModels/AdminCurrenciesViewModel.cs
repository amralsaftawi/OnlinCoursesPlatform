using OnlineCoursesPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class AdminCurrenciesViewModel
    {
        public IEnumerable<Currency> Currencies { get; set; } = Enumerable.Empty<Currency>();

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(5, MinimumLength = 1)]
        public string Symbol { get; set; } = string.Empty;
    }
}
