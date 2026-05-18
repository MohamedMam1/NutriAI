namespace NutriAI.Application.Interfaces.Services;

public interface IWaterService
{
    Task<object> GetStatusAsync(string userId, CancellationToken cancellationToken = default);
    Task<object> AddWaterAsync(string userId, int amountMl, CancellationToken cancellationToken = default);
}
