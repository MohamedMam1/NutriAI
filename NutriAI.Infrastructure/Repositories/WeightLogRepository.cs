using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class WeightLogRepository : GenericRepository<WeightLog>, IWeightLogRepository
{
    public WeightLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<WeightLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(x => x.UserId == userId).OrderBy(x => x.LoggedAt).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<WeightLog?> GetLatestByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(x => x.UserId == userId).OrderByDescending(x => x.LoggedAt).FirstOrDefaultAsync(cancellationToken);
}
