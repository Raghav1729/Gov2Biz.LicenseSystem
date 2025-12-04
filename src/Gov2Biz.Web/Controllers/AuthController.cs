using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gov2Biz.Web.Models.Auth;
using Gov2Biz.Web.Services;
using Gov2Biz.Shared.DTOs;
using System.Security.Claims;

namespace Gov2Biz.Web.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Use database-driven authentication
            var loginRequest = new LoginRequest
            {
                Email = model.Username,
                Password = model.Password,
                TenantDomain = model.TenantDomain
            };

            var loginResponse = await _authService.LoginAsync(loginRequest);

            if (loginResponse.Success && loginResponse.User != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResponse.User.Id.ToString()),
                    new Claim(ClaimTypes.Name, loginResponse.User.Name),
                    new Claim(ClaimTypes.Email, loginResponse.User.Email),
                    new Claim(ClaimTypes.Role, loginResponse.User.Role),
                    new Claim("TenantId", loginResponse.User.TenantId),
                    new Claim("AgencyId", loginResponse.User.AgencyId),
                    new Claim("FullName", loginResponse.User.Name)
                };

                if (!string.IsNullOrEmpty(loginResponse.User.AgencyName))
                {
                    claims.Add(new Claim("AgencyName", loginResponse.User.AgencyName));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Username} logged in successfully", model.Username);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, loginResponse.Message ?? "Invalid login attempt.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out");
        return RedirectToAction("Login", "Auth");
    }
}
