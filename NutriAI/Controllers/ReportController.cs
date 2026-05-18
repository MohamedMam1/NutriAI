using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Extensions;

namespace NutriAI.Controllers;

public class ReportController : Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Weekly Report";
        ViewData["ActiveNav"] = "Report";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetWeeklyData(CancellationToken cancellationToken) =>
        Json(await _reportService.GetWeeklyDataAsync(User.GetUserId(), cancellationToken));
}
