using System.ComponentModel.DataAnnotations;

namespace NutriAI.ViewModels.Auth;

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
