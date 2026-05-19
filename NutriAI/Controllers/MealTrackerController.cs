using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
public class MealTrackerController : Controller
{
    private readonly IMealTrackerService _mealTrackerService;

    public MealTrackerController(IMealTrackerService mealTrackerService)
    {
        _mealTrackerService = mealTrackerService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Meal Tracker";
        ViewData["ActiveNav"] = "MealTracker";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMeals(CancellationToken cancellationToken) =>
        Json(await _mealTrackerService.GetMealsAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] MealAnalyzeRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mealTrackerService.AnalyzeMealAsync(User.GetUserId(), request, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Json(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mealTrackerService.DeleteMealAsync(User.GetUserId(), id, cancellationToken);
        return Json(new { success = true });
    }
}
