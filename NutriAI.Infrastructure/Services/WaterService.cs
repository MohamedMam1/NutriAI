using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Services;

public class WaterService : IWaterService
{
    private readonly IWaterLogRepository _waterLogRepository;
    private readonly IUserGoalRepository _userGoalRepository;

    public WaterService(IWaterLogRepository waterLogRepository, IUserGoalRepository userGoalRepository)
    {
        _waterLogRepository = waterLogRepository;
        _userGoalRepository = userGoalRepository;
    }

    public async Task<object> GetStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var goalMl = goal?.DailyWaterTargetMl ?? 2500;
        var currentMl = await _waterLogRepository.GetTotalForDateAsync(userId, DateTime.UtcNow, cancellationToken);
        var percent = Math.Min(100, (int)(currentMl * 100.0 / goalMl));
        return new { currentMl, goalMl, percent };
    }

    public async Task<object> AddWaterAsync(string userId, int amountMl, CancellationToken cancellationToken = default)
    {
        await _waterLogRepository.AddAsync(new WaterLog
        {
            UserId = userId,
            AmountMl = amountMl,
            LoggedAt = DateTime.UtcNow
        }, cancellationToken);
        await _waterLogRepository.SaveChangesAsync(cancellationToken);
        return await GetStatusAsync(userId, cancellationToken);
    }
}
