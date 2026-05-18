using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriAI.Infrastructure.Data;
using NutriAI.Models;

namespace NutriAI.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    /// <summary>
    /// Returns live platform statistics for the landing page metrics section.
    /// Cached for 5 minutes via ResponseCache to avoid hammering the DB on every visit.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetLandingStats(CancellationToken ct)
    {
        var totalUsers = await _context.Users.CountAsync(ct);
        var totalMealsAnalyzed = await _context.MealLogs.CountAsync(ct);
        var totalMealPlans = await _context.MealPlans.CountAsync(ct);
        var totalWeightLogs = await _context.WeightLogs.CountAsync(ct);
        var totalRecipes = await _context.RecipeAnalyses.CountAsync(ct);

        return Json(new
        {
            totalUsers,
            totalMealsAnalyzed,
            totalMealPlans,
            totalWeightLogs,
            totalRecipes
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
