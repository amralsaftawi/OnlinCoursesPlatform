using OnlinCoursesPlatform.Data;

namespace OnlineCoursesPlatform.Models
{
    public class AdminProfile
    {
        public int Id { get; set; }

        // FK
        public int ApplicationUserId { get; set; }
        public User ApplicationUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}