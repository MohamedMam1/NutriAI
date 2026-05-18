using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

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
        var data = await _dashboardService.GetSummaryAsync(User.GetUserId(), cancellationToken);
        return Json(data);
    }
}
