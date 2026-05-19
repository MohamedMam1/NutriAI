using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;
using NutriAI.Infrastructure.Validation;

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

        string? aiInsight = null;
        if (_aiService.IsConfigured)
            aiInsight = await _aiService.GetWeightInsightAsync(context, current, cancellationToken);

        return new
        {
            currentWeight = current,
            goalWeight = goal?.GoalWeightKg ?? 0,
            startWeight = history.FirstOrDefault()?.WeightKg ?? current,
            aiInsight = aiInsight ?? "Log meals and weight consistently to see personalized AI insights here.",
            history = history.Select(h => new { id = h.Id, date = h.LoggedAt.ToString("yyyy-MM-dd"), weight = h.WeightKg })
        };
    }

    public async Task<object> AddWeightAsync(string userId, double weight, CancellationToken cancellationToken = default)
    {
        var latest = await _weightLogRepository.GetLatestByUserAsync(userId, cancellationToken);
        var validationError = WeightValidation.ValidateNewEntry(weight, latest?.WeightKg);
        if (validationError != null)
            return new { success = false, message = validationError };

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
        return new { success = true, message = "Weight saved.", entry = new { id = log.Id, date = log.LoggedAt.ToString("yyyy-MM-dd"), weight } };
    }

    public async Task<object> DeleteWeightAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var log = await _weightLogRepository.GetByIdAsync(id, cancellationToken);
        if (log == null || log.UserId != userId)
            return new { success = false, message = "Weight entry not found." };

        await _weightLogRepository.DeleteAsync(log, cancellationToken);
        await _weightLogRepository.SaveChangesAsync(cancellationToken);
        return new { success = true, message = "Weight entry deleted." };
    }
}
