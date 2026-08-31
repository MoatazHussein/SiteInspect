namespace SiteInspect.Application.Features.Authentication.Common;

public sealed record AuthenticatedSession(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    AuthenticatedUser User);
