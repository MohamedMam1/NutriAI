using NutriAI.Application.Common;
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
    private readonly IFallbackNutritionRepository _fallbackRepository;

    public MealTrackerService(
        IMealLogRepository mealLogRepository,
        IAIChatRepository aiChatRepository,
        IUserGoalRepository userGoalRepository,
        IAiNutritionService aiService,
        IFallbackNutritionRepository fallbackRepository)
    {
        _mealLogRepository = mealLogRepository;
        _aiChatRepository = aiChatRepository;
        _userGoalRepository = userGoalRepository;
        _aiService = aiService;
        _fallbackRepository = fallbackRepository;
    }

    public async Task<IReadOnlyList<MealLogDto>> GetMealsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var meals = await _mealLogRepository.GetByUserForDateAsync(userId, today, cancellationToken);
        return meals.Select(Map).ToList();
    }

    public async Task<MealAnalyzeResponseDto> AnalyzeMealAsync(string userId, MealAnalyzeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!_aiService.IsConfigured)
        {
            return new MealAnalyzeResponseDto(false, AiMessages.ApiKeyNotConfigured, null, null, AiDataSource.Unavailable);
        }

        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var context = NutritionContextHelper.FromGoal(goal);

        var analysis = await _aiService.AnalyzeMealAsync(request.Description, context, cancellationToken);
        var dataSource = AiDataSource.Ai;
        var userMessage = string.Empty;

        if (analysis == null)
        {
            var template = await _fallbackRepository.GetMealTemplateAsync(cancellationToken: cancellationToken);
            if (template != null)
            {
                analysis = new MealAnalysisResult(
                    template.Calories,
                    template.Protein,
                    template.Carbs,
                    template.Fat,
                    template.AiResponse);
                dataSource = AiDataSource.Database;
                userMessage = AiMessages.AiUnavailableUseDatabase;
            }
            else
            {
                return new MealAnalyzeResponseDto(
                    false,
                    AiMessages.InformationUnavailable,
                    null,
                    null,
                    AiDataSource.Unavailable);
            }
        }

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

        var message = string.IsNullOrEmpty(userMessage)
            ? $"Analyzed: {request.Description}"
            : userMessage;

        return new MealAnalyzeResponseDto(
            true,
            message,
            Map(log),
            analysis.AiResponse,
            dataSource);
    }

    public async Task DeleteMealAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var meal = await _mealLogRepository.GetByIdAsync(id, cancellationToken);
        if (meal == null || meal.UserId != userId) return;
        await _mealLogRepository.DeleteAsync(meal, cancellationToken);
        await _mealLogRepository.SaveChangesAsync(cancellationToken);
    }

    private static MealLogDto Map(MealLog m) =>
        new(m.Id, m.Description, m.Calories, m.Protein, m.Carbs, m.Fat,
            m.LoggedAt.ToLocalTime().ToString("h:mm tt"), m.AiResponse);
}
