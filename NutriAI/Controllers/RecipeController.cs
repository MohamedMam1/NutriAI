using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
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
    public async Task<IActionResult> Analyze([FromBody] RecipeAnalyzeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Json(await _recipeService.AnalyzeRecipeAsync(User.GetUserId(), request.RecipeText, cancellationToken));
    }
}

public class RecipeAnalyzeRequest
{
    [Required, MinLength(10), MaxLength(8000)]
    public string RecipeText { get; set; } = string.Empty;
}
