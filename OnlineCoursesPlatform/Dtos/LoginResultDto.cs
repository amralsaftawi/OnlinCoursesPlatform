namespace OnlineCoursesPlatform.Dtos;

public class LoginResultDto : ServiceResult
{
    public string RedirectAction { get; init; } = "Index";
    public string RedirectController { get; init; } = "Home";
}
