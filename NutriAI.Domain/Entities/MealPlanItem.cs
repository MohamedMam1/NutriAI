namespace NutriAI.Domain.Entities;

public class MealPlanItem
{
    public int Id { get; set; }
    public int MealPlanId { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string Instructions { get; set; } = string.Empty;

    public MealPlan MealPlan { get; set; } = null!;
}
