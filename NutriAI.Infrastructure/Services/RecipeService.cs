using System.Text.Json;
using NutriAI.Application.Common;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _context;
    private readonly IAiNutritionService _aiService;
    private readonly IFallbackNutritionRepository _fallbackRepository;

    public RecipeService(
        ApplicationDbContext context,
        IAiNutritionService aiService,
        IFallbackNutritionRepository fallbackRepository)
    {
        _context = context;
        _aiService = aiService;
        _fallbackRepository = fallbackRepository;
    }

    public async Task<object> AnalyzeRecipeAsync(string userId, string recipeText, CancellationToken cancellationToken = default)
    {
        if (!_aiService.IsConfigured)
        {
            return new { success = false, message = AiMessages.ApiKeyNotConfigured, dataSource = AiDataSource.Unavailable };
        }

        var aiResult = await _aiService.AnalyzeRecipeAsync(recipeText, cancellationToken);
        var dataSource = AiDataSource.Ai;
        var userMessage = string.Empty;

        if (aiResult == null)
        {
            var previous = await _fallbackRepository.GetLatestUserRecipeAnalysisAsync(userId, cancellationToken);
            if (previous != null)
            {
                return MapStoredAnalysis(previous, recipeText, userId, AiDataSource.Database, AiMessages.AiUnavailableUseDatabase, saveNew: true, cancellationToken);
            }

            var template = await _fallbackRepository.GetRecipeTemplateAsync(cancellationToken: cancellationToken);
            if (template != null)
            {
                return await MapTemplateAnalysis(template, recipeText, userId, cancellationToken);
            }

            return new { success = false, message = AiMessages.InformationUnavailable, dataSource = AiDataSource.Unavailable };
        }

        return await SaveAiAnalysis(userId, recipeText, aiResult, dataSource, userMessage, cancellationToken);
    }

    private async Task<object> SaveAiAnalysis(
        string userId,
        string recipeText,
        Application.DTOs.RecipeAnalysisResult aiResult,
        string dataSource,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var recipe = new Recipe
        {
            UserId = userId,
            RawText = recipeText,
            Title = aiResult.RecipeName,
            CreatedAt = DateTime.UtcNow
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        var servings = Math.Max(1, aiResult.Servings);
        var ingredients = aiResult.Ingredients.Select(i => new { name = i.Name, amount = i.Amount, calories = i.Calories }).ToArray();

        var analysis = new RecipeAnalysis
        {
            RecipeId = recipe.Id,
            UserId = userId,
            TotalCalories = aiResult.TotalCalories,
            Servings = servings,
            Protein = aiResult.Protein,
            Carbs = aiResult.Carbs,
            Fat = aiResult.Fat,
            PerServingCalories = aiResult.TotalCalories / servings,
            PerServingProtein = aiResult.Protein / servings,
            PerServingCarbs = aiResult.Carbs / servings,
            PerServingFat = aiResult.Fat / servings,
            IngredientsJson = JsonSerializer.Serialize(ingredients),
            AlternativesJson = JsonSerializer.Serialize(aiResult.Alternatives)
        };
        _context.RecipeAnalyses.Add(analysis);
        await _context.SaveChangesAsync(cancellationToken);

        return BuildPayload(recipe.Title, analysis, ingredients, aiResult.Alternatives.ToArray(), dataSource,
            string.IsNullOrEmpty(userMessage) ? "Recipe analyzed successfully." : userMessage);
    }

    private async Task<object> MapTemplateAnalysis(SeedRecipeTemplate template, string recipeText, string userId, CancellationToken cancellationToken)
    {
        using var doc = JsonDocument.Parse(template.AnalysisJson);
        var root = doc.RootElement;

        var recipe = new Recipe { UserId = userId, RawText = recipeText, Title = template.RecipeName, CreatedAt = DateTime.UtcNow };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        var totalCalories = root.GetProperty("totalCalories").GetInt32();
        var servings = Math.Max(1, root.GetProperty("servings").GetInt32());
        var protein = root.GetProperty("protein").GetDouble();
        var carbs = root.GetProperty("carbs").GetDouble();
        var fat = root.GetProperty("fat").GetDouble();
        var ingredientsJson = root.GetProperty("ingredients").GetRawText();
        var alternativesJson = root.GetProperty("alternatives").GetRawText();

        var analysis = new RecipeAnalysis
        {
            RecipeId = recipe.Id,
            UserId = userId,
            TotalCalories = totalCalories,
            Servings = servings,
            Protein = protein,
            Carbs = carbs,
            Fat = fat,
            PerServingCalories = totalCalories / servings,
            PerServingProtein = protein / servings,
            PerServingCarbs = carbs / servings,
            PerServingFat = fat / servings,
            IngredientsJson = ingredientsJson,
            AlternativesJson = alternativesJson
        };
        _context.RecipeAnalyses.Add(analysis);
        await _context.SaveChangesAsync(cancellationToken);

        return BuildPayload(
            recipe.Title,
            analysis,
            JsonSerializer.Deserialize<object>(ingredientsJson),
            JsonSerializer.Deserialize<string[]>(alternativesJson) ?? [],
            AiDataSource.Database,
            AiMessages.AiUnavailableUseDatabase);
    }

    private async Task<object> MapStoredAnalysis(
        RecipeAnalysis previous,
        string recipeText,
        string userId,
        string dataSource,
        string message,
        bool saveNew,
        CancellationToken cancellationToken)
    {
        if (!saveNew)
        {
            return BuildPayload(
                "Saved recipe analysis",
                previous,
                JsonSerializer.Deserialize<object>(previous.IngredientsJson),
                JsonSerializer.Deserialize<string[]>(previous.AlternativesJson) ?? [],
                dataSource,
                message);
        }

        var recipe = new Recipe { UserId = userId, RawText = recipeText, Title = "Saved recipe analysis", CreatedAt = DateTime.UtcNow };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        var copy = new RecipeAnalysis
        {
            RecipeId = recipe.Id,
            UserId = userId,
            TotalCalories = previous.TotalCalories,
            Servings = previous.Servings,
            Protein = previous.Protein,
            Carbs = previous.Carbs,
            Fat = previous.Fat,
            PerServingCalories = previous.PerServingCalories,
            PerServingProtein = previous.PerServingProtein,
            PerServingCarbs = previous.PerServingCarbs,
            PerServingFat = previous.PerServingFat,
            IngredientsJson = previous.IngredientsJson,
            AlternativesJson = previous.AlternativesJson
        };
        _context.RecipeAnalyses.Add(copy);
        await _context.SaveChangesAsync(cancellationToken);

        return BuildPayload(
            recipe.Title,
            copy,
            JsonSerializer.Deserialize<object>(copy.IngredientsJson),
            JsonSerializer.Deserialize<string[]>(copy.AlternativesJson) ?? [],
            dataSource,
            message);
    }

    private static object BuildPayload(
        string recipeName,
        RecipeAnalysis analysis,
        object? ingredients,
        string[] alternatives,
        string dataSource,
        string message) =>
        new
        {
            success = true,
            message,
            dataSource,
            recipeName,
            totalCalories = analysis.TotalCalories,
            servings = analysis.Servings,
            perServing = new
            {
                calories = analysis.PerServingCalories,
                protein = analysis.PerServingProtein,
                carbs = analysis.PerServingCarbs,
                fat = analysis.PerServingFat
            },
            macros = new { protein = analysis.Protein, carbs = analysis.Carbs, fat = analysis.Fat },
            ingredients,
            alternatives
        };
}
