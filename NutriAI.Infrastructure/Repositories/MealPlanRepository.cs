using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class MealPlanRepository : GenericRepository<MealPlan>, IMealPlanRepository
{
    public MealPlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MealPlan?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MealPlan>> GetByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync(cancellationToken);
}
