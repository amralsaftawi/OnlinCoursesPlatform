namespace OnlineCoursesPlatform.Models;

public class AdminProfile
{
    public int Id { get; set; }

    public int ApplicationUserId { get; set; }
    public User ApplicationUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
