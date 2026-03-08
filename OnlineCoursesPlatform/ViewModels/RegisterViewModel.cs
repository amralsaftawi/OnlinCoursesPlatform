namespace OnlinCoursePlatform.ViewModels;

public class RegisterViewModel
{
    public string FirstName { get; set; } = string.Empty;
     public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsInstructor { get; set; } // عشان نحدد الـ Role
}