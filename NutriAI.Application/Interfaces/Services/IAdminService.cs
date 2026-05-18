namespace NutriAI.Application.Interfaces.Services;

public interface IAdminService
{
    Task<object> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<object> GetUsersAsync(string? search, int page, CancellationToken cancellationToken = default);
}
