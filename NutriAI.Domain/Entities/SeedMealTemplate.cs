namespace NutriAI.Domain.Entities;

public class SeedMealTemplate
{
    public int Id { get; set; }
    public string TemplateKey { get; set; } = "default";
    public string SampleDescription { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public string AiResponse { get; set; } = string.Empty;
}
