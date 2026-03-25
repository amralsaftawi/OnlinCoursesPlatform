using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Repositories.Interface
{
    public interface ILearningService
    {
        Task<Lesson?> GetLessonDetailsAsync(int lessonId);
        Task<IEnumerable<Lesson>> GetCourseLessonsAsync(int courseId);
        Task<bool> MarkLessonAsCompletedAsync(int lessonId, int studentId);
        Task<int> GetProgressPercentageAsync(int courseId, int studentId);
        Task<bool> IsLessonCompletedAsync(int lessonId, int studentId);
    }
}
