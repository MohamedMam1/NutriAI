using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<Domain.Entities.ApplicationUser> _userManager;

    public AdminService(
        ApplicationDbContext context,
        IUserRepository userRepository,
        UserManager<Domain.Entities.ApplicationUser> userManager)
    {
        _context = context;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<object> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var activeUsers = await _userRepository.CountActiveAsync(cancellationToken);
        var totalMealLogs = await _context.MealLogs.CountAsync(cancellationToken);
        var totalRecipes = await _context.RecipeAnalyses.CountAsync(cancellationToken);

        return new { totalUsers, activeUsers, totalMealLogs, totalRecipes };
    }

    public async Task<object> GetUsersAsync(string? search, int page, CancellationToken cancellationToken = default)
    {
        const int pageSize = 5;
        var users = await _userRepository.SearchAsync(search, page, pageSize, cancellationToken);
        var total = await _userRepository.CountAsync(search, cancellationToken);

        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var hasLogs = await _context.MealLogs.AnyAsync(m => m.UserId == u.Id, cancellationToken);
            result.Add(new
            {
                id = Math.Abs(u.Id.GetHashCode()),
                name = u.FullName,
                email = u.Email,
                status = hasLogs ? "Active" : "Inactive",
                joined = u.CreatedAt.ToString("yyyy-MM-dd")
            });
        }

        return new
        {
            users = result,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }
}
