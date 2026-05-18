using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IMealLogRepository : IGenericRepository<MealLog>
{
    Task<IReadOnlyList<MealLog>> GetByUserForDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MealLog>> GetRecentByUserAsync(string userId, int count, CancellationToken cancellationToken = default);
}
