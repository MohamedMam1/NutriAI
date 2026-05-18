namespace NutriAI.Application.DTOs;

public record DashboardSummaryDto(
    int CaloriesConsumed,
    int CaloriesGoal,
    double CurrentWeight,
    double GoalWeight,
    int WaterMl,
    int WaterGoalMl,
    int WeeklyStreak,
    string AiInsight,
    IReadOnlyList<RecentMealDto> RecentMeals,
    IReadOnlyList<SavedPlanDto> SavedPlans);

public record RecentMealDto(string Name, int Calories, string Time);
public record SavedPlanDto(string Name, int Days);
