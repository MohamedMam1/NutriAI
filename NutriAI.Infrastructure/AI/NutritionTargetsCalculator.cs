using NutriAI.Application.DTOs;

namespace NutriAI.Infrastructure.AI;

internal static class NutritionTargetsCalculator
{
    public static (int DailyCalories, int DailyWaterMl) Calculate(ProfileDto profile)
    {
        var bmr = profile.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase)
            ? 10 * profile.CurrentWeight + 6.25 * profile.Height - 5 * profile.Age - 161
            : 10 * profile.CurrentWeight + 6.25 * profile.Height - 5 * profile.Age + 5;

        var activityFactor = profile.ActivityLevel switch
        {
            var l when l.Contains("Sedentary", StringComparison.OrdinalIgnoreCase) => 1.2,
            var l when l.Contains("Light", StringComparison.OrdinalIgnoreCase) => 1.375,
            var l when l.Contains("Very", StringComparison.OrdinalIgnoreCase) => 1.725,
            var l when l.Contains("Extra", StringComparison.OrdinalIgnoreCase) => 1.9,
            _ => 1.55
        };

        var maintenance = (int)Math.Round(bmr * activityFactor);
        var delta = profile.GoalWeight < profile.CurrentWeight ? -500 : profile.GoalWeight > profile.CurrentWeight ? 300 : 0;
        var calories = Math.Clamp(maintenance + delta, 1200, 4500);
        var waterMl = Math.Clamp((int)Math.Round(profile.CurrentWeight * 35), 1500, 4000);

        return (calories, waterMl);
    }
}
