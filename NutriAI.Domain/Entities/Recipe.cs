namespace NutriAI.Domain.Entities;

public class Recipe
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string RawText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<RecipeAnalysis> Analyses { get; set; } = [];
}
