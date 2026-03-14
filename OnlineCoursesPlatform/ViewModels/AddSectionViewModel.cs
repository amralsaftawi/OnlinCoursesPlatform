using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class AddSectionViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please enter a section title.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }
    }
}
