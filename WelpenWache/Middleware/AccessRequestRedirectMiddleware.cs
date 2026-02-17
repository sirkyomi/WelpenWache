using WelpenWache.Core.Services;

namespace WelpenWache.Middleware;

public class AccessRequestRedirectMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext context, PermissionService permissionService, AccessRequestService accessRequestService) {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var pathBase = context.Request.PathBase;
        // Exclude static files, setup page, and request-access page
        if (path.StartsWith("/_") || 
            path.StartsWith("/setup") || 
            path.StartsWith("/request-access") ||
            path.Contains(".") || // Static files (css, js, etc.)
            path.StartsWith("/reconnect") ||
            context.Request.Method != "GET") {
            await next(context);
            return;
        }

        // Only check if user is authenticated
        if (context.User.Identity is { IsAuthenticated: true }) {
            var sid = context.User.FindFirst(System.Security.Claims.ClaimTypes.PrimarySid)?.Value;
            
            if (!string.IsNullOrEmpty(sid)) {
                // Check if user has any permissions
                var hasPermissions = await permissionService.HasAnyPermissionAsync(sid);
                
                if (!hasPermissions && path != "/request-access") {
                    context.Response.Redirect(pathBase + "/request-access");
                    return;
                }
            }
        }

        await next(context);
    }
}

public static class AccessRequestRedirectMiddlewareExtensions {
    public static IApplicationBuilder UseAccessRequestRedirect(this IApplicationBuilder builder) {
        return builder.UseMiddleware<AccessRequestRedirectMiddleware>();
    }
}

