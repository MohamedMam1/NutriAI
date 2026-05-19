using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
public class MealPlannerController : Controller
{
    private readonly IMealPlannerService _mealPlannerService;

    public MealPlannerController(IMealPlannerService mealPlannerService)
    {
        _mealPlannerService = mealPlannerService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "AI Meal Planner";
        ViewData["ActiveNav"] = "MealPlanner";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] MealPlanGenerateRequest request, CancellationToken cancellationToken) =>
        Json(await _mealPlannerService.GeneratePlanAsync(
            User.GetUserId(), request.GoalWeight, request.TimelineWeeks, request.DietaryPreference, cancellationToken));
}

public class MealPlanGenerateRequest
{
    public double GoalWeight { get; set; }
    public int TimelineWeeks { get; set; }
    public string DietaryPreference { get; set; } = string.Empty;
}
