using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
public class WeightController : Controller
{
    private readonly IWeightService _weightService;

    public WeightController(IWeightService weightService)
    {
        _weightService = weightService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Weight Tracker";
        ViewData["ActiveNav"] = "Weight";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetData(CancellationToken cancellationToken) =>
        Json(await _weightService.GetDataAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] WeightAddRequest request, CancellationToken cancellationToken) =>
        Json(await _weightService.AddWeightAsync(User.GetUserId(), request.Weight, cancellationToken));

    [HttpDelete]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        Json(await _weightService.DeleteWeightAsync(User.GetUserId(), id, cancellationToken));
}

public class WeightAddRequest
{
    public double Weight { get; set; }
}
