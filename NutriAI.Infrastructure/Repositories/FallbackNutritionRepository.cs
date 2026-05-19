using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class FallbackNutritionRepository : IFallbackNutritionRepository
{
    private readonly ApplicationDbContext _context;

    public FallbackNutritionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<SeedMealTemplate?> GetMealTemplateAsync(string templateKey = "default", CancellationToken cancellationToken = default) =>
        _context.SeedMealTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey, cancellationToken);

    public async Task<SeedMealPlanTemplate?> GetMealPlanTemplateAsync(string dietaryPreference, CancellationToken cancellationToken = default)
    {
        var pref = dietaryPreference.Trim();
        var exact = await _context.SeedMealPlanTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.DietaryPreference == pref, cancellationToken);
        if (exact != null) return exact;

        return await _context.SeedMealPlanTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.DietaryPreference == "Balanced", cancellationToken);
    }

    public Task<SeedRecipeTemplate?> GetRecipeTemplateAsync(string templateKey = "default", CancellationToken cancellationToken = default) =>
        _context.SeedRecipeTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey, cancellationToken);

    public Task<MealLog?> GetLatestUserMealAsync(string userId, CancellationToken cancellationToken = default) =>
        _context.MealLogs.AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<MealPlan?> GetLatestUserMealPlanAsync(string userId, string? dietaryPreference, CancellationToken cancellationToken = default)
    {
        var query = _context.MealPlans.AsNoTracking()
            .Include(p => p.Items)
            .Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(dietaryPreference))
            query = query.Where(p => p.DietaryPreference == dietaryPreference);

        return query.OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<RecipeAnalysis?> GetLatestUserRecipeAnalysisAsync(string userId, CancellationToken cancellationToken = default) =>
        _context.RecipeAnalyses.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);
}
