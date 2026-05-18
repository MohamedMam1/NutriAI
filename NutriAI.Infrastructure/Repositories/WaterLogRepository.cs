using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class WaterLogRepository : GenericRepository<WaterLog>, IWaterLogRepository
{
    public WaterLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> GetTotalForDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await DbSet
            .Where(x => x.UserId == userId && x.LoggedAt >= start && x.LoggedAt < end)
            .SumAsync(x => x.AmountMl, cancellationToken);
    }

    public async Task<IReadOnlyList<WaterLog>> GetByUserForDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await DbSet
            .Where(x => x.UserId == userId && x.LoggedAt >= start && x.LoggedAt < end)
            .OrderByDescending(x => x.LoggedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
