using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services.Interfaces;

public interface ILearningService
{
    Task<Lesson?> GetLessonDetailsAsync(int lessonId);
    Task<IEnumerable<Lesson>> GetCourseLessonsAsync(int courseId);
    Task<LearningLessonExperienceDto> GetLessonExperienceAsync(int lessonId, int userId, bool isAdmin);
    Task<LessonCompletionResultDto> MarkLessonAsCompletedAsync(int lessonId, int studentId, bool isAdmin);
    Task<LessonCompletionResultDto> SubmitQuizAsync(int lessonId, int studentId, string answer, bool isAdmin);
    Task<IReadOnlyList<EnrolledCourseViewModel>> GetStudentLearningDashboardAsync(int studentId);
    Task<CourseProgressDto> GetCourseProgressAsync(int courseId, int studentId);
    Task<int> GetProgressPercentageAsync(int courseId, int studentId);
    Task<bool> IsLessonCompletedAsync(int lessonId, int studentId);
}
