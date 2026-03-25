using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursesPlatform.Repositories.Interface;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Student,Instructor,Admin")]
    public class LearningController : Controller
    {
        private readonly ILearningService _learningService;

        public LearningController(ILearningService learningService)
        {
            _learningService = learningService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Challenge();
            }

            var userId = int.Parse(userIdClaim);
            var lessonExperience = await _learningService.GetLessonExperienceAsync(id, userId, User.IsInRole("Admin"));
            if (lessonExperience.NotFound)
            {
                return NotFound();
            }

            if (lessonExperience.RedirectCourseId.HasValue)
            {
                return RedirectToAction("Details", "Courses", new { id = lessonExperience.RedirectCourseId.Value });
            }

            return View(lessonExperience.ViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsComplete(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User not found." });
            }

            var userId = int.Parse(userIdClaim);
            var result = await _learningService.MarkLessonAsCompletedAsync(id, userId, User.IsInRole("Admin"));
            if (!result.Succeeded)
            {
                return BadRequest(new { success = false, message = result.Errors.FirstOrDefault() ?? "Could not save progress." });
            }

            return Ok(new
            {
                success = true,
                nextLessonId = result.NextLessonId,
                progressPercentage = result.ProgressPercentage
            });
        }
    }
}
