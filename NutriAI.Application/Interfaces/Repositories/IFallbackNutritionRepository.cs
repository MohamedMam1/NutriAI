using NutriAI.Application.DTOs;
using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IFallbackNutritionRepository
{
    Task<SeedMealTemplate?> GetMealTemplateAsync(string templateKey = "default", CancellationToken cancellationToken = default);
    Task<SeedMealPlanTemplate?> GetMealPlanTemplateAsync(string dietaryPreference, CancellationToken cancellationToken = default);
    Task<SeedRecipeTemplate?> GetRecipeTemplateAsync(string templateKey = "default", CancellationToken cancellationToken = default);
    Task<MealLog?> GetLatestUserMealAsync(string userId, CancellationToken cancellationToken = default);
    Task<MealPlan?> GetLatestUserMealPlanAsync(string userId, string? dietaryPreference, CancellationToken cancellationToken = default);
    Task<RecipeAnalysis?> GetLatestUserRecipeAnalysisAsync(string userId, CancellationToken cancellationToken = default);
}
