namespace NutriAI.Domain.Entities;

public class MealLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? MealId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string? AiResponse { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Meal? Meal { get; set; }
}
