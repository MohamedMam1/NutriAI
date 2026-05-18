using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;

namespace NutriAI.Infrastructure.Services;

public class MealPlannerService : IMealPlannerService
{
    private readonly IMealPlanRepository _mealPlanRepository;

    private static readonly string[] Days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    public MealPlannerService(IMealPlanRepository mealPlanRepository)
    {
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<object> GeneratePlanAsync(string userId, double goalWeight, int timelineWeeks, string dietaryPreference, CancellationToken cancellationToken = default)
    {
        var plan = new MealPlan
        {
            UserId = userId,
            Name = $"{dietaryPreference} Plan",
            GoalWeightKg = goalWeight,
            TimelineWeeks = timelineWeeks,
            DietaryPreference = dietaryPreference,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var day in Days)
        {
            plan.Items.Add(CreateItem(day, "Breakfast", "Balanced breakfast bowl", 380, "Prepare oats, fruit, and protein."));
            plan.Items.Add(CreateItem(day, "Lunch", "Lean protein lunch", 520, "Grill protein with vegetables and whole grains."));
            plan.Items.Add(CreateItem(day, "Dinner", "Light dinner plate", 480, "Bake or steam dinner with greens."));
            plan.Items.Add(CreateItem(day, "Snacks", "Healthy snack", 200, "Portion nuts or yogurt with fruit."));
        }

        await _mealPlanRepository.AddAsync(plan, cancellationToken);
        await _mealPlanRepository.SaveChangesAsync(cancellationToken);

        return new
        {
            success = true,
            goalWeight,
            timelineWeeks,
            preference = dietaryPreference,
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
    }

    private static MealPlanItem CreateItem(string day, string type, string name, int calories, string instructions) =>
        new()
        {
            DayName = day,
            MealType = type,
            Name = name,
            Calories = calories,
            Protein = 25,
            Carbs = 40,
            Fat = 12,
            Instructions = instructions
        };
}
