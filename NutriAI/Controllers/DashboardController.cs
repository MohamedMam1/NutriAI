using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewData["ActiveNav"] = "Dashboard";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        try
        {
            var data = await _dashboardService.GetSummaryAsync(User.GetUserId(), cancellationToken);
            return Json(data);
        }
        catch (Exception)
        {
            return Json(new
            {
                caloriesConsumed = 0,
                caloriesGoal = 0,
                currentWeight = 0,
                goalWeight = 0,
                waterMl = 0,
                waterGoalMl = 0,
                weeklyStreak = 0,
                aiInsight = "We could not load your dashboard right now. Please refresh or complete your profile.",
                recentMeals = Array.Empty<object>(),
                savedPlans = Array.Empty<object>(),
                weeklyCalories = Array.Empty<object>(),
                weightTrend = Array.Empty<object>()
            });
        }
    }
}
