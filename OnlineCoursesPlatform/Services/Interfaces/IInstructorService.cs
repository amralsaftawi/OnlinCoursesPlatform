using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services.Interfaces;

public interface IInstructorService
{
    Task<IEnumerable<InstructorCourseDto>> GetInstructorCoursesAsync(int instructorId);
    Task<InstructorStatsDto> GetInstructorStatsAsync(int instructorId);
    Task<InstructorPublicProfileViewModel?> GetInstructorProfileAsync(int instructorId, int? viewerId, bool viewerIsAdmin);
}
