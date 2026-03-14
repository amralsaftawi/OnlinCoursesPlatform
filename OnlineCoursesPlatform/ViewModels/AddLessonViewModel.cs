using OnlineCoursesPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class AddLessonViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Please enter a lesson title.")]
        [StringLength(150)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please select a lesson type.")]
        public LessonType Type { get; set; }

        [Required(ErrorMessage = "Please enter a valid URL for the content.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string ContentUrl { get; set; }

        [Range(1, 1000, ErrorMessage = "Duration must be at least 1 minute.")]
        public int Duration { get; set; }

        public bool IsFree { get; set; }
    }
}
