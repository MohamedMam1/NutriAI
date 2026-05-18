using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IMealPlanRepository : IGenericRepository<MealPlan>
{
    Task<MealPlan?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MealPlan>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
}
