using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Infrastructure;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Repositories.Interfaces;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services;

public class LessonService : ILessonService
{
    private static readonly string[] AllowedArticleExtensions = [".txt"];
    private static readonly string[] AllowedPdfExtensions = [".pdf"];

    private readonly IRepository<Section> _sectionRepository;
    private readonly IRepository<Lesson> _lessonRepository;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IMapper _mapper;

    public LessonService(
        IRepository<Section> sectionRepository,
        IRepository<Lesson> lessonRepository,
        AppDbContext context,
        IWebHostEnvironment environment,
        IMapper mapper)
    {
        _sectionRepository = sectionRepository;
        _lessonRepository = lessonRepository;
        _context = context;
        _environment = environment;
        _mapper = mapper;
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

        var sectionBelongsToCourse = await _context.Sections
            .AnyAsync(section => section.Id == model.SectionId && section.CourseId == model.CourseId);

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
            resolvedContent = await ResolveLessonContentAsync(new LessonContentRequest
            {
                Type = model.Type,
                ContentUrl = model.ContentUrl,
                ArticleFile = model.ArticleFile,
                PdfFile = model.PdfFile,
                QuizMode = model.QuizMode,
                QuizPrompt = model.QuizPrompt,
                QuizOptionA = model.QuizOptionA,
                QuizOptionB = model.QuizOptionB,
                QuizOptionC = model.QuizOptionC,
                QuizOptionD = model.QuizOptionD,
                QuizCorrectOption = model.QuizCorrectOption,
                QuizCorrectTrueFalse = model.QuizCorrectTrueFalse,
                QuizReferenceAnswer = model.QuizReferenceAnswer
            });

            await AddLessonAsync(model.SectionId, model.Title, model.Type, resolvedContent.Content, model.Duration, model.IsFree);

            return new CourseContentActionResultDto
            {
                Succeeded = true,
                Message = "Lesson added successfully."
            };
        }
        catch (InvalidOperationException ex)
        {
            CleanupNewManagedFile(resolvedContent);
            return new CourseContentActionResultDto
            {
                Errors = [ex.Message]
            };
        }
        catch
        {
            CleanupNewManagedFile(resolvedContent);
            return new CourseContentActionResultDto
            {
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

        var result = _mapper.Map<LessonEditorResultDto>(lesson);

        if (lesson.Type != LessonType.Quiz)
        {
            return result;
        }

        var quiz = LessonQuizContentSerializer.Deserialize(lesson.ContentUrl);
        if (quiz == null)
        {
            return result;
        }

        return new LessonEditorResultDto
        {
            NotFound = result.NotFound,
            IsForbidden = result.IsForbidden,
            LessonId = result.LessonId,
            SectionId = result.SectionId,
            Title = result.Title,
            Type = result.Type,
            ContentUrl = result.ContentUrl,
            Duration = result.Duration,
            IsFree = result.IsFree,
            QuizQuestionType = (int)quiz.QuestionType,
            QuizPrompt = quiz.Prompt,
            QuizOptionA = quiz.Options.ElementAtOrDefault(0) ?? string.Empty,
            QuizOptionB = quiz.Options.ElementAtOrDefault(1) ?? string.Empty,
            QuizOptionC = quiz.Options.ElementAtOrDefault(2) ?? string.Empty,
            QuizOptionD = quiz.Options.ElementAtOrDefault(3) ?? string.Empty,
            QuizCorrectOption = quiz.QuestionType == QuizQuestionType.MultipleChoice
                ? GetCorrectOptionIndex(quiz)
                : null,
            QuizCorrectTrueFalse = quiz.QuestionType == QuizQuestionType.TrueFalse
                ? bool.TryParse(quiz.CorrectAnswer, out var answer) ? answer : null
                : null,
            QuizReferenceAnswer = quiz.QuestionType == QuizQuestionType.Written
                ? quiz.ReferenceAnswer
                : string.Empty
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
        var previousManagedContent = LessonContentStorage.IsManagedLessonUpload(lesson.ContentUrl)
            ? lesson.ContentUrl
            : null;

        try
        {
            resolvedContent = await ResolveLessonContentAsync(
                new LessonContentRequest
                {
                    Type = model.Type,
                    ContentUrl = model.ContentUrl,
                    ArticleFile = model.ArticleFile,
                    PdfFile = model.PdfFile,
                    QuizMode = model.QuizMode,
                    QuizPrompt = model.QuizPrompt,
                    QuizOptionA = model.QuizOptionA,
                    QuizOptionB = model.QuizOptionB,
                    QuizOptionC = model.QuizOptionC,
                    QuizOptionD = model.QuizOptionD,
                    QuizCorrectOption = model.QuizCorrectOption,
                    QuizCorrectTrueFalse = model.QuizCorrectTrueFalse,
                    QuizReferenceAnswer = model.QuizReferenceAnswer
                },
                lesson.ContentUrl);

            await UpdateLessonAsync(model.LessonId, model.Title, model.Type, resolvedContent.Content, model.Duration, model.IsFree);

            if (!string.IsNullOrWhiteSpace(previousManagedContent)
                && !string.Equals(previousManagedContent, resolvedContent.Content, StringComparison.OrdinalIgnoreCase))
            {
                LessonContentStorage.DeleteManagedLessonFile(_environment, previousManagedContent);
            }

            return new CourseContentActionResultDto
            {
                Succeeded = true,
                Message = "Lesson updated successfully."
            };
        }
        catch (InvalidOperationException ex)
        {
            CleanupNewManagedFile(resolvedContent);
            return new CourseContentActionResultDto
            {
                Errors = [ex.Message]
            };
        }
        catch
        {
            CleanupNewManagedFile(resolvedContent);
            return new CourseContentActionResultDto
            {
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
        var existingSectionsCount = await _sectionRepository.GetQueryable()
            .Where(section => section.CourseId == courseId)
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

    public async Task<Lesson> AddLessonAsync(int sectionId, string title, LessonType type, string contentUrl, int duration, bool isFree)
    {
        var courseId = await _context.Sections
            .Where(section => section.Id == sectionId)
            .Select(section => section.CourseId)
            .FirstOrDefaultAsync();

        if (courseId == 0)
        {
            throw new InvalidOperationException("Section not found.");
        }

        var existingLessonsCount = await _lessonRepository.GetQueryable()
            .Where(lesson => lesson.SectionId == sectionId)
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
            .Where(section => section.CourseId == courseId)
            .Include(section => section.Lessons.OrderBy(lesson => lesson.OrderIndex))
            .OrderBy(section => section.OrderIndex)
            .ToListAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(int lessonId)
    {
        return await _lessonRepository.GetByIdAsync(lessonId);
    }

    public async Task<Lesson?> UpdateLessonAsync(int lessonId, string title, LessonType type, string contentUrl, int duration, bool isFree)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            return null;
        }

        lesson.Title = title;
        lesson.Type = type;
        lesson.ContentUrl = contentUrl;
        lesson.Duration = duration;
        lesson.IsFree = isFree;

        var updatedLesson = await _lessonRepository.UpdateAsync(lesson);
        await _lessonRepository.SaveAsync();

        var courseId = await _context.Sections
            .Where(section => section.Id == lesson.SectionId)
            .Select(section => section.CourseId)
            .FirstAsync();

        await RecalculateCourseDurationAsync(courseId);
        return updatedLesson;
    }

    public async Task<bool> DeleteLessonAsync(int lessonId)
    {
        var lessonInfo = await _context.Lessons
            .Where(lesson => lesson.Id == lessonId)
            .Select(lesson => new
            {
                CourseId = lesson.Section.CourseId,
                lesson.ContentUrl
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
        if (!result)
        {
            return false;
        }

        await _lessonRepository.SaveAsync();

        if (lessonInfo.CourseId > 0)
        {
            await RecalculateCourseDurationAsync(lessonInfo.CourseId);
        }

        LessonContentStorage.DeleteManagedLessonFile(_environment, lessonInfo.ContentUrl);
        return true;
    }

    public async Task RecalculateCourseDurationAsync(int courseId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(item => item.Id == courseId);
        if (course == null)
        {
            return;
        }

        course.TotalDuration = await _context.Lessons
            .Where(lesson => lesson.Section.CourseId == courseId)
            .SumAsync(lesson => (int?)lesson.Duration) ?? 0;

        await _context.SaveChangesAsync();
    }

    private async Task<LessonContentResolutionResult> ResolveLessonContentAsync(LessonContentRequest request, string? existingContent = null)
    {
        return request.Type switch
        {
            LessonType.Article => await ResolveArticleContentAsync(request, existingContent),
            LessonType.PDF => await ResolvePdfContentAsync(request, existingContent),
            LessonType.Quiz => ResolveQuizContent(request),
            _ => ResolveVideoContent(request)
        };
    }

    private LessonContentResolutionResult ResolveVideoContent(LessonContentRequest request)
    {
        var content = (request.ContentUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Please provide the lesson video URL.");
        }

        return new LessonContentResolutionResult
        {
            Content = content
        };
    }

    private async Task<LessonContentResolutionResult> ResolveArticleContentAsync(LessonContentRequest request, string? existingContent)
    {
        if (request.ArticleFile != null && request.ArticleFile.Length > 0)
        {
            var uploadedFilePath = await SaveLocalArticleFileAsync(request.ArticleFile);
            return new LessonContentResolutionResult
            {
                Content = uploadedFilePath,
                UploadedFilePath = uploadedFilePath
            };
        }

        var content = string.IsNullOrWhiteSpace(request.ContentUrl)
            ? (existingContent ?? string.Empty).Trim()
            : request.ContentUrl.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Please upload a `.txt` file for the article lesson.");
        }

        return new LessonContentResolutionResult
        {
            Content = content
        };
    }

    private async Task<LessonContentResolutionResult> ResolvePdfContentAsync(LessonContentRequest request, string? existingContent)
    {
        if (request.PdfFile != null && request.PdfFile.Length > 0)
        {
            var uploadedFilePath = await SaveLocalPdfFileAsync(request.PdfFile);
            return new LessonContentResolutionResult
            {
                Content = uploadedFilePath,
                UploadedFilePath = uploadedFilePath
            };
        }

        var content = string.IsNullOrWhiteSpace(request.ContentUrl)
            ? (existingContent ?? string.Empty).Trim()
            : request.ContentUrl.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Please upload a PDF file or paste a direct PDF URL.");
        }

        if (!IsPdfContent(content))
        {
            throw new InvalidOperationException("PDF lessons only support direct `.pdf` files or uploaded PDFs.");
        }

        return new LessonContentResolutionResult
        {
            Content = content
        };
    }

    private LessonContentResolutionResult ResolveQuizContent(LessonContentRequest request)
    {
        if (!request.QuizMode.HasValue)
        {
            throw new InvalidOperationException("Please choose the quiz type.");
        }

        var prompt = (request.QuizPrompt ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new InvalidOperationException("Please write the quiz prompt.");
        }

        string serializedContent;

        switch (request.QuizMode.Value)
        {
            case QuizQuestionType.TrueFalse:
                if (!request.QuizCorrectTrueFalse.HasValue)
                {
                    throw new InvalidOperationException("Please choose whether the correct answer is true or false.");
                }

                serializedContent = LessonQuizContentSerializer.Serialize(
                    request.QuizMode.Value,
                    prompt,
                    ["True", "False"],
                    request.QuizCorrectTrueFalse.Value ? "True" : "False",
                    null);
                break;

            case QuizQuestionType.MultipleChoice:
                var options = new[]
                {
                    request.QuizOptionA,
                    request.QuizOptionB,
                    request.QuizOptionC,
                    request.QuizOptionD
                }
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Select(option => option!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

                if (options.Count < 2)
                {
                    throw new InvalidOperationException("Please provide at least two answer choices for the quiz.");
                }

                if (!request.QuizCorrectOption.HasValue || request.QuizCorrectOption.Value > options.Count)
                {
                    throw new InvalidOperationException("Please select which choice is correct.");
                }

                serializedContent = LessonQuizContentSerializer.Serialize(
                    request.QuizMode.Value,
                    prompt,
                    options,
                    options[request.QuizCorrectOption.Value - 1],
                    null);
                break;

            default:
                serializedContent = LessonQuizContentSerializer.Serialize(
                    request.QuizMode.Value,
                    prompt,
                    Array.Empty<string>(),
                    string.Empty,
                    request.QuizReferenceAnswer);
                break;
        }

        return new LessonContentResolutionResult
        {
            Content = serializedContent
        };
    }

    private async Task<string> SaveLocalArticleFileAsync(IFormFile articleFile)
    {
        if (articleFile.Length == 0)
        {
            throw new InvalidOperationException("The uploaded article text file is empty.");
        }

        var extension = Path.GetExtension(articleFile.FileName);
        if (!AllowedArticleExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Article uploads only support `.txt` files.");
        }

        var uploadsFolder = LessonContentStorage.GetArticleUploadsPhysicalPath(_environment);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await articleFile.CopyToAsync(fileStream);

        return LessonContentStorage.BuildRelativeArticlePath(uniqueFileName);
    }

    private async Task<string> SaveLocalPdfFileAsync(IFormFile pdfFile)
    {
        if (pdfFile.Length == 0)
        {
            throw new InvalidOperationException("The uploaded PDF file is empty.");
        }

        var extension = Path.GetExtension(pdfFile.FileName);
        if (!AllowedPdfExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("PDF uploads only support `.pdf` files.");
        }

        var uploadsFolder = LessonContentStorage.GetPdfUploadsPhysicalPath(_environment);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await pdfFile.CopyToAsync(fileStream);

        return LessonContentStorage.BuildRelativePdfPath(uniqueFileName);
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

    private static bool IsPdfContent(string content)
    {
        if (LessonContentStorage.IsLocalPdfUpload(content))
        {
            return true;
        }

        var normalized = content.Split('?')[0].Split('#')[0];
        return normalized.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private static int? GetCorrectOptionIndex(LessonQuizContentPayload quiz)
    {
        for (var index = 0; index < quiz.Options.Count; index++)
        {
            if (string.Equals(quiz.Options[index], quiz.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
            {
                return index + 1;
            }
        }

        return null;
    }

    private void CleanupNewManagedFile(LessonContentResolutionResult? result)
    {
        if (result?.UploadedFilePath != null)
        {
            LessonContentStorage.DeleteManagedLessonFile(_environment, result.UploadedFilePath);
        }
    }

    private sealed class CourseAuthorizationResult
    {
        public bool NotFound { get; init; }
        public bool IsForbidden { get; init; }
        public Course? Course { get; init; }
    }

    private sealed class LessonContentRequest
    {
        public LessonType Type { get; init; }
        public string? ContentUrl { get; init; }
        public IFormFile? ArticleFile { get; init; }
        public IFormFile? PdfFile { get; init; }
        public QuizQuestionType? QuizMode { get; init; }
        public string? QuizPrompt { get; init; }
        public string? QuizOptionA { get; init; }
        public string? QuizOptionB { get; init; }
        public string? QuizOptionC { get; init; }
        public string? QuizOptionD { get; init; }
        public int? QuizCorrectOption { get; init; }
        public bool? QuizCorrectTrueFalse { get; init; }
        public string? QuizReferenceAnswer { get; init; }
    }

    private sealed class LessonContentResolutionResult
    {
        public string Content { get; init; } = string.Empty;
        public string? UploadedFilePath { get; init; }
    }
}
