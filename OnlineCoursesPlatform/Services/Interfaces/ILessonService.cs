using OnlineCoursesPlatform.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineCoursesPlatform.Services.Interfaces
{
    public interface ILessonService
    {
        Task<Section> AddSectionAsync(int courseId, string title);
        Task<Lesson> AddLessonAsync(int sectionId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree);
        Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(int courseId);
        Task<Lesson> GetLessonByIdAsync(int lessonId);
        Task<Lesson> UpdateLessonAsync(int lessonId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree);
        Task<bool> DeleteLessonAsync(int lessonId);
        Task RecalculateCourseDurationAsync(int courseId);
    }
}
