using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Dtos;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;

namespace OnlineCoursesPlatform.Controllers
{
    [Authorize(Roles = "Student")]
    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User not found." });
            }

            var studentId = int.Parse(userIdClaim);
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(c => c.Id == request.CourseId);

            if (course == null)
            {
                return NotFound(new { message = "The specified course does not exist." });
            }

            if (course.Status != CourseStatus.Approved)
            {
                return BadRequest(new { message = "This course is not available for enrollment yet." });
            }

            if (course.InstructorId == studentId)
            {
                return BadRequest(new { message = "You already own this course. Use the instructor tools to manage it instead." });
            }

            var isAlreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == request.CourseId);
            if (isAlreadyEnrolled)
            {
                return BadRequest(new { message = "You are already enrolled in this course." });
            }

            _context.Enrollments.Add(new Models.Enrollment
            {
                StudentId = studentId,
                CourseId = request.CourseId,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0
            });

            await _context.SaveChangesAsync();

            var firstLessonId = course.Sections.SelectMany(s => s.Lessons).OrderBy(l => l.OrderIndex).Select(l => (int?)l.Id).FirstOrDefault();
            return Ok(new { message = "Enrolled successfully! Happy learning.", firstLessonId });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyEnrolledCourses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var studentId = int.Parse(userIdClaim);
            var myCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .Select(e => new EnrolledCourseViewModel
                {
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    ImageUrl = e.Course.ImageUrl,
                    ProgressPercentage = (int)Math.Round(e.ProgressPercentage, MidpointRounding.AwayFromZero),
                    EnrolledAt = e.EnrolledAt,
                    InstructorName = (e.Course.Instructor.FirstName + " " + e.Course.Instructor.LastName).Trim(),
                    FirstLessonId = e.Course.Sections
                        .OrderBy(section => section.OrderIndex)
                        .SelectMany(section => section.Lessons.OrderBy(lesson => lesson.OrderIndex))
                        .Select(lesson => (int?)lesson.Id)
                        .FirstOrDefault()
                })
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return View(myCourses);
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Currency)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (course.Status != CourseStatus.Approved)
                return NotFound();

            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (course.InstructorId == studentId)
            {
                TempData["Error"] = "You own this course already. Open it from your instructor workspace instead of enrolling.";
                return RedirectToAction("Details", "Courses", new { id });
            }

            var isAlreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == id);
            if (isAlreadyEnrolled)
            {
                TempData["Success"] = "You are already enrolled in this course.";
                return RedirectToAction("Details", "Courses", new { id });
            }

            return View(course);
        }
    }
}
