namespace OnlineCoursesPlatform.ViewModels
{
    public class ReviewDetailsViewModel
    {
        public int Id { get; set; }
        public string? StudentName { get; set; }
        public string? StudentProfilePicture { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
