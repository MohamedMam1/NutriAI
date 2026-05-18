using NutriAI.Application.DTOs;

namespace NutriAI.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(string userId, CancellationToken cancellationToken = default);
}
