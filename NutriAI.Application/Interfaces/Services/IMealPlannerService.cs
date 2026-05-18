namespace NutriAI.Application.Interfaces.Services;

public interface IMealPlannerService
{
    Task<object> GeneratePlanAsync(string userId, double goalWeight, int timelineWeeks, string dietaryPreference, CancellationToken cancellationToken = default);
}
