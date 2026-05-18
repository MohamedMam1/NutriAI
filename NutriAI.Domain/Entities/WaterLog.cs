namespace NutriAI.Domain.Entities;

public class WaterLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AmountMl { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
