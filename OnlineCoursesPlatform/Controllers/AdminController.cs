using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;

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
        ViewBag.CurrentAdminId = GetCurrentUserId();
        var users = await _adminService.GetUsersAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRoles(AdminUserRoleUpdateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault() ?? "Please review the role settings and try again.";
                return RedirectToAction(nameof(Users));
            }

            var currentUserId = GetCurrentUserId();
            await _adminService.UpdateUserRolesAsync(model.Id, model.PrimaryRole, model.IsAdmin, currentUserId);
            TempData["SuccessMessage"] = "User roles updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userToDelete = await _userManager.FindByIdAsync(id.ToString());

            if (userToDelete == null)
                return NotFound();

            if (currentUser != null && currentUser.Id == id)
                throw new Exception("You cannot delete your own account.");

            var roles = await _userManager.GetRolesAsync(userToDelete);
            if (roles.Contains("Admin"))
                throw new Exception("Remove admin access first before deleting this account.");

            await _adminService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Courses()
    {
        var courses = await _adminService.GetCoursesAsync();
        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        await _adminService.ApproveCourseAsync(id);
        TempData["SuccessMessage"] = "Course approved successfully.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        await _adminService.RejectCourseAsync(id);
        TempData["SuccessMessage"] = "Course rejected successfully.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToPending(int id)
    {
        await _adminService.MoveCourseToPendingAsync(id);
        TempData["SuccessMessage"] = "Course moved back to pending review.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        try
        {
            await _adminService.DeleteCourseAsync(id);
            TempData["SuccessMessage"] = "Course deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    public async Task<IActionResult> Categories()
    {
        return View(new AdminCategoriesViewModel
        {
            Categories = await _adminService.GetCategoriesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Categories(AdminCategoriesViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _adminService.GetCategoriesAsync();
                return View(model);
            }

            await _adminService.AddCategoryAsync(model.NewCategoryTitle);
            TempData["SuccessMessage"] = "Category added successfully.";
            return RedirectToAction(nameof(Categories));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            model.Categories = await _adminService.GetCategoriesAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(AdminCategoryUpdateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault() ?? "Please review the category details and try again.";
                return RedirectToAction(nameof(Categories));
            }

            await _adminService.UpdateCategoryAsync(model.Id, model.Title);
            TempData["SuccessMessage"] = "Category updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await _adminService.DeleteCategoryAsync(id);
            TempData["SuccessMessage"] = "Category deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Currencies()
    {
        return View(new AdminCurrenciesViewModel
        {
            Currencies = await _adminService.GetCurrenciesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Currencies(AdminCurrenciesViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Currencies = await _adminService.GetCurrenciesAsync();
                return View(model);
            }

            await _adminService.AddCurrencyAsync(model.Name, model.Code, model.Symbol);
            TempData["SuccessMessage"] = "Currency added successfully.";
            return RedirectToAction(nameof(Currencies));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            model.Currencies = await _adminService.GetCurrenciesAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCurrency(AdminCurrencyUpdateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault() ?? "Please review the currency details and try again.";
                return RedirectToAction(nameof(Currencies));
            }

            await _adminService.UpdateCurrencyAsync(model.Id, model.Name, model.Code, model.Symbol);
            TempData["SuccessMessage"] = "Currency updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Currencies));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCurrency(int id)
    {
        try
        {
            await _adminService.DeleteCurrencyAsync(id);
            TempData["SuccessMessage"] = "Currency deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Currencies));
    }

    public async Task<IActionResult> Tags()
    {
        return View(new AdminTagsViewModel
        {
            Tags = await _adminService.GetTagsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tags(AdminTagsViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Tags = await _adminService.GetTagsAsync();
                return View(model);
            }

            await _adminService.AddTagAsync(model.NewTagName);
            TempData["SuccessMessage"] = "Tag added successfully.";
            return RedirectToAction(nameof(Tags));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            model.Tags = await _adminService.GetTagsAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTag(AdminTagUpdateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault() ?? "Please review the tag details and try again.";
                return RedirectToAction(nameof(Tags));
            }

            await _adminService.UpdateTagAsync(model.Id, model.Name);
            TempData["SuccessMessage"] = "Tag updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Tags));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            await _adminService.DeleteTagAsync(id);
            TempData["SuccessMessage"] = "Tag deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Tags));
    }

    private int GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var id) ? id : 0;
    }
}
