using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    public class LessonsController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;

        public LessonsController(ILessonService lessonService, ICourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageContent(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound();

            var sections = await _lessonService.GetSectionsByCourseIdAsync(courseId);

            var viewModel = new ManageCourseContentViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Sections = sections.ToList()
            };

            return View(viewModel);
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

            await _lessonService.AddSectionAsync(model.CourseId, model.Title);
            
            TempData["SuccessMessage"] = "Section added successfully!";
            return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
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

            await _lessonService.AddLessonAsync(
                model.SectionId, 
                model.Title, 
                model.Type, 
                model.ContentUrl, 
                model.Duration, 
                model.IsFree);

            TempData["SuccessMessage"] = "Lesson added successfully!";
            return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonForEdit(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            if (lesson == null) return NotFound();

            return Json(new {
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
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to update lesson. Please check your inputs.";
                return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
            }

            await _lessonService.UpdateLessonAsync(
                model.LessonId, 
                model.Title, 
                model.Type, 
                model.ContentUrl, 
                model.Duration, 
                model.IsFree);

            TempData["SuccessMessage"] = "Lesson updated successfully!";
            return RedirectToAction(nameof(ManageContent), new { courseId = model.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int lessonId, int courseId)
        {
            var success = await _lessonService.DeleteLessonAsync(lessonId);
            if (success)
            {
                TempData["SuccessMessage"] = "Lesson deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete lesson. It might not exist.";
            }

            return RedirectToAction(nameof(ManageContent), new { courseId = courseId });
        }
    }
}
