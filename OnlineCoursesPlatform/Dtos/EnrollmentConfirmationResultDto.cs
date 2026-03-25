using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Dtos;

public class EnrollmentConfirmationResultDto
{
    public Course? Course { get; init; }
    public bool NotFound { get; init; }
    public bool RedirectToCourseDetails { get; init; }
    public string? Message { get; init; }
    public string? MessageKey { get; init; }
}
