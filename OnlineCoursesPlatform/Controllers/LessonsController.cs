using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    public class LessonsController : Controller
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageContent(int courseId)
        {
            var result = await _lessonService.GetManageCourseContentAsync(courseId, GetCurrentUserId(), User.IsInRole("Admin"));
            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.IsForbidden)
            {
                return Forbid();
            }

            return View(result.ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(AddSectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add section. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            var result = await _lessonService.CreateSectionAsync(model, GetCurrentUserId(), User.IsInRole("Admin"));
            return HandleCourseContentActionResult(result, model.CourseId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(AddLessonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add lesson. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            var result = await _lessonService.CreateLessonAsync(model, GetCurrentUserId(), User.IsInRole("Admin"));
            return HandleCourseContentActionResult(result, model.CourseId);
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonForEdit(int lessonId)
        {
            var result = await _lessonService.GetLessonForEditAsync(lessonId, GetCurrentUserId(), User.IsInRole("Admin"));
            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.IsForbidden)
            {
                return Forbid();
            }

            return Json(new
            {
                lessonId = result.LessonId,
                sectionId = result.SectionId,
                title = result.Title,
                type = result.Type,
                contentUrl = result.ContentUrl,
                duration = result.Duration,
                isFree = result.IsFree
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(EditLessonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to update lesson. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            var result = await _lessonService.UpdateLessonAsync(model, GetCurrentUserId(), User.IsInRole("Admin"));
            return HandleCourseContentActionResult(result, model.CourseId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int lessonId, int courseId)
        {
            var result = await _lessonService.RemoveLessonAsync(lessonId, courseId, GetCurrentUserId(), User.IsInRole("Admin"));
            return HandleCourseContentActionResult(result, courseId);
        }

        private IActionResult HandleCourseContentActionResult(OnlineCoursesPlatform.Dtos.CourseContentActionResultDto result, int courseId)
        {
            if (result.NotFound)
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault() ?? "The requested course content could not be found.";
                return RedirectToAction(nameof(ManageContent), new { courseId });
            }

            if (result.IsForbidden)
            {
                return Forbid();
            }

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? result.Message
                : result.Errors.FirstOrDefault() ?? "Something went wrong while updating the curriculum.";

            return RedirectToAction(nameof(ManageContent), new { courseId });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
