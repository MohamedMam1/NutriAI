using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Services;

public class AdminService : IAdminService
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(
        ApplicationDbContext context,
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<object> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var bannedUsers = await _context.Users.CountAsync(u => u.IsBanned, cancellationToken);
        var activeUsers = await _userRepository.CountActiveAsync(cancellationToken);
        var totalMealLogs = await _context.MealLogs.CountAsync(cancellationToken);
        var totalRecipes = await _context.RecipeAnalyses.CountAsync(cancellationToken);
        var totalReports = await _context.WeeklyReports.CountAsync(cancellationToken);
        var totalPlans = await _context.MealPlans.CountAsync(cancellationToken);

        return new
        {
            totalUsers,
            activeUsers,
            bannedUsers,
            totalMealLogs,
            totalRecipes,
            totalReports,
            totalPlans,
            status = bannedUsers > 0 ? "Attention needed" : "Healthy"
        };
    }

    public async Task<object> GetUsersAsync(string? search, int page, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.SearchAsync(search, page, PageSize, cancellationToken);
        var total = await _userRepository.CountAsync(search, cancellationToken);

        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var mealCount = await _context.MealLogs.CountAsync(m => m.UserId == u.Id, cancellationToken);
            result.Add(new
            {
                id = u.Id,
                name = u.FullName,
                email = u.Email,
                isBanned = u.IsBanned,
                status = u.IsBanned ? "Banned" : mealCount > 0 ? "Active" : "Inactive",
                roles,
                mealCount,
                joined = u.CreatedAt.ToString("yyyy-MM-dd")
            });
        }

        return new { users = result, total, page, pageSize = PageSize, totalPages = (int)Math.Ceiling(total / (double)PageSize) };
    }

    public async Task<object?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new
        {
            id = user.Id,
            name = user.FullName,
            email = user.Email,
            isBanned = user.IsBanned,
            roles,
            joined = user.CreatedAt.ToString("yyyy-MM-dd")
        };
    }

    public async Task<object> CreateUserAsync(AdminCreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return new { success = false, message = "A user with this email already exists." };

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) };

        await _userManager.AddToRoleAsync(user, Roles.User);
        return new { success = true, message = "User created successfully.", userId = user.Id };
    }

    public async Task<object> UpdateUserAsync(string userId, AdminUpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new { success = false, message = "User not found." };

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            return new { success = false, message = "Admin accounts cannot be edited here." };

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.IsBanned = dto.IsBanned;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? new { success = true, message = "User updated successfully." }
            : new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) };
    }

    public async Task<object> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new { success = false, message = "User not found." };

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            return new { success = false, message = "Admin accounts cannot be deleted." };

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? new { success = true, message = "User deleted successfully." }
            : new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) };
    }

    public async Task<object> SetBanAsync(string userId, bool banned, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new { success = false, message = "User not found." };

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            return new { success = false, message = "Admin accounts cannot be banned." };

        user.IsBanned = banned;
        await _userManager.UpdateAsync(user);
        return new { success = true, message = banned ? "User has been banned." : "User has been unbanned." };
    }

    public async Task<object> GetMealLogsAsync(int page, string? userId, CancellationToken cancellationToken = default)
    {
        IQueryable<MealLog> query = _context.MealLogs.AsNoTracking().Include(m => m.User);
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(m => m.UserId == userId);

        var ordered = query.OrderByDescending(m => m.LoggedAt);
        var total = await ordered.CountAsync(cancellationToken);
        var items = await ordered.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken);

        return new
        {
            items = items.Select(m => new
            {
                m.Id,
                userId = m.UserId,
                userName = m.User.FullName,
                userEmail = m.User.Email,
                m.Description,
                m.Calories,
                m.Protein,
                m.Carbs,
                m.Fat,
                loggedAt = m.LoggedAt.ToString("yyyy-MM-dd HH:mm")
            }),
            total,
            page,
            pageSize = PageSize,
            totalPages = (int)Math.Ceiling(total / (double)PageSize)
        };
    }

    public async Task<object> GetRecipeAnalysesAsync(int page, string? userId, CancellationToken cancellationToken = default)
    {
        var query = from r in _context.RecipeAnalyses.AsNoTracking()
                    join u in _context.Users.AsNoTracking() on r.UserId equals u.Id
                    join recipe in _context.Recipes.AsNoTracking() on r.RecipeId equals recipe.Id
                    orderby r.Id descending
                    select new { r, u, recipe };

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(x => x.r.UserId == userId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken);

        return new
        {
            items = items.Select(x => new
            {
                x.r.Id,
                userId = x.r.UserId,
                userName = x.u.FullName,
                userEmail = x.u.Email,
                recipeName = x.recipe.Title,
                x.r.TotalCalories,
                x.r.Servings,
                ingredients = JsonSerializer.Deserialize<object>(x.r.IngredientsJson),
                analyzedAt = x.recipe.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }),
            total,
            page,
            pageSize = PageSize,
            totalPages = (int)Math.Ceiling(total / (double)PageSize)
        };
    }

    public async Task<object> GetWeeklyReportsAsync(int page, string? userId, CancellationToken cancellationToken = default)
    {
        IQueryable<WeeklyReport> query = _context.WeeklyReports.AsNoTracking().Include(r => r.User);
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(r => r.UserId == userId);

        var ordered = query.OrderByDescending(r => r.GeneratedAt);
        var total = await ordered.CountAsync(cancellationToken);
        var items = await ordered.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(cancellationToken);

        return new
        {
            items = items.Select(r => new
            {
                r.Id,
                userId = r.UserId,
                userName = r.User.FullName,
                r.WeightChangeKg,
                r.AverageCalories,
                r.HydrationScore,
                bestDay = r.BestDay,
                worstDay = r.WorstDay,
                generatedAt = r.GeneratedAt.ToString("yyyy-MM-dd")
            }),
            total,
            page,
            pageSize = PageSize,
            totalPages = (int)Math.Ceiling(total / (double)PageSize)
        };
    }
}
