namespace OnlineCoursesPlatform.ViewModels
{
    public class LessonDetailsViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsFree { get; set; }
    }
}
