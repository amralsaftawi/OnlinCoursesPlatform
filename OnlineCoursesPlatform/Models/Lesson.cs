using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int SectionId { get; set; }
        public Section Section { get; set; }

        public string Title { get; set; }

        public string ContentUrl { get; set; }

        public int Duration { get; set; }

        public int OrderIndex { get; set; }

        public LessonType Type { get; set; }

        public bool IsFree { get; set; }

        public ICollection<UserProgress> UserProgresses { get; set; }
    }
}
