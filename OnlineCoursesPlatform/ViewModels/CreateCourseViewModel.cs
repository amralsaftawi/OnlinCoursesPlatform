using OnlineCoursesPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.ViewModels
{
    public class CreateCourseViewModel
    {

        [Required(ErrorMessage = "Course title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set;  }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(9999, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 9999 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Price { get; set; }

        // --- Currency Dropdown ---
        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        public int CurrencyId { get; set; }

        // This list will populate the Currency dropdown in the UI (Not saved to DB)
        public IEnumerable<SelectListItem> CurrenciesList { get; set; } = new List<SelectListItem>();

        // --- Image Upload ---
        [Display(Name = "Course Image")]
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

        // --- Category Dropdown ---
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        // This list will populate the Category dropdown in the UI (Not saved to DB)
        public IEnumerable<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

        // This list will populate the multi-select dropdown in the UI (Not saved to DB)
        public IEnumerable<SelectListItem> TagsList { get; set; } = new List<SelectListItem>();

        // Selected tag IDs (for many-to-many relationship)
        [Display(Name = "Tags")]
        public IEnumerable<int> SelectedTagIds { get; set; } = new List<int>();


        [Required(ErrorMessage = "Course level is required")]
        [Display(Name = "Level")]
        public CourseLevel Level { get; set; } 

        [Required(ErrorMessage = "Language is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 50 characters")]
        public string Language { get; set; } = "English";

        public int TotalDuration { get; set; }

    }
}
