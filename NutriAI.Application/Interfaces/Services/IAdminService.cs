using NutriAI.Application.DTOs;

namespace NutriAI.Application.Interfaces.Services;

public interface IAdminService
{
    Task<object> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<object> GetUsersAsync(string? search, int page, CancellationToken cancellationToken = default);
    Task<object?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<object> CreateUserAsync(AdminCreateUserDto dto, CancellationToken cancellationToken = default);
    Task<object> UpdateUserAsync(string userId, AdminUpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<object> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<object> SetBanAsync(string userId, bool banned, CancellationToken cancellationToken = default);
    Task<object> GetMealLogsAsync(int page, string? userId, CancellationToken cancellationToken = default);
    Task<object> GetRecipeAnalysesAsync(int page, string? userId, CancellationToken cancellationToken = default);
    Task<object> GetWeeklyReportsAsync(int page, string? userId, CancellationToken cancellationToken = default);
}
