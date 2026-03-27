using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.ViewModels;

public class AdminUserRoleUpdateViewModel
{
    [Required(ErrorMessage = "User id is required.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please choose a primary role.")]
    [RegularExpression("^(Student|Instructor)$", ErrorMessage = "Primary role must be Student or Instructor.")]
    [Display(Name = "Primary Role")]
    public string PrimaryRole { get; set; } = "Student";

    [Display(Name = "Admin Access")]
    public bool IsAdmin { get; set; }
}
