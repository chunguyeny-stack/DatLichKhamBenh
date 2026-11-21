using DatLichKhamBenh.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatLichKhamBenh.Filters
{
    public class MustChangePasswordFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MustChangePasswordFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(context.HttpContext.User);
                if (user?.MustChangePassword == true)
                {
                    var path = context.HttpContext.Request.Path.Value ?? "/";
                    var controller = context.RouteData.Values["controller"]?.ToString();
                    var action = context.RouteData.Values["action"]?.ToString();

                    var isWhitelisted = string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(action, "ChangePassword", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase))
                        || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase);

                    if (!isWhitelisted)
                    {
                        context.Result = new RedirectToActionResult("ChangePassword", "Account", null);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
