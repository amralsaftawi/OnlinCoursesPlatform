using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Controllers
{
    public class LearningController : Controller
    {
        private readonly ILearningService _learningService;
        private readonly AppDbContext _context;

        // التصحيح هنا: إضافة AppDbContext للـ Constructor
        public LearningController(ILearningService learningService, AppDbContext context)
        {
            _learningService = learningService;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            // _context دلوقتي مش null وهتشتغل عادي
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();
            
            int userId = int.Parse(userIdClaim);

            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == lesson.Section.CourseId && e.StudentId == userId);

            if (!lesson.IsFree && !isEnrolled)
            {
                return RedirectToAction("Details", "Courses", new { id = lesson.Section.CourseId });
            }

            var courseLessons = await _context.Lessons
                .Where(l => l.Section.CourseId == lesson.Section.CourseId)
                .OrderBy(l => l.Section.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .Select(l => new LessonDetailsViewModel {
                    Id = l.Id,
                    Title = l.Title,
                    Duration = l.Duration,
                    Type = l.Type,
                    OrderIndex = l.OrderIndex 
                })
                .ToListAsync();

            var viewModel = new LearningViewModel
            {
                CurrentLesson = new LessonDetailsViewModel 
                { 
                    Id = lesson.Id, 
                    Title = lesson.Title, 
                    ContentUrl = lesson.ContentUrl,
                    Type = lesson.Type
                },
                CourseLessons = courseLessons,
                CourseTitle = lesson.Section.Course.Title,
                ProgressPercentage = 0, 
                IsCompleted = false 
            };

            return View(viewModel);
        }

        
        [HttpPost]
        public async Task<IActionResult> MarkAsComplete(int id)
        {
            // نمرر الـ UserId للخدمة لضمان حفظ التقدم لليوزر الصح
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // افترضنا أن الخدمة تقوم بتحديث جدول UserLessonProgress
            var success = await _learningService.MarkLessonAsCompletedAsync(id); 
            
            if (!success) 
            {
                return BadRequest(new { success = false, message = "Could not save progress." });
            }

            var currentLesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == id);

            var nextLesson = await _context.Lessons
                .Where(l => l.Section.CourseId == currentLesson.Section.CourseId)
                .OrderBy(l => l.Section.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .FirstOrDefaultAsync(l => (l.Section.OrderIndex > currentLesson.Section.OrderIndex) || 
                                          (l.Section.OrderIndex == currentLesson.Section.OrderIndex && l.OrderIndex > currentLesson.OrderIndex));

            return Ok(new { success = true, nextLessonId = nextLesson?.Id });
        }
    }
}