using NutriAI.Application.DTOs;

namespace NutriAI.Application.Interfaces.Services;

public interface IAiNutritionService
{
    bool IsConfigured { get; }

    Task<MealAnalysisResult?> AnalyzeMealAsync(
        string description,
        UserNutritionContext context,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MealPlanDayResult>?> GenerateMealPlanAsync(
        double goalWeightKg,
        int timelineWeeks,
        string dietaryPreference,
        UserNutritionContext context,
        CancellationToken cancellationToken = default);

    Task<RecipeAnalysisResult?> AnalyzeRecipeAsync(
        string recipeText,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>?> GetWeeklyRecommendationsAsync(
        string reportSummary,
        CancellationToken cancellationToken = default);

    Task<string?> GetHydrationRecommendationAsync(
        UserNutritionContext context,
        int currentMl,
        int todayCalories,
        CancellationToken cancellationToken = default);

    Task<string?> GetDashboardInsightAsync(
        UserNutritionContext context,
        int caloriesConsumed,
        int waterMl,
        CancellationToken cancellationToken = default);

    Task<string?> GetWeightInsightAsync(
        UserNutritionContext context,
        double latestWeight,
        CancellationToken cancellationToken = default);
}
