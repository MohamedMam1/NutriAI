namespace NutriAI.Domain.Entities;

public class WeeklyReport
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public double WeightChangeKg { get; set; }
    public int AverageCalories { get; set; }
    public int HydrationScore { get; set; }
    public string BestDay { get; set; } = string.Empty;
    public string WorstDay { get; set; } = string.Empty;
    public string RecommendationsJson { get; set; } = "[]";
    public string DailyCaloriesJson { get; set; } = "[]";
    public string WeightTrendJson { get; set; } = "[]";
    public string HydrationDaysJson { get; set; } = "[]";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
