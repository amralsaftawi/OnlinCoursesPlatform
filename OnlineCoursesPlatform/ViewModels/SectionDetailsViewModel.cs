namespace OnlineCoursesPlatform.ViewModels
{
    public class SectionDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public List<LessonDetailsViewModel> Lessons { get; set; } = new();
    }
}
