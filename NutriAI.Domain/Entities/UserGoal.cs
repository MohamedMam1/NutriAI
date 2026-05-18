namespace NutriAI.Domain.Entities;

public class UserGoal
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public double HeightCm { get; set; }
    public double CurrentWeightKg { get; set; }
    public double GoalWeightKg { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public int DailyCalorieTarget { get; set; } = 2000;
    public int DailyWaterTargetMl { get; set; } = 2500;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
