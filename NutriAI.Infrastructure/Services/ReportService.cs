using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using NutriAI.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NutriAI.Application.Interfaces.Repositories;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.AI;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IWeeklyReportRepository _weeklyReportRepository;
    private readonly IUserGoalRepository _userGoalRepository;
    private readonly IAiNutritionService _aiService;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        ApplicationDbContext context,
        IWeeklyReportRepository weeklyReportRepository,
        IUserGoalRepository userGoalRepository,
        IAiNutritionService aiService,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        ILogger<ReportService> logger)
    {
        _context = context;
        _weeklyReportRepository = weeklyReportRepository;
        _userGoalRepository = userGoalRepository;
        _aiService = aiService;
        _emailService = emailService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<object> GetWeeklyDataAsync(string userId, CancellationToken cancellationToken = default)
    {
        var latest = await _weeklyReportRepository.GetLatestByUserAsync(userId, cancellationToken);
        if (latest != null && latest.GeneratedAt > DateTime.UtcNow.AddDays(-6))
        {
            return MapReport(latest);
        }

        var weekStart = DateTime.UtcNow.Date.AddDays(-6);
        var labels = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i).ToString("ddd", CultureInfo.InvariantCulture))
            .ToArray();
        var dailyCalories = new List<int>();
        var weightTrend = new List<double>();
        var hydrationDays = new List<int>();
        var goal = await _userGoalRepository.GetByUserIdAsync(userId, cancellationToken);
        var waterGoal = goal?.DailyWaterTargetMl ?? 0;
        var calorieTarget = goal?.DailyCalorieTarget ?? 0;

        for (var i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            var next = day.AddDays(1);
            var cals = await _context.MealLogs
                .Where(m => m.UserId == userId && m.LoggedAt >= day && m.LoggedAt < next)
                .SumAsync(m => m.Calories, cancellationToken);
            dailyCalories.Add(cals);

            var weight = await _context.WeightLogs
                .Where(w => w.UserId == userId && w.LoggedAt >= day && w.LoggedAt < next)
                .OrderByDescending(w => w.LoggedAt)
                .Select(w => w.WeightKg)
                .FirstOrDefaultAsync(cancellationToken);
            var lastWeight = weightTrend.Count > 0 ? weightTrend[^1] : goal?.CurrentWeightKg ?? 0;
            weightTrend.Add(weight > 0 ? weight : lastWeight);

            var water = await _context.WaterLogs
                .Where(w => w.UserId == userId && w.LoggedAt >= day && w.LoggedAt < next)
                .SumAsync(w => w.AmountMl, cancellationToken);
            hydrationDays.Add(Math.Min(100, (int)(water * 100.0 / waterGoal)));
        }

        var weightChange = weightTrend.Count >= 2 ? Math.Round(weightTrend[^1] - weightTrend[0], 1) : 0;
        var avgCalories = dailyCalories.Count > 0 ? (int)dailyCalories.Average() : 0;
        var hydrationScore = hydrationDays.Count > 0 ? (int)hydrationDays.Average() : 0;
        var bestIdx = dailyCalories.IndexOf(dailyCalories.Max());
        var worstIdx = dailyCalories.IndexOf(dailyCalories.Min());

        var reportSummary =
            $"Weight change: {weightChange}kg. Avg calories: {avgCalories}/{calorieTarget}. Hydration score: {hydrationScore}%. Best day: {labels[Math.Max(0, bestIdx)]}. Worst day: {labels[Math.Max(0, worstIdx)]}.";

        string[] tips;
        string? aiMessage = null;
        if (_aiService.IsConfigured)
        {
            var recommendations = await _aiService.GetWeeklyRecommendationsAsync(reportSummary, cancellationToken);
            if (recommendations is { Count: >= 3 })
            {
                tips = recommendations.Take(3).ToArray();
            }
            else
            {
                var previousTips = latest != null
                    ? JsonSerializer.Deserialize<string[]>(latest.RecommendationsJson)
                    : null;
                if (previousTips is { Length: >= 1 })
                {
                    tips = previousTips.Take(3).ToArray();
                    aiMessage = AiMessages.AiUnavailableUseDatabase;
                }
                else
                {
                    tips = [];
                    aiMessage = AiMessages.InformationUnavailable;
                }
            }
        }
        else
        {
            tips = [];
            aiMessage = AiMessages.ApiKeyNotConfigured;
        }

        var report = new WeeklyReport
        {
            UserId = userId,
            WeekStart = weekStart,
            WeightChangeKg = weightChange,
            AverageCalories = avgCalories,
            HydrationScore = hydrationScore,
            BestDay = labels[Math.Max(0, bestIdx)],
            WorstDay = labels[Math.Max(0, worstIdx)],
            RecommendationsJson = JsonSerializer.Serialize(tips),
            DailyCaloriesJson = JsonSerializer.Serialize(dailyCalories),
            WeightTrendJson = JsonSerializer.Serialize(weightTrend),
            HydrationDaysJson = JsonSerializer.Serialize(hydrationDays),
            GeneratedAt = DateTime.UtcNow
        };

        await _weeklyReportRepository.AddAsync(report, cancellationToken);
        await _weeklyReportRepository.SaveChangesAsync(cancellationToken);

        await TrySendWeeklyReportEmailAsync(userId, report, tips, cancellationToken);

        return new
        {
            success = true,
            message = aiMessage,
            weightChange,
            avgCalories,
            calorieTarget,
            hydrationScore,
            bestDay = report.BestDay,
            worstDay = report.WorstDay,
            aiRecommendations = tips,
            dailyCalories,
            dailyLabels = labels,
            weightTrend,
            hydrationDays
        };
    }

    private async Task TrySendWeeklyReportEmailAsync(
        string userId,
        WeeklyReport report,
        IReadOnlyList<string> tips,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user?.Email == null) return;

        var body = $"""
            <h2>Your NutriAI Weekly Progress Report</h2>
            <p>Weight change: <strong>{report.WeightChangeKg:+#.0;-#.0;0} kg</strong></p>
            <p>Average daily calories: <strong>{report.AverageCalories}</strong></p>
            <p>Hydration score: <strong>{report.HydrationScore}%</strong></p>
            <p>Best day: {report.BestDay} | Needs attention: {report.WorstDay}</p>
            <h3>AI Tips for next week</h3>
            <ul>{string.Join("", tips.Select(t => $"<li>{System.Net.WebUtility.HtmlEncode(t)}</li>"))}</ul>
            <p><em>AI-generated nutrition guidance is not medical advice.</em></p>
            """;

        try
        {
            await _emailService.SendEmailAsync(user.Email, "Your NutriAI Weekly Progress Report", body, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send weekly report email to user {UserId}", userId);
        }
    }

    private static object MapReport(WeeklyReport r) => new
    {
        weightChange = r.WeightChangeKg,
        avgCalories = r.AverageCalories,
        hydrationScore = r.HydrationScore,
        bestDay = r.BestDay,
        worstDay = r.WorstDay,
        aiRecommendations = JsonSerializer.Deserialize<string[]>(r.RecommendationsJson),
        dailyCalories = JsonSerializer.Deserialize<int[]>(r.DailyCaloriesJson),
        dailyLabels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
        weightTrend = JsonSerializer.Deserialize<double[]>(r.WeightTrendJson),
        hydrationDays = JsonSerializer.Deserialize<int[]>(r.HydrationDaysJson)
    };
}
