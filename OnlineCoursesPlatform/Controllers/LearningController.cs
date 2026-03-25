using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Student,Instructor,Admin")]
    public class LearningController : Controller
    {
        private readonly ILearningService _learningService;
        private readonly AppDbContext _context;

        public LearningController(ILearningService learningService, AppDbContext context)
        {
            _learningService = learningService;
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _learningService.GetLessonDetailsAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Challenge();
            }

            var userId = int.Parse(userIdClaim);
            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == lesson.Section.CourseId && e.StudentId == userId);
            var isOwner = lesson.Section.Course.InstructorId == userId;
            var isAdmin = User.IsInRole("Admin");

            if (!lesson.IsFree && !isEnrolled && !isOwner && !isAdmin)
            {
                return RedirectToAction("Details", "Courses", new { id = lesson.Section.CourseId });
            }

            var courseLessons = (await _learningService.GetCourseLessonsAsync(lesson.Section.CourseId))
                .Select(l => new LessonDetailsViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Duration = l.Duration,
                    Type = l.Type,
                    OrderIndex = l.OrderIndex,
                    IsFree = l.IsFree
                })
                .ToList();

            var progressPercentage = isEnrolled
                ? await _learningService.GetProgressPercentageAsync(lesson.Section.CourseId, userId)
                : 0;
            var isCompleted = isEnrolled && await _learningService.IsLessonCompletedAsync(lesson.Id, userId);

            var viewModel = new LearningViewModel
            {
                CourseId = lesson.Section.CourseId,
                CurrentLesson = new LessonDetailsViewModel
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    ContentUrl = lesson.ContentUrl,
                    Type = lesson.Type,
                    OrderIndex = lesson.OrderIndex,
                    IsFree = lesson.IsFree,
                    Duration = lesson.Duration
                },
                CourseLessons = courseLessons,
                CourseTitle = lesson.Section.Course.Title,
                ProgressPercentage = progressPercentage,
                IsCompleted = isCompleted,
                IsOwnerPreview = isOwner && !isEnrolled
            };

            return View(viewModel);
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
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound(new { success = false, message = "Lesson not found." });
            }

            if (lesson.Section.Course.InstructorId == userId)
            {
                return BadRequest(new { success = false, message = "Course owners can preview lessons, but progress is tracked only for enrolled students." });
            }

            var success = await _learningService.MarkLessonAsCompletedAsync(id, userId);
            if (!success)
            {
                return BadRequest(new { success = false, message = "Could not save progress." });
            }

            var nextLesson = await _context.Lessons
                .Where(l => l.Section.CourseId == lesson.Section.CourseId)
                .OrderBy(l => l.Section.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .FirstOrDefaultAsync(l =>
                    l.Section.OrderIndex > lesson.Section.OrderIndex ||
                    (l.Section.OrderIndex == lesson.Section.OrderIndex && l.OrderIndex > lesson.OrderIndex));

            var progressPercentage = await _learningService.GetProgressPercentageAsync(lesson.Section.CourseId, userId);

            return Ok(new
            {
                success = true,
                nextLessonId = nextLesson?.Id,
                progressPercentage
            });
        }
    }
}
