using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlinCoursePlatform.Abstrctions;
using System.Security.Claims;

namespace OnlinCoursesPlatform.Controllers;

public class InstructorController : Controller
{
    private readonly IInstructorService _instructorService;

    public InstructorController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Index()
    {
        var instructorId = GetCurrentUserId();
        if (!instructorId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var stats = await _instructorService.GetInstructorStatsAsync(instructorId.Value);
        return View(stats);
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> MyCourses()
    {
        var instructorId = GetCurrentUserId();
        if (!instructorId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var instructorCourses = await _instructorService.GetInstructorCoursesAsync(instructorId.Value);
        return View(instructorCourses);
    }

    [HttpGet]
    public async Task<IActionResult> Profile(int id)
    {
        var profile = await _instructorService.GetInstructorProfileAsync(id, GetCurrentUserId(), User.IsInRole("Admin"));
        return profile == null ? NotFound() : View(profile);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
