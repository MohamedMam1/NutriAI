using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _context;

    public RecipeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> AnalyzeRecipeAsync(string userId, string recipeText, CancellationToken cancellationToken = default)
    {
        var recipe = new Recipe { UserId = userId, RawText = recipeText, Title = "Analyzed Recipe", CreatedAt = DateTime.UtcNow };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        var analysis = new RecipeAnalysis
        {
            RecipeId = recipe.Id,
            UserId = userId,
            TotalCalories = 1840,
            Servings = 4,
            Protein = 112,
            Carbs = 168,
            Fat = 72,
            PerServingCalories = 460,
            PerServingProtein = 28,
            PerServingCarbs = 42,
            PerServingFat = 18,
            IngredientsJson = JsonSerializer.Serialize(new[]
            {
                new { name = "Chicken breast", amount = "500g", calories = 550 },
                new { name = "Olive oil", amount = "2 tbsp", calories = 240 },
                new { name = "Mixed vegetables", amount = "400g", calories = 180 },
                new { name = "Brown rice", amount = "2 cups", calories = 420 }
            }),
            AlternativesJson = JsonSerializer.Serialize(new[]
            {
                "Swap olive oil for cooking spray to reduce fat.",
                "Use cauliflower rice instead of brown rice.",
                "Try skinless grilled chicken for leaner protein."
            })
        };
        _context.RecipeAnalyses.Add(analysis);
        await _context.SaveChangesAsync(cancellationToken);

        return new
        {
            success = true,
            recipeName = recipe.Title,
            totalCalories = analysis.TotalCalories,
            servings = analysis.Servings,
            perServing = new { calories = analysis.PerServingCalories, protein = analysis.PerServingProtein, carbs = analysis.PerServingCarbs, fat = analysis.PerServingFat },
            macros = new { protein = analysis.Protein, carbs = analysis.Carbs, fat = analysis.Fat },
            ingredients = JsonSerializer.Deserialize<object>(analysis.IngredientsJson),
            alternatives = JsonSerializer.Deserialize<object>(analysis.AlternativesJson)
        };
    }
}
