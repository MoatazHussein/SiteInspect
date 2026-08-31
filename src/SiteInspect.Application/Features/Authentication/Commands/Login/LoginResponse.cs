using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Application.Features.Authentication.Commands.Login;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    AuthenticatedUser User);
