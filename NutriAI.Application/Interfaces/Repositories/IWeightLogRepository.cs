using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IWeightLogRepository : IGenericRepository<WeightLog>
{
    Task<IReadOnlyList<WeightLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<WeightLog?> GetLatestByUserAsync(string userId, CancellationToken cancellationToken = default);
}
