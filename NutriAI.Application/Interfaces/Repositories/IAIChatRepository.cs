using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IAIChatRepository : IGenericRepository<AIChat>
{
    Task<IReadOnlyList<AIChat>> GetByUserAndContextAsync(string userId, string context, CancellationToken cancellationToken = default);
}
