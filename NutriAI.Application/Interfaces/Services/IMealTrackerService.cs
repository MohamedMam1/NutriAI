using NutriAI.Application.DTOs;

namespace NutriAI.Application.Interfaces.Services;

public interface IMealTrackerService
{
    Task<IReadOnlyList<MealLogDto>> GetMealsAsync(string userId, CancellationToken cancellationToken = default);
    Task<MealAnalyzeResponseDto> AnalyzeMealAsync(string userId, MealAnalyzeRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteMealAsync(string userId, int id, CancellationToken cancellationToken = default);
}
