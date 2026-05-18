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

    [Required, Range(13, 120), Display(Name = "Age")]
    public int Age { get; set; }

    [Required, Display(Name = "Gender")]
    public string Gender { get; set; } = "Male";

    [Required, Range(50, 300), Display(Name = "Height (cm)")]
    public double HeightCm { get; set; }

    [Required, Range(20, 500), Display(Name = "Current Weight (kg)")]
    public double CurrentWeightKg { get; set; }

    [Required, Range(20, 500), Display(Name = "Goal Weight (kg)")]
    public double GoalWeightKg { get; set; }

    [Required, Display(Name = "Activity Level")]
    public string ActivityLevel { get; set; } = "Moderately Active";

    [Required, Range(500, 6000), Display(Name = "Daily Water Goal (ml)")]
    public int DailyWaterTargetMl { get; set; } = 2500;
}
