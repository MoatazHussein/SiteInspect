namespace SiteInspect.Api.Hosting;

internal sealed class ResponseSecurityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers.XFrameOptions = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/health"))
            {
                context.Response.Headers.CacheControl = "no-store";
            }
            else
            {
                // Revalidate browser HTTP caches; the Angular worker manages its versioned cache separately.
                context.Response.Headers.CacheControl = "no-cache";
            }
            return Task.CompletedTask;
        });
        await next(context);
    }
}
