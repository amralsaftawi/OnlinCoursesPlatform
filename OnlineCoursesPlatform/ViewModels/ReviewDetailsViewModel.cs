namespace OnlineCoursesPlatform.ViewModels
{
    public class ReviewDetailsViewModel
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentProfilePicture { get; set; } = "/images/profiles/default-avatar.png";
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
