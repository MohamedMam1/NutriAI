using NutriAI.Application.DTOs;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.AI;

internal static class NutritionContextHelper
{
    public static UserNutritionContext FromGoal(UserGoal? goal) =>
        goal == null
            ? new UserNutritionContext(2000, 0, 0, 2500, "Moderately Active", 30, "Unspecified")
            : new UserNutritionContext(
                goal.DailyCalorieTarget,
                goal.CurrentWeightKg,
                goal.GoalWeightKg,
                goal.DailyWaterTargetMl,
                goal.ActivityLevel,
                goal.Age,
                goal.Gender);
}
