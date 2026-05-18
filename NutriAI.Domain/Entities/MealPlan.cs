namespace NutriAI.Domain.Entities;

public class MealPlan
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double GoalWeightKg { get; set; }
    public int TimelineWeeks { get; set; }
    public string DietaryPreference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<MealPlanItem> Items { get; set; } = [];
}
