namespace NutriAI.Application.Interfaces.Services;

public interface IWeightService
{
    Task<object> GetDataAsync(string userId, CancellationToken cancellationToken = default);
    Task<object> AddWeightAsync(string userId, double weight, CancellationToken cancellationToken = default);
}
