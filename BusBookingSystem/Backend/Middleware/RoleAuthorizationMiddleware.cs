using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BusBookingApp.Middleware;

public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public RoleAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Check for [Authorize] attribute
        var authorizeAttribute = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
        if (authorizeAttribute != null)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized: Authentication required." });
                return;
            }

            // Check roles if specified
            if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
            {
                var allowedRoles = authorizeAttribute.Roles.Split(',').Select(r => r.Trim());
                var userRole = context.User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(userRole) || !allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "Forbidden: Insufficient permissions." });
                    return;
                }
            }
        }

        await _next(context);
    }
}
