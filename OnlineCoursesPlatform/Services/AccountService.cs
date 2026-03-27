using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using OnlineCoursesPlatform.ViewModels;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Services.Interfaces;

namespace OnlineCoursesPlatform.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AccountService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<RedirectDestinationDto?> GetAuthenticatedRedirectAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            return new RedirectDestinationDto { Action = "Index", Controller = "Admin" };
        }

        if (roles.Contains("Instructor"))
        {
            return new RedirectDestinationDto { Action = "Index", Controller = "Instructor" };
        }

        if (roles.Contains("Student"))
        {
            return new RedirectDestinationDto { Action = "GetMyEnrolledCourses", Controller = "Enrollment" };
        }

        return new RedirectDestinationDto { Action = "Index", Controller = "Home" };
    }

    public async Task<ServiceResult> RegisterAsync(RegisterViewModel model)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return ServiceResult.Failure("This email is already registered.");
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult.Failure(createResult.Errors.Select(error => error.Description));
        }

        var rolesToAssign = new List<string> { "Student" };
        if (model.IsInstructor)
        {
            rolesToAssign.Add("Instructor");
        }

        var roleResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return ServiceResult.Failure(roleResult.Errors.Select(error => error.Description));
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return ServiceResult.Success();
    }

    public async Task<LoginResultDto> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                Errors = ["The email or password wrong."]
            };
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                Errors = ["The email or password wrong."]
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            return new LoginResultDto
            {
                Succeeded = true,
                RedirectAction = "Index",
                RedirectController = "Admin"
            };
        }

        if (roles.Contains("Instructor"))
        {
            return new LoginResultDto
            {
                Succeeded = true,
                RedirectAction = "Index",
                RedirectController = "Instructor"
            };
        }

        if (roles.Contains("Student"))
        {
            return new LoginResultDto
            {
                Succeeded = true,
                RedirectAction = "GetMyEnrolledCourses",
                RedirectController = "Enrollment"
            };
        }

        return new LoginResultDto
        {
            Succeeded = true,
            RedirectAction = "Index",
            RedirectController = "Home"
        };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<ProfileDto?> GetProfileAsync(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new ProfileDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email ?? string.Empty,
            ProfilePicture = user.ProfilePicture,
            Roles = roles
        };
    }

    public async Task<EditProfileViewModel?> GetEditProfileAsync(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return null;
        }

        return new EditProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExistingImageUrl = user.ProfilePicture
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(ClaimsPrincipal principal, EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            user.ProfilePicture = await SaveProfileImageAsync(user.ProfilePicture, model.ProfileImage);
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ServiceResult.Failure(updateResult.Errors.Select(error => error.Description));
        }

        await _signInManager.RefreshSignInAsync(user);
        return ServiceResult.Success("Profile updated successfully!");
    }

    public async Task<ServiceResult> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordViewModel model)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return ServiceResult.Failure(result.Errors.Select(error => error.Description));
        }

        await _signInManager.RefreshSignInAsync(user);
        return ServiceResult.Success("Password changed successfully.");
    }

    public async Task<ForgotPasswordResultDto> ForgotPasswordAsync(ForgotPasswordViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new ForgotPasswordResultDto
            {
                Succeeded = true,
                Message = "If the account exists, a password reset link has been prepared."
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return new ForgotPasswordResultDto
        {
            Succeeded = true,
            Message = "Password reset link generated successfully.",
            Email = user.Email,
            EncodedToken = encodedToken
        };
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return ServiceResult.Failure("We could not find an account for this email.");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch
        {
            return ServiceResult.Failure("The reset token is invalid or expired.");
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
        if (!result.Succeeded)
        {
            return ServiceResult.Failure(result.Errors.Select(error => error.Description));
        }

        return ServiceResult.Success("Password reset successfully. You can log in now.");
    }

    private async Task<string> SaveProfileImageAsync(string? currentProfilePicture, IFormFile profileImage)
    {
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        if (!string.IsNullOrWhiteSpace(currentProfilePicture) &&
            !string.Equals(currentProfilePicture, "default-avatar.png", StringComparison.OrdinalIgnoreCase))
        {
            var currentFileName = Path.GetFileName(currentProfilePicture);
            var oldFilePath = Path.Combine(uploadsFolder, currentFileName);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await profileImage.CopyToAsync(fileStream);

        return uniqueFileName;
    }
}
