using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Application.Features.Authentication.Commands.RefreshSession;

public sealed record RefreshSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    AuthenticatedUser User);
