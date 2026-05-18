using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class WeeklyReportRepository : GenericRepository<WeeklyReport>, IWeeklyReportRepository
{
    public WeeklyReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WeeklyReport?> GetLatestByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(x => x.UserId == userId).OrderByDescending(x => x.GeneratedAt).FirstOrDefaultAsync(cancellationToken);
}
