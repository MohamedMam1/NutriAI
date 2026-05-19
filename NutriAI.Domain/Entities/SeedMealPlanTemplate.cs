namespace NutriAI.Domain.Entities;

public class SeedMealPlanTemplate
{
    public int Id { get; set; }
    public string DietaryPreference { get; set; } = "Balanced";
    public string PlanJson { get; set; } = string.Empty;
}
