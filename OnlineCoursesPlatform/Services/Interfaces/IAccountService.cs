using System.Security.Claims;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services.Interfaces;

public interface IAccountService
{
    Task<RedirectDestinationDto?> GetAuthenticatedRedirectAsync(ClaimsPrincipal principal);
    Task<ServiceResult> RegisterAsync(RegisterViewModel model);
    Task<LoginResultDto> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task<ProfileDto?> GetProfileAsync(ClaimsPrincipal principal);
    Task<EditProfileViewModel?> GetEditProfileAsync(ClaimsPrincipal principal);
    Task<ServiceResult> UpdateProfileAsync(ClaimsPrincipal principal, EditProfileViewModel model);
    Task<ServiceResult> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordViewModel model);
    Task<ForgotPasswordResultDto> ForgotPasswordAsync(ForgotPasswordViewModel model);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel model);
}
