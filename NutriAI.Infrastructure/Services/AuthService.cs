using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using NutriAI.Application.Common;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;

namespace NutriAI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IWeightLogRepository _weightLogRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserGoalRepository userGoalRepository,
        IWeightLogRepository weightLogRepository,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userGoalRepository = userGoalRepository;
        _weightLogRepository = weightLogRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterDto dto, string confirmationCallbackUrlTemplate, CancellationToken cancellationToken = default)
    {
        if (dto.Password != dto.ConfirmPassword)
            return ServiceResult.Failure("Passwords do not match.");

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return ServiceResult.Failure("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ServiceResult.Failure(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.User);

        var profile = new ProfileDto(
            dto.Email,
            dto.FullName,
            dto.Age,
            dto.Gender,
            dto.HeightCm,
            dto.CurrentWeightKg,
            dto.GoalWeightKg,
            dto.ActivityLevel,
            dto.DailyWaterTargetMl);

        var (dailyCalories, calculatedWater) = NutritionTargetsCalculator.Calculate(profile);
        var waterTarget = dto.DailyWaterTargetMl > 0 ? dto.DailyWaterTargetMl : calculatedWater;

        var goal = new UserGoal
        {
            UserId = user.Id,
            Age = dto.Age,
            Gender = dto.Gender,
            HeightCm = dto.HeightCm,
            CurrentWeightKg = dto.CurrentWeightKg,
            GoalWeightKg = dto.GoalWeightKg,
            ActivityLevel = dto.ActivityLevel,
            DailyCalorieTarget = dailyCalories,
            DailyWaterTargetMl = waterTarget,
            UpdatedAt = DateTime.UtcNow
        };
        await _userGoalRepository.AddAsync(goal, cancellationToken);

        await _weightLogRepository.AddAsync(new WeightLog
        {
            UserId = user.Id,
            WeightKg = dto.CurrentWeightKg,
            LoggedAt = DateTime.UtcNow
        }, cancellationToken);

        await _userGoalRepository.SaveChangesAsync(cancellationToken);

        await SendConfirmationEmailAsync(user, confirmationCallbackUrlTemplate, cancellationToken);

        return ServiceResult.Success("Registration successful. Please check your email to confirm your account.");
    }

    public async Task<ServiceResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult.Failure("Invalid login attempt.");

        if (user.IsBanned)
            return ServiceResult.Failure("AccountBanned", Application.Common.AiMessages.AccountBanned);

        if (!user.EmailConfirmed)
            return ServiceResult.Failure(
                "EmailNotConfirmed",
                "Please confirm your email before logging in. Check your inbox for the confirmation link.");

        var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
            return ServiceResult.Success();

        if (result.IsLockedOut)
            return ServiceResult.Failure("Account locked. Try again later.");

        return ServiceResult.Failure("Invalid login attempt.");
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default) =>
        await _signInManager.SignOutAsync();

    public async Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordDto dto, string resetCallbackUrlTemplate, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return ServiceResult.Success("If an account exists, a reset link has been sent.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = string.Format(resetCallbackUrlTemplate, Uri.EscapeDataString(user.Email!), encodedToken);

        await _emailService.SendEmailAsync(
            user.Email!,
            "Reset your NutriAI password",
            $"<p>Hello {user.FullName},</p><p><a href=\"{link}\">Reset your password</a></p>",
            cancellationToken);

        return ServiceResult.Success("If an account exists, a reset link has been sent.");
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Password != dto.ConfirmPassword)
            return ServiceResult.Failure("Passwords do not match.");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult.Failure("Invalid reset request.");

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await _userManager.ResetPasswordAsync(user, token, dto.Password);
        return result.Succeeded
            ? ServiceResult.Success("Password has been reset.")
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResult.Failure("Invalid confirmation link.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        return result.Succeeded
            ? ServiceResult.Success("Email confirmed. You can now log in.")
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> ResendConfirmationAsync(string email, string confirmationCallbackUrlTemplate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ServiceResult.Failure("Email address is required.");

        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user == null)
            return ServiceResult.Success("If an account exists, a confirmation email has been sent.");

        if (user.EmailConfirmed)
            return ServiceResult.Failure("Email is already confirmed.");

        await SendConfirmationEmailAsync(user, confirmationCallbackUrlTemplate, cancellationToken);
        return ServiceResult.Success("Confirmation email sent. Please check your inbox.");
    }

    public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return ServiceResult.Failure("New password and confirmation do not match.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResult.Failure("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded
            ? ServiceResult.Success("Password changed successfully.")
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, string callbackTemplate, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = string.Format(callbackTemplate, user.Id, encodedToken);

        _logger.LogInformation("Sending confirmation email to {Email}", user.Email);

        await _emailService.SendEmailAsync(
            user.Email!,
            "Confirm your NutriAI account",
            $"<p>Hello {user.FullName},</p><p>Please <a href=\"{link}\">confirm your email</a> to activate your account.</p><p>If the link does not work, copy and paste this URL into your browser:</p><p>{link}</p>",
            cancellationToken);
    }
}
