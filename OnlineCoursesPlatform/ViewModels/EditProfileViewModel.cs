namespace OnlinCoursePlatform.ViewModels;

public class EditProfileViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; } // اختياري
    public IFormFile? ProfileImage { get; set; } // لرفع الصورة
    public string? ExistingImageUrl { get; set; } // لعرض الصورة الحالية
}