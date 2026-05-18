using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Admin Dashboard";
        ViewData["ActiveNav"] = "Admin";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken) =>
        Json(await _adminService.GetStatsAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> GetUsers(int page = 1, string? search = null, CancellationToken cancellationToken = default) =>
        Json(await _adminService.GetUsersAsync(search, page, cancellationToken));
}
