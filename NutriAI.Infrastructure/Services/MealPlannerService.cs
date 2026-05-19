using System.Text.Json;
using NutriAI.Application.Common;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;

namespace NutriAI.Infrastructure.Services;

public class MealPlannerService : IMealPlannerService
{
    private static readonly string[] Days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IAiNutritionService _aiService;
    private readonly IFallbackNutritionRepository _fallbackRepository;

    public MealPlannerService(
        IMealPlanRepository mealPlanRepository,
        IUserGoalRepository userGoalRepository,
        IAiNutritionService aiService,
        IFallbackNutritionRepository fallbackRepository)
    {
        _mealPlanRepository = mealPlanRepository;
        _userGoalRepository = userGoalRepository;
        _aiService = aiService;
        _fallbackRepository = fallbackRepository;
    }

    public async Task<object> GeneratePlanAsync(string userId, double goalWeight, int timelineWeeks, string dietaryPreference, CancellationToken cancellationToken = default)
    {
        if (!_aiService.IsConfigured)
        {
            return new
            {
                success = false,
                message = AiMessages.ApiKeyNotConfigured,
                dataSource = AiDataSource.Unavailable
            };
        }

        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var context = NutritionContextHelper.FromGoal(goal);
        var dataSource = AiDataSource.Ai;
        var userMessage = string.Empty;

        var plan = new MealPlan
        {
            UserId = userId,
            Name = $"{dietaryPreference} Plan",
            GoalWeightKg = goalWeight,
            TimelineWeeks = timelineWeeks,
            DietaryPreference = dietaryPreference,
            CreatedAt = DateTime.UtcNow
        };

        var aiDays = await _aiService.GenerateMealPlanAsync(goalWeight, timelineWeeks, dietaryPreference, context, cancellationToken);
        if (aiDays is { Count: > 0 })
        {
            ApplyAiDays(plan, aiDays);
        }
        else
        {
            var existing = await _fallbackRepository.GetLatestUserMealPlanAsync(userId, dietaryPreference, cancellationToken);
            if (existing?.Items.Count > 0)
            {
                CopyPlanItems(plan, existing);
                dataSource = AiDataSource.Database;
                userMessage = AiMessages.AiUnavailableUseDatabase;
            }
            else
            {
                var template = await _fallbackRepository.GetMealPlanTemplateAsync(dietaryPreference, cancellationToken);
                if (template != null && ApplySeedPlanJson(plan, template.PlanJson))
                {
                    dataSource = AiDataSource.Database;
                    userMessage = AiMessages.AiUnavailableUseDatabase;
                }
                else
                {
                    return new
                    {
                        success = false,
                        message = AiMessages.InformationUnavailable,
                        dataSource = AiDataSource.Unavailable
                    };
                }
            }
        }

        await _mealPlanRepository.AddAsync(plan, cancellationToken);
        await _mealPlanRepository.SaveChangesAsync(cancellationToken);

        return BuildResponse(plan, goalWeight, timelineWeeks, dietaryPreference, dataSource, userMessage);
    }

    private static void ApplyAiDays(MealPlan plan, IReadOnlyList<MealPlanDayResult> aiDays)
    {
        foreach (var day in aiDays)
        {
            foreach (var meal in day.Meals)
            {
                plan.Items.Add(new MealPlanItem
                {
                    DayName = day.Day,
                    MealType = meal.MealType,
                    Name = meal.Name,
                    Calories = meal.Calories,
                    Protein = meal.Protein,
                    Carbs = meal.Carbs,
                    Fat = meal.Fat,
                    Instructions = meal.Instructions
                });
            }
        }
    }

    private static void CopyPlanItems(MealPlan target, MealPlan source)
    {
        foreach (var item in source.Items)
        {
            target.Items.Add(new MealPlanItem
            {
                DayName = item.DayName,
                MealType = item.MealType,
                Name = item.Name,
                Calories = item.Calories,
                Protein = item.Protein,
                Carbs = item.Carbs,
                Fat = item.Fat,
                Instructions = item.Instructions
            });
        }
    }

    private static bool ApplySeedPlanJson(MealPlan plan, string planJson)
    {
        try
        {
            var days = JsonSerializer.Deserialize<List<SeedPlanDay>>(planJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (days == null || days.Count == 0) return false;

            foreach (var day in days)
            {
                foreach (var meal in day.Meals)
                {
                    plan.Items.Add(new MealPlanItem
                    {
                        DayName = day.Day,
                        MealType = meal.MealType,
                        Name = meal.Name,
                        Calories = meal.Calories,
                        Protein = meal.Protein,
                        Carbs = meal.Carbs,
                        Fat = meal.Fat,
                        Instructions = meal.Instructions
                    });
                }
            }

            return plan.Items.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static object BuildResponse(MealPlan plan, double goalWeight, int timelineWeeks, string preference, string dataSource, string userMessage) =>
        new
        {
            success = true,
            message = string.IsNullOrEmpty(userMessage) ? "Meal plan generated successfully." : userMessage,
            dataSource,
            goalWeight,
            timelineWeeks,
            preference,
            weeklyPlan = Days.Select(day => new
            {
                day,
                meals = plan.Items.Where(i => i.DayName == day).Select(m => new
                {
                    type = m.MealType,
                    name = m.Name,
                    calories = m.Calories,
                    protein = m.Protein,
                    carbs = m.Carbs,
                    fat = m.Fat,
                    instructions = m.Instructions
                })
            })
        };

    private sealed class SeedPlanDay
    {
        public string Day { get; set; } = string.Empty;
        public List<SeedPlanMeal> Meals { get; set; } = [];
    }

    private sealed class SeedPlanMeal
    {
        public string MealType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public string Instructions { get; set; } = string.Empty;
    }
}
