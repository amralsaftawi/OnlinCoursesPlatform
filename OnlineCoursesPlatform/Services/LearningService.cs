using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;

namespace OnlinCoursePlatform.Services
{
    public class LearningService : ILearningService
    {
        private readonly AppDbContext _context;

        public LearningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lesson?> GetLessonDetailsAsync(int lessonId)
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

        public async Task<bool> MarkLessonAsCompletedAsync(int lessonId, int studentId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return false;

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == lesson.Section.CourseId && e.StudentId == studentId);

            if (enrollment == null && !lesson.IsFree)
                return false;

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.StudentId == studentId);

            if (progress == null)
            {
                _context.UserProgresses.Add(new UserProgress
                {
                    LessonId = lessonId,
                    StudentId = studentId,
                    IsCompleted = true
                });
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
            }

            if (enrollment != null)
            {
                enrollment.ProgressPercentage = await CalculateProgressAsync(lesson.Section.CourseId, studentId, countCurrentLesson: true);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetProgressPercentageAsync(int courseId, int studentId)
        {
            var progress = await CalculateProgressAsync(courseId, studentId, countCurrentLesson: false);
            return (int)Math.Round(progress, MidpointRounding.AwayFromZero);
        }

        public async Task<bool> IsLessonCompletedAsync(int lessonId, int studentId)
        {
            return await _context.UserProgresses.AnyAsync(p => p.LessonId == lessonId && p.StudentId == studentId && p.IsCompleted);
        }

        private async Task<decimal> CalculateProgressAsync(int courseId, int studentId, bool countCurrentLesson)
        {
            var totalLessons = await _context.Lessons.CountAsync(l => l.Section.CourseId == courseId);
            if (totalLessons == 0)
                return 0;

            var completedLessons = await _context.UserProgresses
                .Where(p => p.StudentId == studentId && p.IsCompleted && p.Lesson.Section.CourseId == courseId)
                .Select(p => p.LessonId)
                .Distinct()
                .CountAsync();

            if (countCurrentLesson)
            {
                return Math.Round((decimal)completedLessons / totalLessons * 100m, 2);
            }

            return Math.Round((decimal)completedLessons / totalLessons * 100m, 2);
        }
    }
}
