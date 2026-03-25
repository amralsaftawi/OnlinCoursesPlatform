using System.ComponentModel.DataAnnotations;

namespace OnlinCoursePlatform.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? GeneratedResetLink { get; set; }
}
