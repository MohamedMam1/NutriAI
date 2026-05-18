using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IUserGoalRepository : IGenericRepository<UserGoal>
{
    Task<UserGoal?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
