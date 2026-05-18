using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class AIChatRepository : GenericRepository<AIChat>, IAIChatRepository
{
    public AIChatRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AIChat>> GetByUserAndContextAsync(string userId, string context, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(x => x.UserId == userId && x.Context == context)
            .OrderBy(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
