using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;

namespace NutriAI.Infrastructure.Services;

public class WeightService : IWeightService
{
    private readonly IWeightLogRepository _weightLogRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IAiNutritionService _aiService;

    public WeightService(
        IWeightLogRepository weightLogRepository,
        IUserGoalRepository userGoalRepository,
        IAiNutritionService aiService)
    {
        _weightLogRepository = weightLogRepository;
        _userGoalRepository = userGoalRepository;
        _aiService = aiService;
    }

    public async Task<object> GetDataAsync(string userId, CancellationToken cancellationToken = default)
    {
        var history = await _weightLogRepository.GetByUserAsync(userId, cancellationToken);
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var current = history.LastOrDefault()?.WeightKg ?? goal?.CurrentWeightKg ?? 0;
        var context = NutritionContextHelper.FromGoal(goal);

        var aiInsight = await _aiService.GetWeightInsightAsync(context, current, cancellationToken)
            ?? "Log meals daily so AI can correlate your nutrition patterns with weight progress toward your goal.";

        return new
        {
            currentWeight = current,
            goalWeight = goal?.GoalWeightKg ?? 0,
            startWeight = history.FirstOrDefault()?.WeightKg ?? current,
            aiInsight,
            history = history.Select(h => new { date = h.LoggedAt.ToString("yyyy-MM-dd"), weight = h.WeightKg })
        };
    }

    public async Task<object> AddWeightAsync(string userId, double weight, CancellationToken cancellationToken = default)
    {
        if (weight is < 20 or > 500)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be between 20 and 500 kg.");

        var log = new WeightLog { UserId = userId, WeightKg = weight, LoggedAt = DateTime.UtcNow };
        await _weightLogRepository.AddAsync(log, cancellationToken);

        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        if (goal != null)
        {
            goal.CurrentWeightKg = weight;
            goal.UpdatedAt = DateTime.UtcNow;
            await _userGoalRepository.UpdateAsync(goal, cancellationToken);
        }

        await _weightLogRepository.SaveChangesAsync(cancellationToken);
        return new { success = true, entry = new { date = log.LoggedAt.ToString("yyyy-MM-dd"), weight } };
    }
}
