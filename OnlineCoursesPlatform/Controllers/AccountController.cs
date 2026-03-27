using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCoursesPlatform.ViewModels;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Services.Interfaces;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var redirect = await _accountService.GetAuthenticatedRedirectAsync(User);
        return redirect != null
            ? RedirectToAction(redirect.Action, redirect.Controller)
            : View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.RegisterAsync(model);
        if (!result.Succeeded)
        {
            ApplyErrors(result);
            return View(model);
        }

        return model.IsInstructor
            ? RedirectToAction("Index", "Instructor")
            : RedirectToAction("GetMyEnrolledCourses", "Enrollment");
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        var redirect = await _accountService.GetAuthenticatedRedirectAsync(User);
        return redirect != null
            ? RedirectToAction(redirect.Action, redirect.Controller)
            : View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.LoginAsync(model);
        if (!result.Succeeded)
        {
            ApplyErrors(result);
            return View(model);
        }

        return RedirectToAction(result.RedirectAction, result.RedirectController);
    }

    [HttpGet]
    public async Task<IActionResult> ForgotPassword()
    {
        var redirect = await _accountService.GetAuthenticatedRedirectAsync(User);
        return redirect != null
            ? RedirectToAction(redirect.Action, redirect.Controller)
            : View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.ForgotPasswordAsync(model);
        if (!result.Succeeded)
        {
            ApplyErrors(result);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(result.Email) && !string.IsNullOrWhiteSpace(result.EncodedToken))
        {
            model.GeneratedResetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = result.Email, token = result.EncodedToken },
                Request.Scheme);
        }

        ViewBag.SuccessMessage = "If the account exists, a password reset link has been sent";
        ModelState.Clear();
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "The reset link is invalid or incomplete.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.ResetPasswordAsync(model);
        if (!result.Succeeded)
        {
            ApplyErrors(result);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var profile = await _accountService.GetProfileAsync(User);
        return profile == null ? NotFound() : View(profile);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var model = await _accountService.GetEditProfileAsync(User);
        return model == null ? NotFound() : View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.UpdateProfileAsync(User, model);
        if (!result.Succeeded)
        {
            ApplyErrors(result);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.ChangePasswordAsync(User, model);
        if (!result.Succeeded)
        {
            ApplyErrors(result);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }

    private void ApplyErrors(ServiceResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }
}
