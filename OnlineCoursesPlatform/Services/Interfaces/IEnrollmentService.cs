using OnlineCoursesPlatform.Dtos;

namespace OnlineCoursesPlatform.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentActionResultDto> EnrollAsync(int courseId, int studentId);
    Task<EnrollmentConfirmationResultDto> GetConfirmationAsync(int courseId, int studentId);
}
