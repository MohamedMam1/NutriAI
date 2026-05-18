using NutriAI.Application.Common;
using NutriAI.Application.DTOs;

namespace NutriAI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto dto, string confirmationCallbackUrlTemplate, CancellationToken cancellationToken = default);
    Task<ServiceResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordDto dto, string resetCallbackUrlTemplate, CancellationToken cancellationToken = default);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);
    Task<ServiceResult> ResendConfirmationAsync(string email, string confirmationCallbackUrlTemplate, CancellationToken cancellationToken = default);
    Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
}
