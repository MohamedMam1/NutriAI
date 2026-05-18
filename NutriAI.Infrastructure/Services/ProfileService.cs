using Microsoft.AspNetCore.Identity;
using NutriAI.Application.Common;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;

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
            return new ProfileDto(
                user.Email ?? string.Empty,
                user.FullName,
                0,
                string.Empty,
                0,
                0,
                0,
                string.Empty,
                0);
        }

        return new ProfileDto(
            user.Email ?? string.Empty,
            user.FullName,
            goal.Age,
            goal.Gender,
            goal.HeightCm,
            goal.CurrentWeightKg,
            goal.GoalWeightKg,
            goal.ActivityLevel,
            goal.DailyWaterTargetMl);
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
        var (dailyCalories, calculatedWater) = NutritionTargetsCalculator.Calculate(dto);
        goal.DailyCalorieTarget = dailyCalories;
        goal.DailyWaterTargetMl = dto.DailyWaterTargetMl > 0 ? dto.DailyWaterTargetMl : calculatedWater;
        goal.UpdatedAt = DateTime.UtcNow;

        await _userGoalRepository.UpdateAsync(goal, cancellationToken);
        await _userGoalRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Profile saved.");
    }
}
