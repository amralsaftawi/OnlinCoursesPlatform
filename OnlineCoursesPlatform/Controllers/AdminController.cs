using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Services.Interfaces;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly UserManager<User> _userManager;

    public AdminController(IAdminService adminService, UserManager<User> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }
    public async Task<IActionResult> Index()
    {
        var data = await _adminService.GetDashboardAsync();
        return View(data);
    }

    public async Task<IActionResult> Users()
    {
        var users = await _adminService.GetUsersAsync();
        return View(users);
    }

    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var userToDelete = await _userManager.FindByIdAsync(id.ToString());

        if (userToDelete == null)
            return NotFound();

        if (currentUser.Id == id)
        {
            TempData["Error"] = "You cannot delete your own account!";
            return RedirectToAction(nameof(Users));
        }
        var roles = await _userManager.GetRolesAsync(currentUser);

        if (roles.Contains("Admin"))
        {
            TempData["Error"] = "Cannot delete another admin!";
            return RedirectToAction(nameof(Users));
        }

        await _adminService.DeleteUserAsync(id);
        return RedirectToAction(nameof(Users));
    }
    public async Task<IActionResult> MakeAdmin(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null) return NotFound();

        await _userManager.AddToRoleAsync(user, "Admin");

        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await _adminService.GetCoursesAsync();
        return View(courses);
    }

    public async Task<IActionResult> Approve(int id)
    {
        await _adminService.ApproveCourseAsync(id);
        return RedirectToAction(nameof(Courses));
    }

    public async Task<IActionResult> Reject(int id)
    {
        await _adminService.RejectCourseAsync(id);
        return RedirectToAction(nameof(Courses));
    }
}