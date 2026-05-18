using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Recipe?> GetWithAnalysesAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(x => x.Analyses).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
