using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class UserGoalRepository : GenericRepository<UserGoal>, IUserGoalRepository
{
    public UserGoalRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserGoal?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
}
