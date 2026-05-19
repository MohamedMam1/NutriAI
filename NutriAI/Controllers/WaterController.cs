using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
public class WaterController : Controller
{
    private readonly IWaterService _waterService;

    public WaterController(IWaterService waterService)
    {
        _waterService = waterService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Water Tracker";
        ViewData["ActiveNav"] = "Water";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken) =>
        Json(await _waterService.GetStatusAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] WaterAddRequest request, CancellationToken cancellationToken) =>
        Json(await _waterService.AddWaterAsync(User.GetUserId(), request.AmountMl, cancellationToken));
}

public class WaterAddRequest
{
    public int AmountMl { get; set; }
}
