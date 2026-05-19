using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Domain.Constants;
using NutriAI.Extensions;
using NutriAI.ViewModels.Auth;

namespace NutriAI.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Domain.Entities.ApplicationUser> _userManager;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration,
        UserManager<Domain.Entities.ApplicationUser> userManager)
    {
        _authService = authService;
        _configuration = configuration;
        _userManager = userManager;
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Login";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.LoginAsync(new LoginDto(model.Email, model.Password, model.RememberMe));
        if (!result.Succeeded)
        {
            if (result.ErrorCode == "EmailNotConfirmed")
            {
                model.ShowEmailConfirmationWarning = true;
                model.PendingConfirmationEmail = model.Email;
                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Please confirm your email.");
                return View(model);
            }

            if (result.ErrorCode == "AccountBanned")
            {
                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Your account has been banned.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Login failed.");
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        if (user != null && await _userManager.IsInRoleAsync(user, Roles.Admin))
            return RedirectToAction("Index", "Admin");

        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.RegisterAsync(
            new RegisterDto(
                model.FullName,
                model.Email,
                model.Password,
                model.ConfirmPassword,
                model.Age,
                model.Gender,
                model.HeightCm,
                model.CurrentWeightKg,
                model.GoalWeightKg,
                model.ActivityLevel,
                model.DailyWaterTargetMl),
            BuildCallbackUrl("/Auth/ConfirmEmail?userId={0}&token={1}"));

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = result.Message;
        TempData["RegisteredEmail"] = model.Email;
        return RedirectToAction(nameof(RegisterConfirmation));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult RegisterConfirmation()
    {
        ViewData["Title"] = "Check Your Email";
        ViewData["RegisteredEmail"] = TempData["RegisteredEmail"]?.ToString();
        return View();
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _authService.ForgotPasswordAsync(
            new ForgotPasswordDto(model.Email),
            BuildCallbackUrl("/Auth/ResetPassword?email={0}&token={1}"));

        TempData["Success"] = "If an account exists, a password reset link has been sent.";
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    [AllowAnonymous, HttpGet]
    public IActionResult ResetPassword(string email, string token) =>
        View(new ResetPasswordViewModel { Email = email, Token = token });

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.ResetPasswordAsync(
            new ResetPasswordDto(model.Email, model.Token, model.Password, model.ConfirmPassword));

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        ViewData["Message"] = result.Succeeded ? result.Message : result.Errors.FirstOrDefault();
        ViewData["Success"] = result.Succeeded;
        return View();
    }

    [AllowAnonymous, HttpGet]
    public IActionResult ResendConfirmation()
    {
        ViewData["Email"] = Request.Query["email"].ToString();
        return View();
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        var result = await _authService.ResendConfirmationAsync(
            email,
            BuildCallbackUrl("/Auth/ConfirmEmail?userId={0}&token={1}"));

        TempData["Success"] = result.Succeeded
            ? result.Message
            : result.Errors.FirstOrDefault() ?? "Unable to send confirmation email.";

        return RedirectToAction(nameof(ResendConfirmation), new { email });
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.ChangePasswordAsync(
            User.GetUserId(),
            new ChangePasswordDto(model.CurrentPassword, model.NewPassword, model.ConfirmPassword));

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Index", "Profile");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied() => View();

    private string BuildCallbackUrl(string pathTemplate)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = $"{Request.Scheme}://{Request.Host}";

        return baseUrl + string.Format(pathTemplate, "{0}", "{1}");
    }
}
