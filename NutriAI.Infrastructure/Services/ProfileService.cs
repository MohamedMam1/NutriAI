using Microsoft.AspNetCore.Identity;
using NutriAI.Application.Common;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserGoalRepository _userGoalRepository;

    public ProfileService(UserManager<ApplicationUser> userManager, IUserGoalRepository userGoalRepository)
    {
        _userManager = userManager;
        _userGoalRepository = userGoalRepository;
    }

    public async Task<ProfileDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        if (goal == null)
        {
            return new ProfileDto(user.FullName, 28, "Male", 175, 78, 72, "Moderately Active");
        }

        return new ProfileDto(user.FullName, goal.Age, goal.Gender, goal.HeightCm,
            goal.CurrentWeightKg, goal.GoalWeightKg, goal.ActivityLevel);
    }

    public async Task<ServiceResult> SaveProfileAsync(string userId, ProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ServiceResult.Failure("User not found.");

        user.FullName = dto.Name;
        await _userManager.UpdateAsync(user);

        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        if (goal == null)
        {
            goal = new UserGoal { UserId = userId };
            await _userGoalRepository.AddAsync(goal, cancellationToken);
        }

        goal.Age = dto.Age;
        goal.Gender = dto.Gender;
        goal.HeightCm = dto.Height;
        goal.CurrentWeightKg = dto.CurrentWeight;
        goal.GoalWeightKg = dto.GoalWeight;
        goal.ActivityLevel = dto.ActivityLevel;
        goal.UpdatedAt = DateTime.UtcNow;

        await _userGoalRepository.UpdateAsync(goal, cancellationToken);
        await _userGoalRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Profile saved.");
    }
}
