using NutriAI.Application.Common;
using NutriAI.Application.DTOs;

namespace NutriAI.Application.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult> SaveProfileAsync(string userId, ProfileDto dto, CancellationToken cancellationToken = default);
}
