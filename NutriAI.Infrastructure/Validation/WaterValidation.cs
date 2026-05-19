using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Validation;

public static class WaterValidation
{
    public static string? ValidateAdd(int amountMl, int currentMl, UserGoal? goal)
    {
        if (amountMl < 50)
            return "Minimum water entry is 50 ml.";

        var dailyGoal = goal?.DailyWaterTargetMl ?? 2500;
        var maxSingle = Math.Max(250, (int)(dailyGoal * 0.5));
        if (amountMl > maxSingle)
            return $"A single entry cannot exceed {maxSingle} ml based on your daily goal of {dailyGoal} ml.";

        var maxDaily = (int)(dailyGoal * 1.5);
        if (currentMl + amountMl > maxDaily)
            return $"Daily water intake cannot exceed {maxDaily} ml (150% of your goal).";

        return null;
    }
}
