namespace OnlineCoursesPlatform.Dtos
{
    public class AdminCourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = "$";
        public decimal Price { get; set; }
        public int SectionCount { get; set; }
        public int LessonCount { get; set; }
        public int TotalDuration { get; set; }
    }
}
