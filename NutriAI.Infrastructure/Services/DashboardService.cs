using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Infrastructure.AI;

namespace NutriAI.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IWeightLogRepository _weightLogRepository;
    private readonly IWaterLogRepository _waterLogRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IWeeklyReportRepository _weeklyReportRepository;
    private readonly IAiNutritionService _aiService;

    public DashboardService(
        IMealLogRepository mealLogRepository,
        IWeightLogRepository weightLogRepository,
        IWaterLogRepository waterLogRepository,
        IUserGoalRepository userGoalRepository,
        IMealPlanRepository mealPlanRepository,
        IWeeklyReportRepository weeklyReportRepository,
        IAiNutritionService aiService)
    {
        _mealLogRepository = mealLogRepository;
        _weightLogRepository = weightLogRepository;
        _waterLogRepository = waterLogRepository;
        _userGoalRepository = userGoalRepository;
        _mealPlanRepository = mealPlanRepository;
        _weeklyReportRepository = weeklyReportRepository;
        _aiService = aiService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var meals = await _mealLogRepository.GetByUserForDateAsync(userId, today, cancellationToken);
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var latestWeight = await _weightLogRepository.GetLatestByUserAsync(userId, cancellationToken);
        var weightHistory = await _weightLogRepository.GetByUserAsync(userId, cancellationToken);
        var waterMl = await _waterLogRepository.GetTotalForDateAsync(userId, today, cancellationToken);
        var plans = await _mealPlanRepository.GetByUserAsync(userId, cancellationToken);
        var latestReport = await _weeklyReportRepository.GetLatestByUserAsync(userId, cancellationToken);

        var caloriesConsumed = meals.Sum(m => m.Calories);
        var calorieGoal = goal?.DailyCalorieTarget ?? 0;
        var waterGoal = goal?.DailyWaterTargetMl ?? 0;
        var context = NutritionContextHelper.FromGoal(goal);

        var streak = await CalculateStreakAsync(userId, cancellationToken);
        var insight = await _aiService.GetDashboardInsightAsync(context, caloriesConsumed, waterMl, cancellationToken)
            ?? (goal == null
                ? "Complete your profile to unlock personalized nutrition insights."
                : caloriesConsumed < calorieGoal
                    ? $"You're {calorieGoal - caloriesConsumed} calories under your goal. Consider a protein-rich snack."
                    : "Great job staying on track with your nutrition today!");

        var weeklyCalories = new List<DailyCaloriePointDto>();
        for (var i = 6; i >= 0; i--)
        {
            var day = today.Date.AddDays(-i);
            var dayMeals = await _mealLogRepository.GetByUserForDateAsync(userId, day, cancellationToken);
            weeklyCalories.Add(new DailyCaloriePointDto(day.ToString("ddd"), dayMeals.Sum(m => m.Calories)));
        }

        var recentWeights = weightHistory
            .OrderByDescending(h => h.LoggedAt)
            .Take(7)
            .Reverse()
            .ToList();

        var weightTrend = recentWeights.Count > 0
            ? recentWeights.Select((h, index) =>
                new DailyWeightPointDto($"W{index + 1}", h.WeightKg)).ToList()
            : new List<DailyWeightPointDto>
            {
                new("Now", latestWeight?.WeightKg ?? goal?.CurrentWeightKg)
            };

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
            plans.Take(3).Select(p => new SavedPlanDto(p.Name, p.TimelineWeeks * 7)).ToList(),
            weeklyCalories,
            weightTrend,
            latestReport?.BestDay,
            latestReport?.WorstDay);
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
            if (streak > 365) break;
        }
        return streak;
    }
}
