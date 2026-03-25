namespace OnlineCoursesPlatform.ViewModels
{
    public class EnrolledCourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime EnrolledAt { get; set; }
        public int? ContinueLessonId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public bool IsCompleted => TotalLessons > 0 && CompletedLessons >= TotalLessons;
    }
}
