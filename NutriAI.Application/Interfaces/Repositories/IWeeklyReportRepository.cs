using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface IWeeklyReportRepository : IGenericRepository<WeeklyReport>
{
    Task<WeeklyReport?> GetLatestByUserAsync(string userId, CancellationToken cancellationToken = default);
}
