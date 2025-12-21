using Microsoft.AspNetCore.Mvc;

namespace NovaToolsHub.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminController : Controller
{
    private readonly IConfiguration _configuration;

    public AdminController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Admin login page
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    /// <summary>
    /// Process admin login
    /// WARNING: This is a basic authentication for demonstration.
    /// Use ASP.NET Core Identity or external auth providers in production.
    /// </summary>
    [HttpPost]
    public IActionResult Login(string username, string password, string? returnUrl = null)
    {
        var adminUsername = _configuration["AdminCredentials:Username"] ?? "admin";
        var adminPassword = _configuration["AdminCredentials:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            ViewBag.Error = "Admin password is not configured.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        if (username == adminUsername && password == adminPassword)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            HttpContext.Session.SetString("AdminUsername", username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Blog", new { area = "Admin" });
        }

        ViewBag.Error = "Invalid username or password";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    /// <summary>
    /// Admin logout
    /// </summary>
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
