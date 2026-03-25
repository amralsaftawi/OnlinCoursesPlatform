using Microsoft.AspNetCore.Http;
using OnlineCoursesPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class EditLessonViewModel : IValidatableObject
    {
        [Required]
        public int LessonId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Please enter a lesson title.")]
        [StringLength(150)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please select a lesson type.")]
        public LessonType Type { get; set; }

        public string? ContentUrl { get; set; }

        public IFormFile? ArticleFile { get; set; }

        [Range(1, 1000, ErrorMessage = "Duration must be at least 1 minute.")]
        public int Duration { get; set; }

        public bool IsFree { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == LessonType.Article)
            {
                if (string.IsNullOrWhiteSpace(ContentUrl) && ArticleFile == null)
                {
                    yield return new ValidationResult("Please keep the article content or upload a replacement article file.", [nameof(ContentUrl), nameof(ArticleFile)]);
                }
            }
            else if (string.IsNullOrWhiteSpace(ContentUrl))
            {
                yield return new ValidationResult("Please provide the lesson file or stream URL.", [nameof(ContentUrl)]);
            }
        }
    }
}
