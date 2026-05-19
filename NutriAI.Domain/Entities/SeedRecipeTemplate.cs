namespace NutriAI.Domain.Entities;

public class SeedRecipeTemplate
{
    public int Id { get; set; }
    public string TemplateKey { get; set; } = "default";
    public string RecipeName { get; set; } = string.Empty;
    public string AnalysisJson { get; set; } = string.Empty;
}
