using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Users.AsNoTracking().OrderBy(x => x.Email).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ApplicationUser>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                u.FullName.Contains(search));
        }

        return await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                u.FullName.Contains(search));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        var monthAgo = DateTime.UtcNow.AddDays(-30);
        return await _context.MealLogs
            .Where(m => m.LoggedAt >= monthAgo)
            .Select(m => m.UserId)
            .Distinct()
            .CountAsync(cancellationToken);
    }
}
