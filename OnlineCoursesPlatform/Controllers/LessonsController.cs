using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;
using System.Text;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    public class LessonsController : Controller
    {
        private static readonly string[] AllowedArticleExtensions = [".txt", ".md"];

        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;
        private readonly AppDbContext _context;

        public LessonsController(ILessonService lessonService, ICourseService courseService, AppDbContext context)
        {
            _lessonService = lessonService;
            _courseService = courseService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ManageContent(int courseId)
        {
            if (!await CanManageCourseAsync(courseId))
            {
                return Forbid();
            }

            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var sections = await _lessonService.GetSectionsByCourseIdAsync(courseId);
            return View(new ManageCourseContentViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Sections = sections.ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(AddSectionViewModel model)
        {
            if (!await CanManageCourseAsync(model.CourseId))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add section. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            await _lessonService.AddSectionAsync(model.CourseId, model.Title);
            TempData["SuccessMessage"] = "Section added successfully.";
            return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(AddLessonViewModel model)
        {
            if (!await CanManageCourseAsync(model.CourseId))
            {
                return Forbid();
            }

            var sectionBelongsToCourse = await _context.Sections.AnyAsync(s => s.Id == model.SectionId && s.CourseId == model.CourseId);
            if (!sectionBelongsToCourse)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add lesson. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            try
            {
                var content = await ResolveLessonContentAsync(model.Type, model.ContentUrl, model.ArticleFile);
                await _lessonService.AddLessonAsync(model.SectionId, model.Title, model.Type, content, model.Duration, model.IsFree);
                TempData["SuccessMessage"] = "Lesson added successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonForEdit(int lessonId)
        {
            var lesson = await _context.Lessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            if (!await CanManageCourseAsync(lesson.Section.CourseId))
            {
                return Forbid();
            }

            return Json(new
            {
                lessonId = lesson.Id,
                sectionId = lesson.SectionId,
                title = lesson.Title,
                type = (int)lesson.Type,
                contentUrl = lesson.ContentUrl,
                duration = lesson.Duration,
                isFree = lesson.IsFree
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(EditLessonViewModel model)
        {
            if (!await CanManageCourseAsync(model.CourseId))
            {
                return Forbid();
            }

            var lesson = await _context.Lessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == model.LessonId);
            if (lesson == null || lesson.Section.CourseId != model.CourseId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to update lesson. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            try
            {
                var content = await ResolveLessonContentAsync(model.Type, model.ContentUrl, model.ArticleFile, lesson.ContentUrl);
                await _lessonService.UpdateLessonAsync(model.LessonId, model.Title, model.Type, content, model.Duration, model.IsFree);
                TempData["SuccessMessage"] = "Lesson updated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int lessonId, int courseId)
        {
            if (!await CanManageCourseAsync(courseId))
            {
                return Forbid();
            }

            var lesson = await _context.Lessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null || lesson.Section.CourseId != courseId)
            {
                return BadRequest();
            }

            var success = await _lessonService.DeleteLessonAsync(lessonId);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Lesson deleted successfully."
                : "Failed to delete lesson. It might not exist.";

            return RedirectToAction(nameof(ManageContent), new { courseId });
        }

        private async Task<string> ResolveLessonContentAsync(LessonType type, string? contentUrl, IFormFile? articleFile, string? existingContent = null)
        {
            if (type != LessonType.Article)
            {
                return (contentUrl ?? string.Empty).Trim();
            }

            if (articleFile != null && articleFile.Length > 0)
            {
                var extension = Path.GetExtension(articleFile.FileName);
                if (!AllowedArticleExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Article uploads currently support .txt and .md files only.");
                }

                using var streamReader = new StreamReader(articleFile.OpenReadStream(), Encoding.UTF8);
                var fileContent = await streamReader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    throw new InvalidOperationException("The uploaded article file is empty.");
                }

                return fileContent.Trim();
            }

            var candidateContent = string.IsNullOrWhiteSpace(contentUrl) ? existingContent : contentUrl;
            return (candidateContent ?? string.Empty).Trim();
        }

        private async Task<bool> CanManageCourseAsync(int courseId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return false;
            }

            var userId = int.Parse(userIdClaim);
            var course = await _courseService.GetCourseByIdAsync(courseId);
            return course != null && course.InstructorId == userId;
        }
    }
}
