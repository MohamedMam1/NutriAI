using System.ComponentModel.DataAnnotations;

namespace NutriAI.ViewModels.Profile;

public class ProfileViewModel
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(1, 120)]
    public int Age { get; set; }

    public string Gender { get; set; } = "Male";

    [Range(50, 300)]
    public double Height { get; set; }

    [Range(20, 500)]
    public double CurrentWeight { get; set; }

    [Range(20, 500)]
    public double GoalWeight { get; set; }

    public string ActivityLevel { get; set; } = "Moderately Active";

    [Range(500, 6000), Display(Name = "Daily Water Goal (ml)")]
    public int DailyWaterTargetMl { get; set; }

    public bool HasCompletedProfile { get; set; }
}
