using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IWaterLogRepository : IGenericRepository<WaterLog>
{
    Task<int> GetTotalForDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaterLog>> GetByUserForDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default);
}
