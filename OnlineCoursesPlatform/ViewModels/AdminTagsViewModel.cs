using OnlineCoursesPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels
{
    public class AdminTagsViewModel
    {
        public IEnumerable<Tag> Tags { get; set; } = Enumerable.Empty<Tag>();

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string NewTagName { get; set; } = string.Empty;
    }
}
