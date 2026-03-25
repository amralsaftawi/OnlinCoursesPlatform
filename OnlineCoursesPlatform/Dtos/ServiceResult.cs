namespace OnlineCoursesPlatform.Dtos;

public class ServiceResult
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ServiceResult Success(string? message = null)
    {
        return new ServiceResult
        {
            Succeeded = true,
            Message = message
        };
    }

    public static ServiceResult Failure(params string[] errors)
    {
        return Failure(errors.AsEnumerable());
    }

    public static ServiceResult Failure(IEnumerable<string> errors, string? message = null)
    {
        return new ServiceResult
        {
            Succeeded = false,
            Message = message,
            Errors = errors.Where(error => !string.IsNullOrWhiteSpace(error)).ToArray()
        };
    }
}
