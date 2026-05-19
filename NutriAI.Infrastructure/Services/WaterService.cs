using NutriAI.Application.Common;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;
using NutriAI.Infrastructure.Validation;

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
        var goalMl = goal?.DailyWaterTargetMl ?? 2500;
        var currentMl = await _waterLogRepository.GetTotalForDateAsync(userId, DateTime.UtcNow, cancellationToken);
        var percent = goalMl > 0 ? Math.Min(100, (int)(currentMl * 100.0 / goalMl)) : 0;

        var context = NutritionContextHelper.FromGoal(goal);
        var todayMeals = await _mealLogRepository.GetByUserForDateAsync(userId, DateTime.UtcNow, cancellationToken);
        var todayCalories = todayMeals.Sum(m => m.Calories);

        string recommendation;
        if (_aiService.IsConfigured)
        {
            recommendation = await _aiService.GetHydrationRecommendationAsync(context, currentMl, todayCalories, cancellationToken)
                ?? GetDefaultHydrationTip(currentMl, goalMl, percent);
        }
        else
        {
            recommendation = GetDefaultHydrationTip(currentMl, goalMl, percent);
        }

        return new { success = true, currentMl, goalMl, percent, recommendation };
    }

    public async Task<object> AddWaterAsync(string userId, int amountMl, CancellationToken cancellationToken = default)
    {
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var currentMl = await _waterLogRepository.GetTotalForDateAsync(userId, DateTime.UtcNow, cancellationToken);
        var validationError = WaterValidation.ValidateAdd(amountMl, currentMl, goal);
        if (validationError != null)
            return new { success = false, message = validationError };

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
            : $"You have {Math.Max(0, goalMl - currentMl)} ml left to reach your daily water target. Sip regularly through the day.";
}
