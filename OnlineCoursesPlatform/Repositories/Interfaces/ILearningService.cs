namespace OnlineCoursesPlatform.Repositories.Interface;

using OnlineCoursesPlatform.Models;

public interface ILearningService
{
    // جلب بيانات الدرس بالكامل مع الكورس والسكشن
    Task<Lesson> GetLessonDetailsAsync(int lessonId);
    
    // جلب قائمة الدروس لنفس الكورس عشان الطالب يتنقل بينهم
    Task<IEnumerable<Lesson>> GetCourseLessonsAsync(int courseId);

     Task<bool> MarkLessonAsCompletedAsync(int lessonId);
   
}