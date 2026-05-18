using NutriAI.Domain.Entities;

namespace NutriAI.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
}
