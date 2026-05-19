using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Identity;

public static class NutritionFallbackSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.SeedMealTemplates.AnyAsync())
        {
            context.SeedMealTemplates.Add(new SeedMealTemplate
            {
                TemplateKey = "default",
                SampleDescription = "Balanced lunch with lean protein and vegetables",
                Calories = 520,
                Protein = 38,
                Carbs = 42,
                Fat = 18,
                AiResponse =
                    "This balanced meal provides steady energy and supports your daily calorie target. " +
                    "Pair it with water and a light evening meal to stay on track toward your goal weight."
            });
        }

        if (!await context.SeedMealPlanTemplates.AnyAsync())
        {
            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var plan = days.Select(day => new
            {
                day,
                meals = new[]
                {
                    new { mealType = "Breakfast", name = "Oat bowl with fruit", calories = 380, protein = 18.0, carbs = 52.0, fat = 10.0, instructions = "Cook oats with water; top with berries and a spoon of yogurt." },
                    new { mealType = "Lunch", name = "Grilled chicken salad", calories = 480, protein = 42.0, carbs = 28.0, fat = 16.0, instructions = "Grill chicken; serve over mixed greens with light dressing." },
                    new { mealType = "Dinner", name = "Baked fish with vegetables", calories = 450, protein = 36.0, carbs = 30.0, fat = 14.0, instructions = "Bake fish with herbs; steam vegetables on the side." },
                    new { mealType = "Snacks", name = "Greek yogurt and almonds", calories = 200, protein = 14.0, carbs = 12.0, fat = 11.0, instructions = "Portion yogurt and a small handful of almonds." }
                }
            });

            context.SeedMealPlanTemplates.Add(new SeedMealPlanTemplate
            {
                DietaryPreference = "Balanced",
                PlanJson = JsonSerializer.Serialize(plan)
            });
        }

        if (!await context.SeedRecipeTemplates.AnyAsync())
        {
            var analysis = new
            {
                recipeName = "Reference chicken and rice bowl",
                totalCalories = 1840,
                servings = 4,
                protein = 112.0,
                carbs = 168.0,
                fat = 72.0,
                ingredients = new[]
                {
                    new { name = "Chicken breast", amount = "500g", calories = 550 },
                    new { name = "Olive oil", amount = "2 tbsp", calories = 240 },
                    new { name = "Mixed vegetables", amount = "400g", calories = 180 },
                    new { name = "Brown rice", amount = "2 cups cooked", calories = 420 }
                },
                alternatives = new[]
                {
                    "Use cooking spray instead of olive oil to reduce fat.",
                    "Try cauliflower rice instead of brown rice.",
                    "Choose skinless grilled chicken for leaner protein."
                }
            };

            context.SeedRecipeTemplates.Add(new SeedRecipeTemplate
            {
                TemplateKey = "default",
                RecipeName = analysis.recipeName,
                AnalysisJson = JsonSerializer.Serialize(analysis)
            });
        }

        await context.SaveChangesAsync();
    }
}
