using Microsoft.AspNetCore.Http;
using OnlineCoursesPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class EditCourseViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Course title is required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(9999, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 9999 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Course level is required.")]
        public CourseLevel Level { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 50 characters.")]
        public string Language { get; set; } = "English";

        public string ExistingImageUrl { get; set; } = "/images/default-course.jpg";

        public IFormFile? ImageFile { get; set; }
    }
}
