using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlinCoursePlatform.Dtos;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILearningService _learningService;

        public EnrollmentController(IEnrollmentService enrollmentService, ILearningService learningService)
        {
            _enrollmentService = enrollmentService;
            _learningService = learningService;
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest request)
        {
            if (User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User not found." });
            }

            var studentId = int.Parse(userIdClaim);
            var result = await _enrollmentService.EnrollAsync(request.CourseId, studentId);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Errors.FirstOrDefault() ?? "Enrollment failed." });
            }

            return Ok(new { message = result.Message, firstLessonId = result.FirstLessonId });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyEnrolledCourses()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var studentId = int.Parse(userIdClaim);
            var myCourses = await _learningService.GetStudentLearningDashboardAsync(studentId);

            return View(myCourses);
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(int id)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _enrollmentService.GetConfirmationAsync(id, studentId);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.RedirectToCourseDetails)
            {
                if (!string.IsNullOrWhiteSpace(result.MessageKey) && !string.IsNullOrWhiteSpace(result.Message))
                {
                    TempData[result.MessageKey] = result.Message;
                }

                return RedirectToAction("Details", "Courses", new { id });
            }

            return View(result.Course);
        }
    }
}
