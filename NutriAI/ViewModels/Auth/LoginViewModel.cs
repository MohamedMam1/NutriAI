using System.ComponentModel.DataAnnotations;

namespace NutriAI.ViewModels.Auth;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public bool ShowEmailConfirmationWarning { get; set; }

    public string? PendingConfirmationEmail { get; set; }
}
