using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync(cancellationToken);
}
