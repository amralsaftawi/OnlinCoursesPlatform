using OnlinCoursePlatform.Dtos;

namespace OnlinCoursePlatform.Abstrctions;


public interface IInstructorService
{
    
    
    Task<IEnumerable<InstructorCourseDto>> GetInstructorCoursesAsync(int instructorId);
    
        Task<InstructorStatsDto> GetInstructorStatsAsync(int instructorId);
}