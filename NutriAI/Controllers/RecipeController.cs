using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

public class RecipeController : Controller
{
    private readonly IRecipeService _recipeService;

    public RecipeController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Recipe Analyzer";
        ViewData["ActiveNav"] = "Recipe";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] RecipeAnalyzeRequest request, CancellationToken cancellationToken) =>
        Json(await _recipeService.AnalyzeRecipeAsync(User.GetUserId(), request.RecipeText, cancellationToken));
}

public class RecipeAnalyzeRequest
{
    public string RecipeText { get; set; } = string.Empty;
}
