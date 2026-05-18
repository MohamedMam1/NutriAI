using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IRecipeRepository : IGenericRepository<Recipe>
{
    Task<Recipe?> GetWithAnalysesAsync(int id, CancellationToken cancellationToken = default);
}
