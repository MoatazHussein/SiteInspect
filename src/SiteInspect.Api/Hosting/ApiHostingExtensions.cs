using System.Threading.RateLimiting;
using SiteInspect.Api.ErrorHandling;

namespace SiteInspect.Api.Hosting;

internal static class ApiHostingExtensions
{
    internal static IServiceCollection AddApiHosting(this IServiceCollection services)
    {
        services.AddHttpsRedirection(options => options.HttpsPort = 443);
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await ApiResponseWriter.WriteFailureAsync(context.HttpContext,
                    StatusCodes.Status429TooManyRequests, [ApiErrors.TooManyRequests], cancellationToken);
            };
        });
        return services;
    }

    internal static void ValidateProductionSettings(this IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("Database")))
        {
            throw new InvalidOperationException("ConnectionStrings:Database is required outside Development.");
        }
        var hosts = configuration["AllowedHosts"];
        if (string.IsNullOrWhiteSpace(hosts) || hosts.Contains('*', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("AllowedHosts must name the deployment host outside Development.");
        }
        var attachments = configuration["Storage:AttachmentsPath"];
        if (string.IsNullOrWhiteSpace(attachments) || !Path.IsPathFullyQualified(attachments))
        {
            throw new InvalidOperationException("Storage:AttachmentsPath must be an absolute persistent path outside Development.");
        }
        var relativePath = Path.GetRelativePath(environment.ContentRootPath, attachments);
        if (!Path.IsPathRooted(relativePath) && relativePath != ".." &&
            !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Store attachments outside the application deployment directory.");
        }
    }
}
