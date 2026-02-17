using WelpenWache.Core.Services;

namespace WelpenWache.Middleware;

public class SetupRedirectMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext context, SetupService setupService) {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var pathBase = context.Request.PathBase;
        
        // Exclude static files and setup page itself
        if (path.StartsWith("/_") || 
            path.StartsWith("/setup") || 
            path.Contains(".") || // Static files (css, js, etc.)
            path.StartsWith("/reconnect") ||
            context.Request.Method != "GET") {
            await next(context);
            return;
        }

        // Check if setup is required
        var setupRequired = await setupService.IsSetupRequiredAsync();
        
        if (setupRequired && path != "/setup") {
            context.Response.Redirect(pathBase + "/setup");
            return;
        }

        await next(context);
    }
}

public static class SetupRedirectMiddlewareExtensions {
    public static IApplicationBuilder UseSetupRedirect(this IApplicationBuilder builder) {
        return builder.UseMiddleware<SetupRedirectMiddleware>();
    }
}


