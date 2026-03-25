using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.ViewModels;

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

        public async Task<LearningLessonExperienceDto> GetLessonExperienceAsync(int lessonId, int userId, bool isAdmin)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Section)
                    .ThenInclude(section => section.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                return new LearningLessonExperienceDto { NotFound = true };
            }

            var courseId = lesson.Section.CourseId;
            var isEnrolled = await _context.Enrollments.AnyAsync(enrollment => enrollment.CourseId == courseId && enrollment.StudentId == userId);
            var isOwner = lesson.Section.Course.InstructorId == userId;

            if (!lesson.IsFree && !isEnrolled && !isOwner && !isAdmin)
            {
                return new LearningLessonExperienceDto
                {
                    RedirectCourseId = courseId
                };
            }

            var orderedLessons = await _context.Lessons
                .AsNoTracking()
                .Where(l => l.Section.CourseId == courseId)
                .OrderBy(l => l.Section.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .Select(l => new LessonDetailsViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Duration = l.Duration,
                    Type = l.Type,
                    OrderIndex = l.OrderIndex,
                    SectionOrderIndex = l.Section.OrderIndex,
                    SectionTitle = l.Section.Title,
                    IsFree = l.IsFree
                })
                .ToListAsync();

            var progress = isEnrolled
                ? await GetCourseProgressAsync(courseId, userId)
                : new CourseProgressDto
                {
                    CourseId = courseId,
                    TotalLessons = orderedLessons.Count,
                    ContinueLessonId = orderedLessons.FirstOrDefault()?.Id
                };

            var isCompleted = isEnrolled && await IsLessonCompletedAsync(lesson.Id, userId);

            return new LearningLessonExperienceDto
            {
                ViewModel = new LearningViewModel
                {
                    CourseId = courseId,
                    CurrentLesson = new LessonDetailsViewModel
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        ContentUrl = lesson.ContentUrl,
                        Type = lesson.Type,
                        OrderIndex = lesson.OrderIndex,
                        SectionOrderIndex = lesson.Section.OrderIndex,
                        SectionTitle = lesson.Section.Title,
                        IsFree = lesson.IsFree,
                        Duration = lesson.Duration
                    },
                    CourseLessons = orderedLessons,
                    CourseTitle = lesson.Section.Course.Title,
                    ProgressPercentage = progress.ProgressPercentage,
                    IsCompleted = isCompleted,
                    IsOwnerPreview = isOwner && !isEnrolled,
                    CanTrackProgress = isEnrolled && !isOwner && !isAdmin,
                    CompletedLessons = progress.CompletedLessons,
                    TotalLessons = progress.TotalLessons
                }
            };
        }

        public async Task<LessonCompletionResultDto> MarkLessonAsCompletedAsync(int lessonId, int studentId, bool isAdmin)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .ThenInclude(section => section.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                return new LessonCompletionResultDto
                {
                    Succeeded = false,
                    Errors = ["Lesson not found."]
                };
            }

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == lesson.Section.CourseId && e.StudentId == studentId);

            if (isAdmin || lesson.Section.Course.InstructorId == studentId)
            {
                return new LessonCompletionResultDto
                {
                    Succeeded = false,
                    Errors = ["Course owners and admins can preview lessons, but progress is tracked only for enrolled students."]
                };
            }

            if (enrollment == null)
            {
                return new LessonCompletionResultDto
                {
                    Succeeded = false,
                    Errors = ["Enroll in the course first to track progress."]
                };
            }

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

            await _context.SaveChangesAsync();

            var summary = await BuildCourseProgressAsync(lesson.Section.CourseId, studentId);
            if (enrollment.ProgressPercentage != summary.ProgressPercentage)
            {
                enrollment.ProgressPercentage = summary.ProgressPercentage;
                await _context.SaveChangesAsync();
            }

            var orderedLessons = await GetOrderedLessonsAsync(lesson.Section.CourseId);
            var currentLessonIndex = orderedLessons.FindIndex(item => item.Id == lessonId);
            int? nextLessonId = currentLessonIndex >= 0 && currentLessonIndex < orderedLessons.Count - 1
                ? orderedLessons[currentLessonIndex + 1].Id
                : null;

            return new LessonCompletionResultDto
            {
                Succeeded = true,
                NextLessonId = nextLessonId,
                ProgressPercentage = summary.ProgressPercentage
            };
        }

        public async Task<IReadOnlyList<EnrolledCourseViewModel>> GetStudentLearningDashboardAsync(int studentId)
        {
            var enrollments = await _context.Enrollments
                .Where(enrollment => enrollment.StudentId == studentId)
                .Include(enrollment => enrollment.Course)
                    .ThenInclude(course => course.Instructor)
                .OrderByDescending(enrollment => enrollment.EnrolledAt)
                .ToListAsync();

            var learningCourses = new List<EnrolledCourseViewModel>();
            var hasEnrollmentUpdates = false;

            foreach (var enrollment in enrollments)
            {
                var summary = await BuildCourseProgressAsync(enrollment.CourseId, studentId);
                if (enrollment.ProgressPercentage != summary.ProgressPercentage)
                {
                    enrollment.ProgressPercentage = summary.ProgressPercentage;
                    hasEnrollmentUpdates = true;
                }

                learningCourses.Add(new EnrolledCourseViewModel
                {
                    CourseId = enrollment.CourseId,
                    Title = enrollment.Course.Title,
                    ImageUrl = enrollment.Course.ImageUrl,
                    ProgressPercentage = summary.ProgressPercentage,
                    EnrolledAt = enrollment.EnrolledAt,
                    ContinueLessonId = summary.ContinueLessonId,
                    InstructorName = $"{enrollment.Course.Instructor.FirstName} {enrollment.Course.Instructor.LastName}".Trim(),
                    CompletedLessons = summary.CompletedLessons,
                    TotalLessons = summary.TotalLessons
                });
            }

            if (hasEnrollmentUpdates)
            {
                await _context.SaveChangesAsync();
            }

            return learningCourses;
        }

        public async Task<CourseProgressDto> GetCourseProgressAsync(int courseId, int studentId)
        {
            var summary = await BuildCourseProgressAsync(courseId, studentId);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(item => item.CourseId == courseId && item.StudentId == studentId);

            if (enrollment != null && enrollment.ProgressPercentage != summary.ProgressPercentage)
            {
                enrollment.ProgressPercentage = summary.ProgressPercentage;
                await _context.SaveChangesAsync();
            }

            return summary;
        }

        public async Task<int> GetProgressPercentageAsync(int courseId, int studentId)
        {
            var progress = await GetCourseProgressAsync(courseId, studentId);
            return progress.ProgressPercentage;
        }

        public async Task<bool> IsLessonCompletedAsync(int lessonId, int studentId)
        {
            return await _context.UserProgresses.AnyAsync(p => p.LessonId == lessonId && p.StudentId == studentId && p.IsCompleted);
        }

        private async Task<CourseProgressDto> BuildCourseProgressAsync(int courseId, int studentId)
        {
            var orderedLessons = await GetOrderedLessonsAsync(courseId);
            if (orderedLessons.Count == 0)
            {
                return new CourseProgressDto
                {
                    CourseId = courseId,
                    TotalLessons = 0,
                    CompletedLessons = 0,
                    ProgressPercentage = 0
                };
            }

            var completedLessonIds = await _context.UserProgresses
                .Where(p => p.StudentId == studentId && p.IsCompleted && p.Lesson.Section.CourseId == courseId)
                .Select(p => p.LessonId)
                .Distinct()
                .ToListAsync();

            var completedLessonSet = completedLessonIds.ToHashSet();
            var completedLessons = orderedLessons.Count(lesson => completedLessonSet.Contains(lesson.Id));
            var progressPercentage = (int)Math.Round((decimal)completedLessons / orderedLessons.Count * 100m, MidpointRounding.AwayFromZero);
            var continueLessonId = orderedLessons.FirstOrDefault(lesson => !completedLessonSet.Contains(lesson.Id))?.Id
                ?? orderedLessons.FirstOrDefault()?.Id;

            return new CourseProgressDto
            {
                CourseId = courseId,
                TotalLessons = orderedLessons.Count,
                CompletedLessons = completedLessons,
                ProgressPercentage = progressPercentage,
                ContinueLessonId = continueLessonId
            };
        }

        private async Task<List<OrderedLessonItem>> GetOrderedLessonsAsync(int courseId)
        {
            return await _context.Lessons
                .Where(lesson => lesson.Section.CourseId == courseId)
                .OrderBy(lesson => lesson.Section.OrderIndex)
                .ThenBy(lesson => lesson.OrderIndex)
                .Select(lesson => new OrderedLessonItem
                {
                    Id = lesson.Id
                })
                .ToListAsync();
        }

        private sealed class OrderedLessonItem
        {
            public int Id { get; init; }
        }
    }
}
