using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using OnlineCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Infrastructure;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services;

public class LearningService : ILearningService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    public LearningService(AppDbContext context, IMapper mapper, IWebHostEnvironment environment)
    {
        _context = context;
        _mapper = mapper;
        _environment = environment;
    }

    public async Task<Lesson?> GetLessonDetailsAsync(int lessonId)
    {
        return await _context.Lessons
            .Include(lesson => lesson.Section)
                .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(lesson => lesson.Id == lessonId);
    }

    public async Task<IEnumerable<Lesson>> GetCourseLessonsAsync(int courseId)
    {
        return await _context.Lessons
            .Where(lesson => lesson.Section.CourseId == courseId)
            .OrderBy(lesson => lesson.Section.OrderIndex)
            .ThenBy(lesson => lesson.OrderIndex)
            .ToListAsync();
    }

    public async Task<LearningLessonExperienceDto> GetLessonExperienceAsync(int lessonId, int userId, bool isAdmin)
    {
        var lesson = await _context.Lessons
            .AsNoTracking()
            .Include(item => item.Section)
                .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(item => item.Id == lessonId);

        if (lesson == null)
        {
            return new LearningLessonExperienceDto { NotFound = true };
        }

        var courseId = lesson.Section.CourseId;
        var isEnrolled = await _context.Enrollments
            .AnyAsync(enrollment => enrollment.CourseId == courseId && enrollment.StudentId == userId);

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
            .Where(item => item.Section.CourseId == courseId)
            .OrderBy(item => item.Section.OrderIndex)
            .ThenBy(item => item.OrderIndex)
            .ProjectTo<LessonDetailsViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var progress = isEnrolled
            ? await GetCourseProgressAsync(courseId, userId)
            : new CourseProgressDto
            {
                CourseId = courseId,
                TotalLessons = orderedLessons.Count,
                ContinueLessonId = orderedLessons.FirstOrDefault()?.Id
            };

        var currentLesson = _mapper.Map<LessonDetailsViewModel>(lesson);
        await EnrichCurrentLessonAsync(currentLesson, lesson);

        var isCompleted = isEnrolled && await IsLessonCompletedAsync(lesson.Id, userId);

        return new LearningLessonExperienceDto
        {
            ViewModel = new LearningViewModel
            {
                CourseId = courseId,
                CurrentLesson = currentLesson,
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
            .Include(item => item.Section)
                .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(item => item.Id == lessonId);

        if (lesson == null)
        {
            return new LessonCompletionResultDto
            {
                Errors = ["Lesson not found."]
            };
        }

        return await TrackLessonCompletionAsync(lesson, studentId, isAdmin, null);
    }

    public async Task<LessonCompletionResultDto> SubmitQuizAsync(int lessonId, int studentId, string answer, bool isAdmin)
    {
        var lesson = await _context.Lessons
            .Include(item => item.Section)
                .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(item => item.Id == lessonId);

        if (lesson == null)
        {
            return new LessonCompletionResultDto
            {
                Errors = ["Quiz lesson not found."]
            };
        }

        if (lesson.Type != LessonType.Quiz)
        {
            return new LessonCompletionResultDto
            {
                Errors = ["This lesson is not a quiz."]
            };
        }

        var payload = LessonQuizContentSerializer.Deserialize(lesson.ContentUrl);
        if (payload == null)
        {
            return new LessonCompletionResultDto
            {
                Errors = ["This quiz could not be loaded correctly."]
            };
        }

        var userAnswer = (answer ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(userAnswer))
        {
            return new LessonCompletionResultDto
            {
                Errors = ["Please answer the quiz before submitting."]
            };
        }

        if (payload.QuestionType == QuizQuestionType.Written)
        {
            var message = string.IsNullOrWhiteSpace(payload.ReferenceAnswer)
                ? "Written response submitted."
                : $"Written response submitted. Reference answer: {payload.ReferenceAnswer}";

            return await TrackLessonCompletionAsync(lesson, studentId, isAdmin, message);
        }

        if (!string.Equals(userAnswer, payload.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
        {
            return new LessonCompletionResultDto
            {
                Errors = ["That answer is not correct yet. Try again."]
            };
        }

        return await TrackLessonCompletionAsync(lesson, studentId, isAdmin, "Correct answer.");
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
        return await _context.UserProgresses
            .AnyAsync(progress => progress.LessonId == lessonId && progress.StudentId == studentId && progress.IsCompleted);
    }

    private async Task EnrichCurrentLessonAsync(LessonDetailsViewModel destination, Lesson source)
    {
        if (source.Type == LessonType.Article)
        {
            if (LessonContentStorage.IsLocalArticleUpload(source.ContentUrl))
            {
                destination.ArticleContent = await LessonContentStorage.ReadLocalArticleContentAsync(_environment, source.ContentUrl) ?? string.Empty;
            }
            else if (!Uri.IsWellFormedUriString(source.ContentUrl, UriKind.Absolute) && !source.ContentUrl.StartsWith("/"))
            {
                destination.ArticleContent = source.ContentUrl ?? string.Empty;
            }
        }
        else if (source.Type == LessonType.Quiz)
        {
            var payload = LessonQuizContentSerializer.Deserialize(source.ContentUrl);
            if (payload != null)
            {
                destination.Quiz = new LessonQuizViewModel
                {
                    QuestionType = payload.QuestionType,
                    Prompt = payload.Prompt,
                    Options = payload.Options,
                    HasReferenceAnswer = !string.IsNullOrWhiteSpace(payload.ReferenceAnswer)
                };
            }
        }
    }

    private async Task<LessonCompletionResultDto> TrackLessonCompletionAsync(Lesson lesson, int studentId, bool isAdmin, string? successMessage)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(item => item.CourseId == lesson.Section.CourseId && item.StudentId == studentId);

        if (isAdmin || lesson.Section.Course.InstructorId == studentId)
        {
            return new LessonCompletionResultDto
            {
                Errors = ["Course owners and admins can preview lessons, but progress is tracked only for enrolled students."]
            };
        }

        if (enrollment == null)
        {
            return new LessonCompletionResultDto
            {
                Errors = ["Enroll in the course first to track progress."]
            };
        }

        var progress = await _context.UserProgresses
            .FirstOrDefaultAsync(item => item.LessonId == lesson.Id && item.StudentId == studentId);

        if (progress == null)
        {
            _context.UserProgresses.Add(new UserProgress
            {
                LessonId = lesson.Id,
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
        var currentLessonIndex = orderedLessons.FindIndex(item => item.Id == lesson.Id);

        int? nextLessonId = currentLessonIndex >= 0 && currentLessonIndex < orderedLessons.Count - 1
            ? orderedLessons[currentLessonIndex + 1].Id
            : null;

        return new LessonCompletionResultDto
        {
            Succeeded = true,
            Message = successMessage,
            NextLessonId = nextLessonId,
            ProgressPercentage = summary.ProgressPercentage
        };
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
            .Where(progress => progress.StudentId == studentId
                && progress.IsCompleted
                && progress.Lesson.Section.CourseId == courseId)
            .Select(progress => progress.LessonId)
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
