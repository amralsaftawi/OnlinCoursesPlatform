using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineCoursesPlatform.Services.Interfaces
{
    public interface ILessonService
    {
        Task<CourseContentViewResultDto> GetManageCourseContentAsync(int courseId, int? actingUserId, bool isAdmin);
        Task<CourseContentActionResultDto> CreateSectionAsync(AddSectionViewModel model, int? actingUserId, bool isAdmin);
        Task<CourseContentActionResultDto> CreateLessonAsync(AddLessonViewModel model, int? actingUserId, bool isAdmin);
        Task<LessonEditorResultDto> GetLessonForEditAsync(int lessonId, int? actingUserId, bool isAdmin);
        Task<CourseContentActionResultDto> UpdateLessonAsync(EditLessonViewModel model, int? actingUserId, bool isAdmin);
        Task<CourseContentActionResultDto> RemoveLessonAsync(int lessonId, int courseId, int? actingUserId, bool isAdmin);
        Task<Section> AddSectionAsync(int courseId, string title);
        Task<Lesson> AddLessonAsync(int sectionId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree);
        Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(int courseId);
        Task<Lesson> GetLessonByIdAsync(int lessonId);
        Task<Lesson> UpdateLessonAsync(int lessonId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree);
        Task<bool> DeleteLessonAsync(int lessonId);
        Task RecalculateCourseDurationAsync(int courseId);
    }
}
