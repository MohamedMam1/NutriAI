namespace NutriAI.Application.Interfaces.Services;

public interface IRecipeService
{
    Task<object> AnalyzeRecipeAsync(string userId, string recipeText, CancellationToken cancellationToken = default);
}
