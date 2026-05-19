using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;
using NutriAI.ViewModels.Profile;

namespace NutriAI.Controllers;

[Authorize(Roles = Roles.User)]
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
        if (profile == null)
            return View(new ProfileViewModel());

        var hasGoal = profile.Height > 0 && profile.Age > 0;
        var vm = new ProfileViewModel
        {
            Email = profile.Email,
            Name = profile.Name,
            Age = profile.Age,
            Gender = string.IsNullOrEmpty(profile.Gender) ? "Male" : profile.Gender,
            Height = profile.Height,
            CurrentWeight = profile.CurrentWeight,
            GoalWeight = profile.GoalWeight,
            ActivityLevel = string.IsNullOrEmpty(profile.ActivityLevel) ? "Moderately Active" : profile.ActivityLevel,
            DailyWaterTargetMl = profile.DailyWaterTargetMl,
            HasCompletedProfile = hasGoal
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Json(await _profileService.GetProfileAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] ProfileDto dto, CancellationToken cancellationToken)
    {
        var current = await _profileService.GetProfileAsync(User.GetUserId(), cancellationToken);
        if (current != null && !string.IsNullOrEmpty(current.Email))
            dto = dto with { Email = current.Email };

        var result = await _profileService.SaveProfileAsync(User.GetUserId(), dto, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message, errors = result.Errors });
    }
}
