using OnlineCoursesPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AdminTagsViewModel
{
    public IEnumerable<Tag> Tags { get; set; } = [];

    [Required(ErrorMessage = "Tag name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters.")]
    [Display(Name = "Tag Name")]
    public string NewTagName { get; set; } = string.Empty;
}
