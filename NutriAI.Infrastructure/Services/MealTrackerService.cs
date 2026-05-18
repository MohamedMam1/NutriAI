using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Services;

public class MealTrackerService : IMealTrackerService
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IAIChatRepository _aiChatRepository;

    public MealTrackerService(IMealLogRepository mealLogRepository, IAIChatRepository aiChatRepository)
    {
        _mealLogRepository = mealLogRepository;
        _aiChatRepository = aiChatRepository;
    }

    public async Task<IReadOnlyList<MealLogDto>> GetMealsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var meals = await _mealLogRepository.GetByUserForDateAsync(userId, today, cancellationToken);
        return meals.Select(Map).ToList();
    }

    public async Task<MealAnalyzeResponseDto> AnalyzeMealAsync(string userId, MealAnalyzeRequestDto request, CancellationToken cancellationToken = default)
    {
        var random = new Random(request.Description.GetHashCode());
        var calories = random.Next(200, 650);
        var protein = Math.Round(random.NextDouble() * 40 + 10, 1);
        var carbs = Math.Round(random.NextDouble() * 50 + 15, 1);
        var fat = Math.Round(random.NextDouble() * 25 + 5, 1);
        var aiResponse = $"Based on your description, this meal provides approximately {calories} calories with a balanced macro profile.";

        var log = new MealLog
        {
            UserId = userId,
            Description = request.Description,
            Calories = calories,
            Protein = protein,
            Carbs = carbs,
            Fat = fat,
            AiResponse = aiResponse,
            LoggedAt = DateTime.UtcNow
        };

        await _mealLogRepository.AddAsync(log, cancellationToken);
        await _aiChatRepository.AddAsync(new AIChat
        {
            UserId = userId,
            Role = "User",
            Message = request.Description,
            Context = "MealTracker"
        }, cancellationToken);
        await _aiChatRepository.AddAsync(new AIChat
        {
            UserId = userId,
            Role = "Assistant",
            Message = aiResponse,
            Context = "MealTracker"
        }, cancellationToken);
        await _mealLogRepository.SaveChangesAsync(cancellationToken);

        var dto = Map(log);
        return new MealAnalyzeResponseDto(true, $"Analyzed: {request.Description}", dto, aiResponse);
    }

    public async Task DeleteMealAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var meal = await _mealLogRepository.GetByIdAsync(id, cancellationToken);
        if (meal == null || meal.UserId != userId) return;
        await _mealLogRepository.DeleteAsync(meal, cancellationToken);
        await _mealLogRepository.SaveChangesAsync(cancellationToken);
    }

    private static MealLogDto Map(MealLog m) =>
        new(m.Id, m.Description, m.Calories, m.Protein, m.Carbs, m.Fat,
            m.LoggedAt.ToLocalTime().ToString("h:mm tt"), m.AiResponse);
}
