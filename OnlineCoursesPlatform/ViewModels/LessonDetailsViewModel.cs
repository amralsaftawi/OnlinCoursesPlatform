using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.ViewModels
{
    public class LessonDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public int SectionOrderIndex { get; set; }
        public string SectionTitle { get; set; } = string.Empty;
    public bool IsFree { get; set; }
    public LessonType Type { get; set; }
    public string ContentUrl { get; set; } = string.Empty;
    public string ArticleContent { get; set; } = string.Empty;
    public LessonQuizViewModel? Quiz { get; set; }
}
}
