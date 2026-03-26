using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Infrastructure;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineCoursesPlatform.Services
{
    public class LessonService : ILessonService
    {
        private static readonly string[] AllowedArticleExtensions = [".pdf"];

        private readonly IRepository<Section> _sectionRepository;
        private readonly IRepository<Lesson> _lessonRepository;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public LessonService(IRepository<Section> sectionRepository, IRepository<Lesson> lessonRepository, AppDbContext context, IWebHostEnvironment environment)
        {
            _sectionRepository = sectionRepository;
            _lessonRepository = lessonRepository;
            _context = context;
            _environment = environment;
        }

        public async Task<CourseContentViewResultDto> GetManageCourseContentAsync(int courseId, int? actingUserId, bool isAdmin)
        {
            var authorization = await AuthorizeCourseManagementAsync(courseId, actingUserId, isAdmin);
            if (authorization.NotFound || authorization.IsForbidden)
            {
                return new CourseContentViewResultDto
                {
                    NotFound = authorization.NotFound,
                    IsForbidden = authorization.IsForbidden
                };
            }

            var course = authorization.Course!;
            var sections = await GetSectionsByCourseIdAsync(courseId);
            return new CourseContentViewResultDto
            {
                ViewModel = new ManageCourseContentViewModel
                {
                    CourseId = courseId,
                    CourseTitle = course.Title,
                    Sections = sections.ToList()
                }
            };
        }

        public async Task<CourseContentActionResultDto> CreateSectionAsync(AddSectionViewModel model, int? actingUserId, bool isAdmin)
        {
            var authorization = await AuthorizeCourseManagementAsync(model.CourseId, actingUserId, isAdmin);
            if (authorization.NotFound || authorization.IsForbidden)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = authorization.NotFound,
                    IsForbidden = authorization.IsForbidden
                };
            }

            await AddSectionAsync(model.CourseId, model.Title);
            return new CourseContentActionResultDto
            {
                Succeeded = true,
                Message = "Section added successfully."
            };
        }

        public async Task<CourseContentActionResultDto> CreateLessonAsync(AddLessonViewModel model, int? actingUserId, bool isAdmin)
        {
            var authorization = await AuthorizeCourseManagementAsync(model.CourseId, actingUserId, isAdmin);
            if (authorization.NotFound || authorization.IsForbidden)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = authorization.NotFound,
                    IsForbidden = authorization.IsForbidden
                };
            }

            var sectionBelongsToCourse = await _context.Sections.AnyAsync(section => section.Id == model.SectionId && section.CourseId == model.CourseId);
            if (!sectionBelongsToCourse)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = true,
                    Errors = ["The selected section does not belong to this course."]
                };
            }

            LessonContentResolutionResult? resolvedContent = null;
            try
            {
                resolvedContent = await ResolveLessonContentAsync(model.Type, model.ContentUrl, model.ArticleFile);
                await AddLessonAsync(model.SectionId, model.Title, model.Type, resolvedContent.Content, model.Duration, model.IsFree);
                return new CourseContentActionResultDto
                {
                    Succeeded = true,
                    Message = "Lesson added successfully."
                };
            }
            catch (InvalidOperationException ex)
            {
                if (resolvedContent?.UploadedFilePath != null)
                {
                    LessonContentStorage.DeleteLocalArticleFile(_environment, resolvedContent.UploadedFilePath);
                }

                return new CourseContentActionResultDto
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
            catch
            {
                if (resolvedContent?.UploadedFilePath != null)
                {
                    LessonContentStorage.DeleteLocalArticleFile(_environment, resolvedContent.UploadedFilePath);
                }

                return new CourseContentActionResultDto
                {
                    Succeeded = false,
                    Errors = ["Something went wrong while saving the lesson. Please try again."]
                };
            }
        }

        public async Task<LessonEditorResultDto> GetLessonForEditAsync(int lessonId, int? actingUserId, bool isAdmin)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(item => item.Section)
                .FirstOrDefaultAsync(item => item.Id == lessonId);

            if (lesson == null)
            {
                return new LessonEditorResultDto { NotFound = true };
            }

            var authorization = await AuthorizeCourseManagementAsync(lesson.Section.CourseId, actingUserId, isAdmin);
            if (authorization.IsForbidden)
            {
                return new LessonEditorResultDto { IsForbidden = true };
            }

            return new LessonEditorResultDto
            {
                LessonId = lesson.Id,
                SectionId = lesson.SectionId,
                Title = lesson.Title,
                Type = (int)lesson.Type,
                ContentUrl = lesson.ContentUrl ?? string.Empty,
                Duration = lesson.Duration,
                IsFree = lesson.IsFree
            };
        }

        public async Task<CourseContentActionResultDto> UpdateLessonAsync(EditLessonViewModel model, int? actingUserId, bool isAdmin)
        {
            var authorization = await AuthorizeCourseManagementAsync(model.CourseId, actingUserId, isAdmin);
            if (authorization.NotFound || authorization.IsForbidden)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = authorization.NotFound,
                    IsForbidden = authorization.IsForbidden
                };
            }

            var lesson = await _context.Lessons
                .Include(item => item.Section)
                .FirstOrDefaultAsync(item => item.Id == model.LessonId);

            if (lesson == null || lesson.Section.CourseId != model.CourseId)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = true,
                    Errors = ["The lesson could not be found inside this course."]
                };
            }

            LessonContentResolutionResult? resolvedContent = null;
            try
            {
                resolvedContent = await ResolveLessonContentAsync(model.Type, model.ContentUrl, model.ArticleFile, lesson.ContentUrl);
                var previousArticlePath = lesson.Type == LessonType.Article ? lesson.ContentUrl : null;

                await UpdateLessonAsync(model.LessonId, model.Title, model.Type, resolvedContent.Content, model.Duration, model.IsFree);

                if (!string.IsNullOrWhiteSpace(previousArticlePath)
                    && !string.Equals(previousArticlePath, resolvedContent.Content, StringComparison.OrdinalIgnoreCase))
                {
                    LessonContentStorage.DeleteLocalArticleFile(_environment, previousArticlePath);
                }

                return new CourseContentActionResultDto
                {
                    Succeeded = true,
                    Message = "Lesson updated successfully."
                };
            }
            catch (InvalidOperationException ex)
            {
                if (resolvedContent?.UploadedFilePath != null)
                {
                    LessonContentStorage.DeleteLocalArticleFile(_environment, resolvedContent.UploadedFilePath);
                }

                return new CourseContentActionResultDto
                {
                    Succeeded = false,
                    Errors = [ex.Message]
                };
            }
            catch
            {
                if (resolvedContent?.UploadedFilePath != null)
                {
                    LessonContentStorage.DeleteLocalArticleFile(_environment, resolvedContent.UploadedFilePath);
                }

                return new CourseContentActionResultDto
                {
                    Succeeded = false,
                    Errors = ["Something went wrong while updating the lesson. Please try again."]
                };
            }
        }

        public async Task<CourseContentActionResultDto> RemoveLessonAsync(int lessonId, int courseId, int? actingUserId, bool isAdmin)
        {
            var authorization = await AuthorizeCourseManagementAsync(courseId, actingUserId, isAdmin);
            if (authorization.NotFound || authorization.IsForbidden)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = authorization.NotFound,
                    IsForbidden = authorization.IsForbidden
                };
            }

            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(item => item.Section)
                .FirstOrDefaultAsync(item => item.Id == lessonId);

            if (lesson == null || lesson.Section.CourseId != courseId)
            {
                return new CourseContentActionResultDto
                {
                    NotFound = true,
                    Errors = ["The lesson does not belong to this course."]
                };
            }

            var success = await DeleteLessonAsync(lessonId);
            return new CourseContentActionResultDto
            {
                Succeeded = success,
                Errors = success ? Array.Empty<string>() : ["Failed to delete lesson. It might not exist."],
                Message = success ? "Lesson deleted successfully." : null
            };
        }

        public async Task<Section> AddSectionAsync(int courseId, string title)
        {
            // Calculate the next OrderIndex for the section
            var existingSectionsCount = await _sectionRepository.GetQueryable()
                .Where(s => s.CourseId == courseId)
                .CountAsync();

            var section = new Section
            {
                CourseId = courseId,
                Title = title,
                OrderIndex = existingSectionsCount + 1
            };

            var createdSection = await _sectionRepository.AddAsync(section);
            await _sectionRepository.SaveAsync();

            return createdSection;
        }

        public async Task<Lesson> AddLessonAsync(int sectionId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree)
        {
            var courseId = await _context.Sections
                .Where(s => s.Id == sectionId)
                .Select(s => s.CourseId)
                .FirstOrDefaultAsync();

            if (courseId == 0)
                throw new InvalidOperationException("Section not found.");

            // Calculate the next OrderIndex for the lesson in the section
            var existingLessonsCount = await _lessonRepository.GetQueryable()
                .Where(l => l.SectionId == sectionId)
                .CountAsync();

            var lesson = new Lesson
            {
                SectionId = sectionId,
                Title = title,
                Type = type,
                ContentUrl = contentUrl,
                Duration = duration,
                IsFree = isFree,
                OrderIndex = existingLessonsCount + 1
            };

            var createdLesson = await _lessonRepository.AddAsync(lesson);
            await _lessonRepository.SaveAsync();
            await RecalculateCourseDurationAsync(courseId);

            return createdLesson;
        }

        public async Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(int courseId)
        {
            return await _sectionRepository.GetQueryable()
                .Where(s => s.CourseId == courseId)
                .Include(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .OrderBy(s => s.OrderIndex)
                .ToListAsync();
        }

        public async Task<Lesson> GetLessonByIdAsync(int lessonId)
        {
            return await _lessonRepository.GetByIdAsync(lessonId);
        }

        public async Task<Lesson> UpdateLessonAsync(int lessonId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return null;

            lesson.Title = title;
            lesson.Type = type;
            lesson.ContentUrl = contentUrl;
            lesson.Duration = duration;
            lesson.IsFree = isFree;

            var updatedLesson = await _lessonRepository.UpdateAsync(lesson);
            await _lessonRepository.SaveAsync();
            var courseId = await _context.Sections.Where(s => s.Id == lesson.SectionId).Select(s => s.CourseId).FirstAsync();
            await RecalculateCourseDurationAsync(courseId);

            return updatedLesson;
        }

        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            var lessonInfo = await _context.Lessons
                .Where(l => l.Id == lessonId)
                .Select(l => new
                {
                    CourseId = l.Section.CourseId,
                    l.ContentUrl,
                    l.Type
                })
                .FirstOrDefaultAsync();

            if (lessonInfo == null)
            {
                return false;
            }

            var lessonProgresses = await _context.UserProgresses
                .Where(progress => progress.LessonId == lessonId)
                .ToListAsync();

            if (lessonProgresses.Count != 0)
            {
                _context.UserProgresses.RemoveRange(lessonProgresses);
                await _context.SaveChangesAsync();
            }

            var result = await _lessonRepository.DeleteAsync(lessonId);
            if (result)
            {
                await _lessonRepository.SaveAsync();
                if (lessonInfo.CourseId > 0)
                {
                    await RecalculateCourseDurationAsync(lessonInfo.CourseId);
                }

                if (lessonInfo.Type == LessonType.Article)
                {
                    LessonContentStorage.DeleteLocalArticleFile(_environment, lessonInfo.ContentUrl);
                }
            }
            return result;
        }

        public async Task RecalculateCourseDurationAsync(int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                return;

            course.TotalDuration = await _context.Lessons
                .Where(l => l.Section.CourseId == courseId)
                .SumAsync(l => (int?)l.Duration) ?? 0;

            await _context.SaveChangesAsync();
        }

        private async Task<LessonContentResolutionResult> ResolveLessonContentAsync(LessonType type, string? contentUrl, IFormFile? articleFile, string? existingContent = null)
        {
            if (type != LessonType.Article)
            {
                return new LessonContentResolutionResult
                {
                    Content = (contentUrl ?? string.Empty).Trim()
                };
            }

            if (articleFile != null && articleFile.Length > 0)
            {
                var uploadedFilePath = await SaveLocalArticleFileAsync(articleFile);
                return new LessonContentResolutionResult
                {
                    Content = uploadedFilePath,
                    UploadedFilePath = uploadedFilePath
                };
            }

            var candidateContent = string.IsNullOrWhiteSpace(contentUrl) ? existingContent : contentUrl;
            var resolvedContent = (candidateContent ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(resolvedContent))
            {
                throw new InvalidOperationException("Please upload a PDF file for the article lesson.");
            }

            if ((Uri.IsWellFormedUriString(resolvedContent, UriKind.Absolute) || resolvedContent.StartsWith('/'))
                && !IsPdfContentUrl(resolvedContent))
            {
                throw new InvalidOperationException("Article lessons only support PDF files.");
            }

            return new LessonContentResolutionResult
            {
                Content = resolvedContent
            };
        }

        private async Task<string> SaveLocalArticleFileAsync(IFormFile articleFile)
        {
            if (articleFile.Length == 0)
            {
                throw new InvalidOperationException("The uploaded article PDF is empty.");
            }

            var extension = Path.GetExtension(articleFile.FileName);
            if (!AllowedArticleExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Article uploads only support PDF files.");
            }

            var uploadsFolder = LessonContentStorage.GetArticleUploadsPhysicalPath(_environment);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await articleFile.CopyToAsync(fileStream);

            return LessonContentStorage.BuildRelativeArticlePath(uniqueFileName);
        }

        private static bool IsPdfContentUrl(string url)
        {
            var normalizedUrl = url.Split('?')[0].Split('#')[0];
            return normalizedUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<CourseAuthorizationResult> AuthorizeCourseManagementAsync(int courseId, int? actingUserId, bool isAdmin)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == courseId);

            if (course == null)
            {
                return new CourseAuthorizationResult { NotFound = true };
            }

            if (isAdmin)
            {
                return new CourseAuthorizationResult { Course = course };
            }

            if (!actingUserId.HasValue || course.InstructorId != actingUserId.Value)
            {
                return new CourseAuthorizationResult { IsForbidden = true };
            }

            return new CourseAuthorizationResult { Course = course };
        }

        private sealed class CourseAuthorizationResult
        {
            public bool NotFound { get; init; }
            public bool IsForbidden { get; init; }
            public Course? Course { get; init; }
        }

        private sealed class LessonContentResolutionResult
        {
            public string Content { get; init; } = string.Empty;
            public string? UploadedFilePath { get; init; }
        }
    }
}
