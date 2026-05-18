namespace NutriAI.Domain.Entities;

public class RecipeAnalysis
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TotalCalories { get; set; }
    public int Servings { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public int PerServingCalories { get; set; }
    public double PerServingProtein { get; set; }
    public double PerServingCarbs { get; set; }
    public double PerServingFat { get; set; }
    public string IngredientsJson { get; set; } = "[]";
    public string AlternativesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Recipe Recipe { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
