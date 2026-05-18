using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;

namespace NutriAI.Infrastructure.Services;

public class WaterService : IWaterService
{
    private readonly IWaterLogRepository _waterLogRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IAiNutritionService _aiService;

    public WaterService(
        IWaterLogRepository waterLogRepository,
        IUserGoalRepository userGoalRepository,
        IMealLogRepository mealLogRepository,
        IAiNutritionService aiService)
    {
        _waterLogRepository = waterLogRepository;
        _userGoalRepository = userGoalRepository;
        _mealLogRepository = mealLogRepository;
        _aiService = aiService;
    }

    public async Task<object> GetStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var goalMl = goal?.DailyWaterTargetMl ?? 0;
        var currentMl = await _waterLogRepository.GetTotalForDateAsync(userId, DateTime.UtcNow, cancellationToken);
        var percent = goalMl > 0 ? Math.Min(100, (int)(currentMl * 100.0 / goalMl)) : 0;

        var context = NutritionContextHelper.FromGoal(goal);
        var todayMeals = await _mealLogRepository.GetByUserForDateAsync(userId, DateTime.UtcNow, cancellationToken);
        var todayCalories = todayMeals.Sum(m => m.Calories);

        var recommendation = await _aiService.GetHydrationRecommendationAsync(context, currentMl, todayCalories, cancellationToken)
            ?? GetDefaultHydrationTip(currentMl, goalMl, percent);

        return new { currentMl, goalMl, percent, recommendation };
    }

    public async Task<object> AddWaterAsync(string userId, int amountMl, CancellationToken cancellationToken = default)
    {
        if (amountMl is <= 0 or > 5000)
            throw new ArgumentOutOfRangeException(nameof(amountMl), "Amount must be between 1 and 5000 ml.");

        await _waterLogRepository.AddAsync(new WaterLog
        {
            UserId = userId,
            AmountMl = amountMl,
            LoggedAt = DateTime.UtcNow
        }, cancellationToken);
        await _waterLogRepository.SaveChangesAsync(cancellationToken);
        return await GetStatusAsync(userId, cancellationToken);
    }

    private static string GetDefaultHydrationTip(int currentMl, int goalMl, int percent) =>
        percent >= 100
            ? "Great job meeting your hydration goal today!"
            : $"You have {goalMl - currentMl}ml left to reach your daily water target. Sip regularly through the afternoon.";
}
