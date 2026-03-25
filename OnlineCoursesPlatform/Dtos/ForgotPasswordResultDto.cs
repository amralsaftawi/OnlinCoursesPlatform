namespace OnlineCoursesPlatform.Dtos;

public class ForgotPasswordResultDto : ServiceResult
{
    public string? Email { get; init; }
    public string? EncodedToken { get; init; }
}
