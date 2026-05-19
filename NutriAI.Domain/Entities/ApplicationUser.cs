using Microsoft.AspNetCore.Identity;

namespace NutriAI.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBanned { get; set; }

    public UserGoal? UserGoal { get; set; }
    public ICollection<Meal> Meals { get; set; } = [];
    public ICollection<MealLog> MealLogs { get; set; } = [];
    public ICollection<WeightLog> WeightLogs { get; set; } = [];
    public ICollection<WaterLog> WaterLogs { get; set; } = [];
    public ICollection<MealPlan> MealPlans { get; set; } = [];
    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<RecipeAnalysis> RecipeAnalyses { get; set; } = [];
    public ICollection<WeeklyReport> WeeklyReports { get; set; } = [];
    public ICollection<AIChat> AIChats { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
