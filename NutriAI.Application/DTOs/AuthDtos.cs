namespace NutriAI.Application.DTOs;

public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    int Age,
    string Gender,
    double HeightCm,
    double CurrentWeightKg,
    double GoalWeightKg,
    string ActivityLevel,
    int DailyWaterTargetMl);
public record LoginDto(string Email, string Password, bool RememberMe);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Email, string Token, string Password, string ConfirmPassword);
public record ChangePasswordDto(string CurrentPassword, string NewPassword, string ConfirmPassword);
