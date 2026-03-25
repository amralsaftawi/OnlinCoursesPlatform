using OnlineCoursesPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class AdminCategoriesViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string NewCategoryTitle { get; set; } = string.Empty;
    }
}
