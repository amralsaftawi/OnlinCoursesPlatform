namespace OnlineCoursesPlatform.Dtos
{
    public class CourseUpdateResultDto
    {
        public bool NotFound { get; init; }
        public bool IsForbidden { get; init; }
        public bool Succeeded { get; init; }
        public string? Message { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    }
}
