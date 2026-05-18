using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;

namespace NutriAI.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IWeightLogRepository _weightLogRepository;
    private readonly IWaterLogRepository _waterLogRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IMealPlanRepository _mealPlanRepository;

    public DashboardService(
        IMealLogRepository mealLogRepository,
        IWeightLogRepository weightLogRepository,
        IWaterLogRepository waterLogRepository,
        IUserGoalRepository userGoalRepository,
        IMealPlanRepository mealPlanRepository)
    {
        _mealLogRepository = mealLogRepository;
        _weightLogRepository = weightLogRepository;
        _waterLogRepository = waterLogRepository;
        _userGoalRepository = userGoalRepository;
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var meals = await _mealLogRepository.GetByUserForDateAsync(userId, today, cancellationToken);
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var latestWeight = await _weightLogRepository.GetLatestByUserAsync(userId, cancellationToken);
        var waterMl = await _waterLogRepository.GetTotalForDateAsync(userId, today, cancellationToken);
        var plans = await _mealPlanRepository.GetByUserAsync(userId, cancellationToken);

        var caloriesConsumed = meals.Sum(m => m.Calories);
        var calorieGoal = goal?.DailyCalorieTarget ?? 2000;
        var waterGoal = goal?.DailyWaterTargetMl ?? 2500;

        var streak = await CalculateStreakAsync(userId, cancellationToken);
        var insight = caloriesConsumed < calorieGoal
            ? $"You're {calorieGoal - caloriesConsumed} calories under your goal. Consider a protein-rich snack."
            : "Great job staying on track with your nutrition today!";

        return new DashboardSummaryDto(
            caloriesConsumed,
            calorieGoal,
            latestWeight?.WeightKg ?? goal?.CurrentWeightKg ?? 0,
            goal?.GoalWeightKg ?? 0,
            waterMl,
            waterGoal,
            streak,
            insight,
            meals.Take(3).Select(m => new RecentMealDto(m.Description, m.Calories, m.LoggedAt.ToLocalTime().ToString("h:mm tt"))).ToList(),
            plans.Take(3).Select(p => new SavedPlanDto(p.Name, p.TimelineWeeks * 7)).ToList());
    }

    private async Task<int> CalculateStreakAsync(string userId, CancellationToken cancellationToken)
    {
        var streak = 0;
        var date = DateTime.UtcNow.Date;
        while (true)
        {
            var meals = await _mealLogRepository.GetByUserForDateAsync(userId, date, cancellationToken);
            if (meals.Count == 0) break;
            streak++;
            date = date.AddDays(-1);
            if (streak > 30) break;
        }
        return streak;
    }
}
