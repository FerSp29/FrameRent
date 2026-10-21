using FrameRent.Application.DTOs.Auth;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Exceptions;
using FrameRent.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FrameRent.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(
        IAuthService authService,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _authService = authService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var authResult = await _authService.LoginAsync(new LoginRequestDto(model.Email, model.Password));
            var user = await _userManager.FindByIdAsync(authResult.UserId);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            TempData["SuccessMessage"] = $"Welcome back, {authResult.Email}!";
            return RedirectToAction("Index", "Catalog");
        }
        catch (Exception ex) when (ex is DomainValidationException or ForbiddenAccessException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var authResult = await _authService.RegisterCustomerAsync(new RegisterRequestDto(model.Email, model.Password));
            var user = await _userManager.FindByIdAsync(authResult.UserId);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            TempData["SuccessMessage"] = "Registration successful! Welcome to FrameRent.";
            return RedirectToAction("Index", "Catalog");
        }
        catch (Exception ex) when (ex is DomainValidationException or ConflictException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "You have been logged out.";
        return RedirectToAction("Index", "Catalog");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
