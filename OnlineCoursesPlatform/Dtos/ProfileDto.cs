namespace OnlineCoursesPlatform.Dtos;

public class ProfileDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? ProfilePicture { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();

    public bool IsInstructor => Roles.Contains("Instructor");
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsStudent => Roles.Contains("Student");
}
