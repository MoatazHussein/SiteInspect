using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Errors;

public static class AuthorizationErrors
{
    public static readonly Error Forbidden = new(
        "Authorization.Forbidden",
        "You do not have permission to access this resource.",
        ErrorType.Forbidden);
}
