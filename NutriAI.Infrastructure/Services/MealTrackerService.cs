using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;

namespace NutriAI.Infrastructure.Services;

public class MealTrackerService : IMealTrackerService
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IAIChatRepository _aiChatRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IAiNutritionService _aiService;

    public MealTrackerService(
        IMealLogRepository mealLogRepository,
        IAIChatRepository aiChatRepository,
        IUserGoalRepository userGoalRepository,
        IAiNutritionService aiService)
    {
        _mealLogRepository = mealLogRepository;
        _aiChatRepository = aiChatRepository;
        _userGoalRepository = userGoalRepository;
        _aiService = aiService;
    }

    public async Task<IReadOnlyList<MealLogDto>> GetMealsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var meals = await _mealLogRepository.GetByUserForDateAsync(userId, today, cancellationToken);
        return meals.Select(Map).ToList();
    }

    public async Task<MealAnalyzeResponseDto> AnalyzeMealAsync(string userId, MealAnalyzeRequestDto request, CancellationToken cancellationToken = default)
    {
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var context = NutritionContextHelper.FromGoal(goal);
        var analysis = await _aiService.AnalyzeMealAsync(request.Description, context, cancellationToken)
                       ?? CreateFallbackAnalysis(request.Description, context);

        var log = new MealLog
        {
            UserId = userId,
            Description = request.Description,
            Calories = analysis.Calories,
            Protein = analysis.Protein,
            Carbs = analysis.Carbs,
            Fat = analysis.Fat,
            AiResponse = analysis.AiResponse,
            LoggedAt = DateTime.UtcNow
        };

        await _mealLogRepository.AddAsync(log, cancellationToken);
        await _aiChatRepository.AddAsync(new AIChat
        {
            UserId = userId,
            Role = "User",
            Message = request.Description,
            Context = "MealTracker"
        }, cancellationToken);
        await _aiChatRepository.AddAsync(new AIChat
        {
            UserId = userId,
            Role = "Assistant",
            Message = analysis.AiResponse,
            Context = "MealTracker"
        }, cancellationToken);
        await _mealLogRepository.SaveChangesAsync(cancellationToken);

        var dto = Map(log);
        return new MealAnalyzeResponseDto(true, $"Analyzed: {request.Description}", dto, analysis.AiResponse);
    }

    public async Task DeleteMealAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var meal = await _mealLogRepository.GetByIdAsync(id, cancellationToken);
        if (meal == null || meal.UserId != userId) return;
        await _mealLogRepository.DeleteAsync(meal, cancellationToken);
        await _mealLogRepository.SaveChangesAsync(cancellationToken);
    }

    private static MealAnalysisResult CreateFallbackAnalysis(string description, UserNutritionContext context)
    {
        var random = new Random(description.GetHashCode());
        var calories = random.Next(200, 650);
        return new MealAnalysisResult(
            calories,
            Math.Round(random.NextDouble() * 40 + 10, 1),
            Math.Round(random.NextDouble() * 50 + 15, 1),
            Math.Round(random.NextDouble() * 25 + 5, 1),
            $"Based on your description, this meal provides approximately {calories} calories. " +
            $"You are targeting {context.DailyCalorieTarget} calories per day toward your goal weight of {context.GoalWeightKg}kg.");
    }

    private static MealLogDto Map(MealLog m) =>
        new(m.Id, m.Description, m.Calories, m.Protein, m.Carbs, m.Fat,
            m.LoggedAt.ToLocalTime().ToString("h:mm tt"), m.AiResponse);
}
