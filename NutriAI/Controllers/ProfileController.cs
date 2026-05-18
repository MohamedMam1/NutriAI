using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Extensions;
using NutriAI.ViewModels.Profile;

namespace NutriAI.Controllers;

public class ProfileController : Controller
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Profile";
        ViewData["ActiveNav"] = "Profile";
        var profile = await _profileService.GetProfileAsync(User.GetUserId(), cancellationToken);
        var vm = profile == null ? new ProfileViewModel() : new ProfileViewModel
        {
            Name = profile.Name,
            Age = profile.Age,
            Gender = profile.Gender,
            Height = profile.Height,
            CurrentWeight = profile.CurrentWeight,
            GoalWeight = profile.GoalWeight,
            ActivityLevel = profile.ActivityLevel
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Json(await _profileService.GetProfileAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] ProfileDto dto, CancellationToken cancellationToken)
    {
        var result = await _profileService.SaveProfileAsync(User.GetUserId(), dto, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message, errors = result.Errors });
    }
}
