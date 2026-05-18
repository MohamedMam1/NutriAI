using System.ComponentModel.DataAnnotations;

namespace NutriAI.ViewModels.Auth;

public class RegisterViewModel
{
    [Required, Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
