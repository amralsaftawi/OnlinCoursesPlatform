using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Dtos;
using OnlineCoursesPlatform.Models;

namespace OnlinCoursesPlatform.Controllers;

public class InstructorController(UserManager<User> userManager,IInstructorService instructorService) : Controller
{
    private readonly IInstructorService _instructorService=instructorService;
    private readonly UserManager<User> _userManager=userManager;

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
}