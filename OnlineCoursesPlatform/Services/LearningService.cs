namespace OnlinCoursePlatform.Services;

using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;

public class LearningService : ILearningService
{
    private readonly AppDbContext _context;

    public LearningService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson> GetLessonDetailsAsync(int lessonId)
    {
        return await _context.Lessons
            .Include(l => l.Section)
            .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);
    }

    public async Task<IEnumerable<Lesson>> GetCourseLessonsAsync(int courseId)
    {
        return await _context.Lessons
            .Where(l => l.Section.CourseId == courseId)
            .OrderBy(l => l.Section.OrderIndex)
            .ThenBy(l => l.OrderIndex)
            .ToListAsync();
    }


    public async Task<bool> MarkLessonAsCompletedAsync(int lessonId)
{
    // ملاحظة: في المشروع الحقيقي، هتحتاج الـ UserId كمان عشان تعرف مين اللي خلص الدرس
    // حالياً بنفترض إننا بنحدث حالة عامة أو بنتعامل مع Progress جدول
    
    var lesson = await _context.Lessons.FindAsync(lessonId);
    if (lesson == null) return false;

    // هنا ممكن تضيف Logic لحفظ التقدم في جدول UserProgress
    // مثال بسيط: 
    // var progress = new UserProgress { LessonId = lessonId, UserId = currentUserId, IsCompleted = true };
    // _context.UserProgress.Add(progress);

    try 
    {
        await _context.SaveChangesAsync();
        return true;
    }
    catch
    {
        return false;
    }
}
}