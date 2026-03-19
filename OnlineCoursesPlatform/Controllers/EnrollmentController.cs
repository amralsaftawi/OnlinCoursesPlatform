using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Dtos;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using System.Security.Claims;

namespace YourProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollInCourse([FromBody] EnrollmentRequest request)
        {
            // 1. Get the current logged-in Student ID from Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { Message = "User not found." });
            }

            int studentId = int.Parse(userIdClaim);

            // 2. Check if the course exists in the database
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId);
            if (!courseExists)
            {
                return NotFound(new { Message = "The specified course does not exist." });
            }

            // 3. Check for existing enrollment to prevent duplicates
            var isAlreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == request.CourseId);

            if (isAlreadyEnrolled)
            {
                return BadRequest(new { Message = "You are already enrolled in this course." });
            }

            // 4. Create a new enrollment record
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = request.CourseId,
                EnrolledAt = DateTime.UtcNow, // Default is handled by SQL but good to set here
                ProgressPercentage = 0.00m // Start with zero progress
            };

            try
            {
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Enrolled successfully! Happy learning." });
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { Message = "An error occurred while processing your enrollment." });
            }
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyEnrolledCourses()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int studentId = int.Parse(userIdClaim);

            // Fetching courses joined with Enrollment data
            var myCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .Select(e => new
                {
                    e.CourseId,
                    e.Course.Title,
                    e.Course.ImageUrl,
                    e.ProgressPercentage,
                    e.EnrolledAt
                })
                .ToListAsync();

            return View(myCourses);
        }


        public async Task<IActionResult> Confirm(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Currency) //
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            return View(course); // هيفتح Views/Enrollment/Confirm.cshtml
        }
    }

}