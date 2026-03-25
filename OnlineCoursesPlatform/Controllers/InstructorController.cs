using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Dtos;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace OnlinCoursesPlatform.Controllers;

public class InstructorController(UserManager<User> userManager, IInstructorService instructorService, AppDbContext context) : Controller
{
    private readonly IInstructorService _instructorService=instructorService;
    private readonly UserManager<User> _userManager=userManager;
    private readonly AppDbContext _context = context;

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var stats = await _instructorService.GetInstructorStatsAsync(user.Id);
        return View(stats);
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> MyCourses()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var instructorCourses = await _instructorService.GetInstructorCoursesAsync(user.Id);
        return View(instructorCourses);
    }

    [HttpGet]
    public async Task<IActionResult> Profile(int id)
    {
        var instructor = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id);

        if (instructor == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        var canSeeAllCourses = User.IsInRole("Admin") || currentUserId == instructor.Id.ToString();

        var coursesQuery = _context.Courses
            .AsNoTracking()
            .Where(course => course.InstructorId == id);

        if (!canSeeAllCourses)
        {
            coursesQuery = coursesQuery.Where(course => course.Status == CourseStatus.Approved);
        }

        var instructorCourses = await coursesQuery
            .OrderByDescending(course => course.Id)
            .Select(course => new CourseListViewModel
            {
                Id = course.Id,
                InstructorId = course.InstructorId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                CategoryName = course.Category != null ? course.Category.Title : "General",
                ImageUrl = course.ImageUrl,
                CurrencySymbol = course.Currency != null ? course.Currency.Symbol : "$",
                InstructorName = $"{instructor.FirstName} {instructor.LastName}".Trim(),
                InstructorProfilePicture = instructor.ProfilePicture ?? "default-avatar.png"
            })
            .ToListAsync();

        var model = new InstructorPublicProfileViewModel
        {
            InstructorId = instructor.Id,
            InstructorName = $"{instructor.FirstName} {instructor.LastName}".Trim(),
            ProfilePicture = instructor.ProfilePicture,
            TotalCourses = instructorCourses.Count,
            TotalStudents = await _context.Enrollments.CountAsync(enrollment => enrollment.Course.InstructorId == id),
            Courses = instructorCourses
        };

        return View(model);
    }
}
