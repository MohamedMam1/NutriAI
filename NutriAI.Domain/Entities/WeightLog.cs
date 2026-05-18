namespace NutriAI.Domain.Entities;

public class WeightLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
