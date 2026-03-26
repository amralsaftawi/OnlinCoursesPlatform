using OnlinCoursePlatform.Dtos;
using OnlineCoursesPlatform.ViewModels;

namespace OnlinCoursePlatform.Abstrctions;


public interface IInstructorService
{
    Task<IEnumerable<InstructorCourseDto>> GetInstructorCoursesAsync(int instructorId);
    Task<InstructorStatsDto> GetInstructorStatsAsync(int instructorId);
    Task<InstructorPublicProfileViewModel?> GetInstructorProfileAsync(int instructorId, int? viewerId, bool viewerIsAdmin);
}
