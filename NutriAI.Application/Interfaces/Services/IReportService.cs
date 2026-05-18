namespace NutriAI.Application.Interfaces.Services;

public interface IReportService
{
    Task<object> GetWeeklyDataAsync(string userId, CancellationToken cancellationToken = default);
}
