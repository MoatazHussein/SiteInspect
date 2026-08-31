using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Errors;

public static class AuthenticationErrors
{
    public static readonly Error AuthenticationRequired = new(
        "Authentication.Required",
        "Authentication is required to access this resource.",
        ErrorType.Unauthorized);

    public static readonly Error InvalidCredentials = new(
        "Authentication.InvalidCredentials",
        "The email address or password is incorrect.",
        ErrorType.Unauthorized);

    public static readonly Error InvalidRefreshToken = new(
        "Authentication.InvalidRefreshToken",
        "The refresh session is invalid or has expired.",
        ErrorType.Unauthorized);

    public static readonly Error UserNotFound = new(
        "Authentication.UserNotFound",
        "The authenticated user no longer exists.",
        ErrorType.Unauthorized);
}
