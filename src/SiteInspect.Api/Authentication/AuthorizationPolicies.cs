using SiteInspect.Application.Common.Authorization;

namespace SiteInspect.Api.Authentication;

internal static class AuthorizationPolicies
{
    public const string InspectionReader = "InspectionReader";
    public const string InspectionManager = "InspectionManager";
    public const string InspectionExecutor = "InspectionExecutor";

    public static IServiceCollection AddSiteInspectAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(
                InspectionReader,
                policy => policy.RequireRole(RoleNames.Manager, RoleNames.Inspector))
            .AddPolicy(
                InspectionManager,
                policy => policy.RequireRole(RoleNames.Manager))
            .AddPolicy(
                InspectionExecutor,
                policy => policy.RequireRole(RoleNames.Inspector));

        return services;
    }
}
