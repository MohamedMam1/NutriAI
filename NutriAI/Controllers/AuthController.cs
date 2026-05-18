using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriAI.Application.DTOs;
using NutriAI.Application.Interfaces.Services;
using NutriAI.Extensions;
using NutriAI.ViewModels.Auth;

namespace NutriAI.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
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
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "Login failed.");
            return View(model);
        }

        return LocalRedirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/Dashboard" : model.ReturnUrl);
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.RegisterAsync(
            new RegisterDto(model.FullName, model.Email, model.Password, model.ConfirmPassword),
            BuildUrl("/Auth/ConfirmEmail?userId={0}&token={1}"));

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(RegisterConfirmation));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult RegisterConfirmation()
    {
        ViewData["Title"] = "Check Your Email";
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
            BuildUrl("/Auth/ResetPassword?email={0}&token={1}"));

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
    public IActionResult ResendConfirmation() => View();

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        var result = await _authService.ResendConfirmationAsync(email, BuildUrl("/Auth/ConfirmEmail?userId={0}&token={1}"));
        TempData["Success"] = result.Succeeded ? result.Message : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(ResendConfirmation));
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

    private string BuildUrl(string pathTemplate) =>
        _configuration["AppSettings:BaseUrl"]?.TrimEnd('/') + string.Format(pathTemplate, "{0}", "{1}");
}
