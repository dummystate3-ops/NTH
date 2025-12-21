using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaToolsHub.Filters;

/// <summary>
/// Simple authentication filter for admin area
/// WARNING: This is a basic implementation for demonstration purposes.
/// Use ASP.NET Core Identity, OAuth, or JWT for production applications.
/// </summary>
public class AdminAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var isAuthenticated = session.GetString("IsAdmin") == "true";

        if (!isAuthenticated)
        {
            context.Result = new RedirectToActionResult("Login", "Admin", new { area = "Admin" });
        }
    }
}
