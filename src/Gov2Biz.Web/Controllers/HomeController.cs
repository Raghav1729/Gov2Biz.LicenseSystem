using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gov2Biz.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [Authorize]
    public IActionResult Index()
    {
        var userName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "User";
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";
        var tenantId = User.FindFirst("TenantId")?.Value ?? "Unknown";

        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        ViewBag.TenantId = tenantId;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
