using Microsoft.AspNetCore.Identity;

namespace OnlineCoursesPlatform.Models;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfilePicture { get; set; } = "default-avatar.png";

    public AdminProfile? AdminProfile { get; set; }

    public ICollection<Course> Courses { get; set; } = [];

    public ICollection<Enrollment> Enrollments { get; set; } = [];

    public ICollection<UserProgress> Progresses { get; set; } = [];

    public ICollection<Review> Reviews { get; set; } = [];
}
