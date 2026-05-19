namespace NutriAI.Infrastructure.Validation;

public static class WeightValidation
{
    public const double MinWeightKg = 30;
    public const double MaxWeightKg = 300;
    public const double MaxDailyChangeKg = 3;

    public static string? ValidateNewEntry(double weightKg, double? previousWeightKg)
    {
        if (weightKg < MinWeightKg || weightKg > MaxWeightKg)
            return $"Weight must be between {MinWeightKg} and {MaxWeightKg} kg.";

        if (previousWeightKg.HasValue)
        {
            var change = Math.Abs(weightKg - previousWeightKg.Value);
            if (change > MaxDailyChangeKg)
                return $"Daily weight change cannot exceed {MaxDailyChangeKg} kg. Your last entry was {previousWeightKg:F1} kg.";
        }

        return null;
    }
}
