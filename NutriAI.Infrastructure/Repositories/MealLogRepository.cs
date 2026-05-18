using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class MealLogRepository : GenericRepository<MealLog>, IMealLogRepository
{
    public MealLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MealLog>> GetByUserForDateAsync(string userId, DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await DbSet
            .Where(x => x.UserId == userId && x.LoggedAt >= start && x.LoggedAt < end)
            .OrderByDescending(x => x.LoggedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MealLog>> GetRecentByUserAsync(string userId, int count, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LoggedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
