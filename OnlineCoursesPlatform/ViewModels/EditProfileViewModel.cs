using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Profile Image")]
    public IFormFile? ProfileImage { get; set; }

    public string? ExistingImageUrl { get; set; }
}
