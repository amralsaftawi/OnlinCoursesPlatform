using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.ViewModels
{
    public class LessonDetailsViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsFree { get; set; }

        public LessonType Type { get; set; } 
        public string? ContentUrl { get; set; }

        public static implicit operator LessonDetailsViewModel(Lesson v)
        {
            throw new NotImplementedException();
        }
    }
}
